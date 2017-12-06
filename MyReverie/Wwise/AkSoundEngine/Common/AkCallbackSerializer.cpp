#include "stdafx.h"
#include "AkCallbackSerializer.h"
#include <AK/Tools/Common/AkLock.h>
#include <AK/Tools/Common/AkAutoLock.h>
#include <stdio.h>
#include "ExtraCallbacks.h"
#include <AK/Tools/Common/AkAssert.h>

CAkLock g_Lock;

AkSerializedCallbackHeader* m_pLockedPtr = NULL;
AkSerializedCallbackHeader** m_pLastNextPtr = NULL;
AkSerializedCallbackHeader* m_pNextAvailable = NULL;
AkSerializedCallbackHeader* m_pFirst = NULL;
AkEvent m_DrainEvent;
AkSerializedCallbackHeader* m_pBlockStart = NULL;
void* m_pBlockEnd = NULL;
AkThreadID m_idThread = 0;


template<typename InfoStruct>
InfoStruct* AllocNewStruct(bool in_bCritical, void* pPackage, AkUInt32 eType, AkUInt32 uStringLength = 0)
{
	AkUInt32 uItemSize = sizeof(AkSerializedCallbackHeader) + sizeof(InfoStruct) + uStringLength;

	// Ensure 32bit alignment
	uItemSize = ((uItemSize + 3) / 4) * 4;

	//If the current thread is the main thread that normally pumps the messages, then don't block if the buffer is full!
	in_bCritical = in_bCritical && m_idThread != AKPLATFORM::CurrentThread();

retry:
	void* pItemEnd = (char*)m_pNextAvailable + uItemSize;
	void* pReadPtr = m_pLockedPtr != NULL ? m_pLockedPtr : m_pFirst;

	if (m_pBlockStart == NULL || m_pBlockEnd == NULL || m_pNextAvailable == NULL)
	{
		AKPLATFORM::OutputDebugMsg("AkCallbackSerializer::AllocNewCall: Callback serializer terminated but still received event calls. Abort.\n");
		return NULL;
	}

	//Is there enough space between the write head and the end of the buffer?
	if (pItemEnd >= m_pBlockEnd)
	{
		//Nope, need to wrap around
		//But is the read ptr in the way?
		if (pReadPtr > m_pNextAvailable)
		{
			//Queue is full, wait for the game to empty it to avoid losing information.
			if (in_bCritical)
			{
				g_Lock.Unlock();
				AKPLATFORM::AkWaitForEvent(m_DrainEvent);
				g_Lock.Lock();
				goto retry;
			}
			else
				return NULL;	//No memory for that, and we can't block the main game thread.  Expected if the ErrorLevel is set to ALL.
		}

		m_pNextAvailable = m_pBlockStart;
		pItemEnd = (char*)m_pNextAvailable + uItemSize;
	}

	//Is there enough space up to the read pointer?
	if (m_pNextAvailable == pReadPtr || (m_pNextAvailable < pReadPtr && pItemEnd >= pReadPtr))
	{
		//Nope!  Queue is full, wait for the game to empty it to avoid losing information.
		if (in_bCritical)
		{
			g_Lock.Unlock();
			AKPLATFORM::AkWaitForEvent(m_DrainEvent);
			g_Lock.Lock();
			goto retry;
		}
		else
			return NULL;	//No memory for that, and we can't block the main game thread.  Expected if the ErrorLevel is set to ALL.
	}

	//Link the new item in the list.
	if (m_pFirst == NULL)
		m_pFirst = m_pNextAvailable;
	else
		*m_pLastNextPtr = m_pNextAvailable;

	m_pLastNextPtr = &(m_pNextAvailable->pNext);

	AkSerializedCallbackHeader* pHeader = m_pNextAvailable;
	m_pNextAvailable = (AkSerializedCallbackHeader*)pItemEnd;

	pHeader->pPackage = pPackage;
	pHeader->pNext = NULL;
	pHeader->eType = (AkCallbackType)eType;

	return (InfoStruct*)(pHeader + 1);
}

void LocalOutput(AK::Monitor::ErrorCode in_eErrorCode, const AkOSChar* in_pszError, AK::Monitor::ErrorLevel in_eErrorLevel, AkPlayingID in_playingID, AkGameObjectID in_gameObjID)
{
	//Ak_Monitoring isn't defined on the regular SDK.  It's a modification that only the C# side sees.
	const AkUInt32 uLength = (AkUInt32)AKPLATFORM::OsStrLen(in_pszError) + 1;

	AkAutoLock<CAkLock> autoLock(g_Lock);
	AkSerializedMonitoringCallbackInfo* info = AllocNewStruct<AkSerializedMonitoringCallbackInfo>(false, NULL, AK_Monitoring_Val, uLength * sizeof(AkOSChar));
	if (info != NULL)
	{
		info->errorCode = in_eErrorCode;
		info->errorLevel = in_eErrorLevel;
		info->playingID = in_playingID;
		info->gameObjID = in_gameObjID;

		if (uLength)
		{
			AKPLATFORM::SafeStrCpy(info->message, in_pszError, uLength);
		}
		info->message[uLength] = 0;
	}
	else
	{
		//No space, can't log this.  Expected if the ErrorLevel is set to ALL as some logging is done on the game thread
	}
}


AKRESULT AkCallbackSerializer::Init(void * in_pMemory, AkUInt32 in_uSize)
{
	if (m_pBlockStart == NULL && in_pMemory != NULL)
	{
		m_pNextAvailable = m_pBlockStart = (AkSerializedCallbackHeader*)in_pMemory;
		m_pBlockEnd = (char*)in_pMemory + in_uSize;
		AKPLATFORM::AkCreateEvent(m_DrainEvent);
		m_idThread = AKPLATFORM::CurrentThread();
	}
	return AK_Success;
}

void AkCallbackSerializer::Term()
{
	AkAutoLock<CAkLock> autoLock(g_Lock);

	if (m_pBlockStart != NULL)
	{
		AKPLATFORM::AkSignalEvent(m_DrainEvent);
		AKPLATFORM::AkDestroyEvent(m_DrainEvent);
		m_pNextAvailable = m_pBlockStart = NULL;
		m_pBlockEnd = NULL;
	}

	AK::Monitor::SetLocalOutput();
}

void AkCallbackSerializer::EventCallback(AkCallbackType in_eType, AkCallbackInfo* in_pCallbackInfo)
{
	if (in_pCallbackInfo == NULL)
	{
		//There wasn't enough memory to store the callback when the user registered it.  We don't know where to call to.
		return;
	}

	AkAutoLock<CAkLock> autoLock(g_Lock);

	switch (in_eType)
	{
	case AK_EndOfEvent:
	case AK_Starvation:
	case AK_MusicPlayStarted:
	{
		const AkEventCallbackInfo* copyFrom = (AkEventCallbackInfo*)in_pCallbackInfo;
		AkSerializedEventCallbackInfo* info = AllocNewStruct<AkSerializedEventCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;
		}
		break;
	}
	case AK_EndOfDynamicSequenceItem:
	{
		const AkDynamicSequenceItemCallbackInfo* copyFrom = (AkDynamicSequenceItemCallbackInfo*)in_pCallbackInfo;
		AkSerializedDynamicSequenceItemCallbackInfo* info = AllocNewStruct<AkSerializedDynamicSequenceItemCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->audioNodeID = copyFrom->audioNodeID;
			info->pCustomInfo = copyFrom->pCustomInfo;
		}
		break;
	}
	case AK_Marker:
	{
		const AkMarkerCallbackInfo* copyFrom = (AkMarkerCallbackInfo*)in_pCallbackInfo;
		const AkUInt32 uLength = copyFrom->strLabel != NULL ? (AkUInt32)strlen(copyFrom->strLabel) : 0;

		AkSerializedMarkerCallbackInfo* info = AllocNewStruct<AkSerializedMarkerCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, uLength);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;
			info->uIdentifier = copyFrom->uIdentifier;
			info->uPosition = copyFrom->uPosition;

			if (uLength)
			{
				memcpy(info->strLabel, copyFrom->strLabel, uLength);
			}
			info->strLabel[uLength] = 0;
		}
		break;
	}
	case AK_Duration:
	{
		const AkDurationCallbackInfo* copyFrom = (AkDurationCallbackInfo*)in_pCallbackInfo;
		AkSerializedDurationCallbackInfo* info = AllocNewStruct<AkSerializedDurationCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;
			info->fDuration = copyFrom->fDuration;
			info->fEstimatedDuration = copyFrom->fEstimatedDuration;
			info->audioNodeID = copyFrom->audioNodeID;
			info->mediaID = copyFrom->mediaID;
			info->bStreaming = copyFrom->bStreaming;
		}
		break;
	}
	case AK_MIDIEvent:
	{
		const AkMIDIEventCallbackInfo* copyFrom = (AkMIDIEventCallbackInfo*)in_pCallbackInfo;
		AkSerializedMIDIEventCallbackInfo* info = AllocNewStruct<AkSerializedMIDIEventCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;

			info->byType = copyFrom->midiEvent.byType;
			info->byChan = copyFrom->midiEvent.byChan;
			info->byParam1 = copyFrom->midiEvent.Gen.byParam1;
			info->byParam2 = copyFrom->midiEvent.Gen.byParam2;
		}
		break;
	}
	case AK_MusicSyncBeat:
	case AK_MusicSyncBar:
	case AK_MusicSyncEntry:
	case AK_MusicSyncExit:
	case AK_MusicSyncGrid:
	case AK_MusicSyncPoint:
	{
		const AkMusicSyncCallbackInfo* copyFrom = (AkMusicSyncCallbackInfo*)in_pCallbackInfo;
		AkSerializedMusicSyncCallbackInfo* info = AllocNewStruct<AkSerializedMusicSyncCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;

			info->segmentInfo_iCurrentPosition = copyFrom->segmentInfo.iCurrentPosition;
			info->segmentInfo_iPreEntryDuration = copyFrom->segmentInfo.iPreEntryDuration;
			info->segmentInfo_iActiveDuration = copyFrom->segmentInfo.iActiveDuration;
			info->segmentInfo_iPostExitDuration = copyFrom->segmentInfo.iPostExitDuration;
			info->segmentInfo_iRemainingLookAheadTime = copyFrom->segmentInfo.iRemainingLookAheadTime;
			info->segmentInfo_fBeatDuration = copyFrom->segmentInfo.fBeatDuration;
			info->segmentInfo_fBarDuration = copyFrom->segmentInfo.fBarDuration;
			info->segmentInfo_fGridDuration = copyFrom->segmentInfo.fGridDuration;
			info->segmentInfo_fGridOffset = copyFrom->segmentInfo.fGridOffset;

			info->musicSyncType = copyFrom->musicSyncType;
			info->userCueName[0] = 0;
		}
		break;
	}
	case AK_MusicSyncUserCue:
	{
		const AkMusicSyncCallbackInfo* copyFrom = (AkMusicSyncCallbackInfo*)in_pCallbackInfo;
		const AkUInt32 uLength = copyFrom->pszUserCueName ? (AkUInt32)strlen(copyFrom->pszUserCueName) : 0;

		AkSerializedMusicSyncCallbackInfo* info = AllocNewStruct<AkSerializedMusicSyncCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, uLength);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;

			info->segmentInfo_iCurrentPosition = copyFrom->segmentInfo.iCurrentPosition;
			info->segmentInfo_iPreEntryDuration = copyFrom->segmentInfo.iPreEntryDuration;
			info->segmentInfo_iActiveDuration = copyFrom->segmentInfo.iActiveDuration;
			info->segmentInfo_iPostExitDuration = copyFrom->segmentInfo.iPostExitDuration;
			info->segmentInfo_iRemainingLookAheadTime = copyFrom->segmentInfo.iRemainingLookAheadTime;
			info->segmentInfo_fBeatDuration = copyFrom->segmentInfo.fBeatDuration;
			info->segmentInfo_fBarDuration = copyFrom->segmentInfo.fBarDuration;
			info->segmentInfo_fGridDuration = copyFrom->segmentInfo.fGridDuration;
			info->segmentInfo_fGridOffset = copyFrom->segmentInfo.fGridOffset;

			info->musicSyncType = copyFrom->musicSyncType;

			if (uLength)
			{
				memcpy(info->userCueName, copyFrom->pszUserCueName, uLength);
			}
			info->userCueName[uLength] = 0;
		}
		break;
	}
	case AK_MusicPlaylistSelect:
	{
		const AkMusicPlaylistCallbackInfo* copyFrom = (AkMusicPlaylistCallbackInfo*)in_pCallbackInfo;
		AkSerializedMusicPlaylistCallbackInfo* info = AllocNewStruct<AkSerializedMusicPlaylistCallbackInfo>(true, in_pCallbackInfo->pCookie, in_eType, 0);
		if (info != NULL)
		{
			info->pCookie = copyFrom->pCookie;
			info->gameObjID = copyFrom->gameObjID;
			info->playingID = copyFrom->playingID;
			info->eventID = copyFrom->eventID;

			info->playlistID = copyFrom->playlistID;
			info->uNumPlaylistItems = copyFrom->uNumPlaylistItems;
			info->uPlaylistSelection = copyFrom->uPlaylistSelection;
			info->uPlaylistItemDone = copyFrom->uPlaylistItemDone;
		}
		break;
	}
	default:
		break;
	}
}

void* AkCallbackSerializer::Lock()
{
	AkAutoLock<CAkLock> autoLock(g_Lock);
	AkSerializedCallbackHeader* pRead = NULL;
	if (m_pFirst != NULL)
	{
		//Terminate the linked list.
		*m_pLastNextPtr = NULL;
		m_pLastNextPtr = NULL;	
		pRead = m_pLockedPtr = m_pFirst;
		m_pFirst = NULL;
	}
	return pRead;
}	

void AkCallbackSerializer::Unlock()
{
	{
		AkAutoLock<CAkLock> autoLock(g_Lock);
		m_pLockedPtr = NULL;
	}
	AKPLATFORM::AkSignalEvent(m_DrainEvent);
}

void AkCallbackSerializer::SetLocalOutput( AkUInt32 in_uErrorLevel )
{
	AK::Monitor::SetLocalOutput(in_uErrorLevel, (in_uErrorLevel != 0) ? (AK::Monitor::LocalOutputFunc)LocalOutput : NULL);
}

void AkCallbackSerializer::BankCallback(AkUInt32 in_bankID, void* in_pInMemoryBankPtr, AKRESULT in_eLoadResult, AkMemPoolId in_memPoolId, void *in_pCookie)
{
	if (in_pCookie == NULL)
	{
		//There wasn't enough memory to store the callback when the user registered it.  We don't know where to call to.
		return;
	}

	// Ak_Bank_Val is a customization only for C#.
	AkAutoLock<CAkLock> autoLock(g_Lock);
	AkSerializedBankCallbackInfo* info = AllocNewStruct<AkSerializedBankCallbackInfo>(false, in_pCookie, AK_Bank_Val, 0);
	if (info != NULL)
	{
		info->bankID = in_bankID;
		info->inMemoryBankPtr = in_pInMemoryBankPtr;
		info->loadResult = in_eLoadResult;
		info->memPoolId = in_memPoolId;
	}
}

AKRESULT AkCallbackSerializer::AudioInterruptionCallbackFunc(bool in_bEnterInterruption, void* in_pCookie)
{
	// AK_AudioInterruption_Val is a customization only for C#.
	AkAutoLock<CAkLock> autoLock(g_Lock);
	AkSerializedAudioInterruptionCallbackInfo* info = AllocNewStruct<AkSerializedAudioInterruptionCallbackInfo>(true, in_pCookie, AK_AudioInterruption_Val, 0);
	if (info == NULL)
		return AK_Fail;

	info->bEnterInterruption = in_bEnterInterruption;
	return AK_Success;
}

AKRESULT AkCallbackSerializer::AudioSourceChangeCallbackFunc(bool in_bOtherAudioPlaying, void* in_pCookie)
{
	// On iOS, this user callback is triggered by the initial WakeupFromSuspend() call
	// This is before the sound engine is initialized. 
	// Bypass this call.
	if (m_pBlockStart == NULL || m_pBlockEnd == NULL || m_pNextAvailable == NULL)
		return AK_Cancelled;

	// AK_AudioSourceChange_Val is a customization only for C#.
	AkAutoLock<CAkLock> autoLock(g_Lock);
	AkSerializedAudioSourceChangeCallbackInfo* info = AllocNewStruct<AkSerializedAudioSourceChangeCallbackInfo>(true, in_pCookie, AK_AudioSourceChange_Val, 0);
	if (info == NULL)
		return AK_Fail;

	info->bOtherAudioPlaying = in_bOtherAudioPlaying;
	return AK_Success;
}
