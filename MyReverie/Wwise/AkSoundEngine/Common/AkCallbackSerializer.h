
#include <AK/SoundEngine/Common/AkCallback.h>
#include <AK/Tools/Common/AkMonitorError.h>

#include "SwigExceptionSwitch.h"


PAUSE_SWIG_EXCEPTIONS
#ifdef SWIG
%feature("immutable");
#endif // SWIG

struct AkSerializedCallbackHeader
{
	void* pPackage; //The C# CallbackPackage to return to C#
	AkSerializedCallbackHeader* pNext; //Pointer to the next callback
	AkCallbackType eType; //The type of structure following, generally an enumerate of AkCallbackType

	void* GetData() const { return (void*)(this + 1); }
};

struct AkSerializedCallbackInfo
{
	void* pCookie; ///< User data, passed to PostEvent()
	AkGameObjectID gameObjID; ///< Game object ID
};

struct AkSerializedEventCallbackInfo : AkSerializedCallbackInfo
{
	AkPlayingID		playingID;		///< Playing ID of Event, returned by PostEvent()
	AkUniqueID		eventID;		///< Unique ID of Event, passed to PostEvent()
};

struct AkSerializedMIDIEventCallbackInfo : AkSerializedEventCallbackInfo
{
	// AkMIDIEvent expanded to prevent packing issues
	// BEGIN_AKMIDIEVENT_EXPANSION

	AkUInt8 byType; // (Ak_MIDI_EVENT_TYPE_)
	AkMidiChannelNo byChan;

	AkUInt8		byParam1;
	AkUInt8		byParam2;

#if SWIG
	%extend
	{
		AkMidiNoteNo	byOnOffNote;
		AkUInt8			byVelocity;

		AkUInt8		byCc;
		AkUInt8		byCcValue;

		AkUInt8		byValueLsb;
		AkUInt8		byValueMsb;

		AkUInt8		byAftertouchNote;
		AkUInt8		byNoteAftertouchValue;

		AkUInt8		byChanAftertouchValue;

		AkUInt8		byProgramNum;
	}
#endif // SWIG

	// END_AKMIDIEVENT_EXPANSION
};

#if SWIG
%{
	AkMidiNoteNo AkSerializedMIDIEventCallbackInfo_byOnOffNote_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return (AkMidiNoteNo)info->byParam1;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byVelocity_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam2;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byCc_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam1;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byCcValue_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam2;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byValueLsb_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam1;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byValueMsb_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam2;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byAftertouchNote_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam1;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byNoteAftertouchValue_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam2;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byChanAftertouchValue_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam1;
	}

	AkUInt8 AkSerializedMIDIEventCallbackInfo_byProgramNum_get(AkSerializedMIDIEventCallbackInfo *info)
	{
		return info->byParam1;
	}
%}
#endif // SWIG

struct AkSerializedMarkerCallbackInfo : AkSerializedEventCallbackInfo
{
	AkUInt32	uIdentifier;		///< Cue point identifier
	AkUInt32	uPosition;			///< Position in the cue point (unit: sample frames)
	char		strLabel[1];		///< Label of the marker, read from the file
};

struct AkSerializedDurationCallbackInfo : AkSerializedEventCallbackInfo
{
	AkReal32	fDuration;				///< Duration of the sound (unit: milliseconds)
	AkReal32	fEstimatedDuration;		///< Estimated duration of the sound depending on source settings such as pitch. (unit: milliseconds)
	AkUniqueID	audioNodeID;			///< Audio Node ID of playing item
	AkUniqueID  mediaID;				///< Media ID of playing item. (corresponds to 'ID' attribute of 'File' element in SoundBank metadata file)
	bool bStreaming;				///< True if source is streaming, false otherwise.
};

struct AkSerializedDynamicSequenceItemCallbackInfo : AkSerializedCallbackInfo
{
	AkPlayingID playingID;			///< Playing ID of Dynamic Sequence, returned by AK::SoundEngine:DynamicSequence::Open()
	AkUniqueID	audioNodeID;		///< Audio Node ID of finished item
	void*		pCustomInfo;		///< Custom info passed to the DynamicSequence::Open function
};

struct AkSerializedMusicSyncCallbackInfo : AkSerializedCallbackInfo
{
	AkPlayingID playingID;			///< Playing ID of Event, returned by PostEvent()
	//AkSegmentInfo segmentInfo; ///< Segment information corresponding to the segment triggering this callback.

	// AkSegmentInfo expanded to prevent packing issues
	// BEGIN_AKSEGMENTINFO_EXPANSION
	AkTimeMs		segmentInfo_iCurrentPosition;		///< Current position of the segment, relative to the Entry Cue, in milliseconds. Range is [-iPreEntryDuration, iActiveDuration+iPostExitDuration].
	AkTimeMs		segmentInfo_iPreEntryDuration;		///< Duration of the pre-entry region of the segment, in milliseconds.
	AkTimeMs		segmentInfo_iActiveDuration;		///< Duration of the active region of the segment (between the Entry and Exit Cues), in milliseconds.
	AkTimeMs		segmentInfo_iPostExitDuration;		///< Duration of the post-exit region of the segment, in milliseconds.
	AkTimeMs		segmentInfo_iRemainingLookAheadTime;///< Number of milliseconds remaining in the "looking-ahead" state of the segment, when it is silent but streamed tracks are being prefetched.
	AkReal32		segmentInfo_fBeatDuration;			///< Beat Duration in seconds.
	AkReal32		segmentInfo_fBarDuration;			///< Bar Duration in seconds.
	AkReal32		segmentInfo_fGridDuration;			///< Grid duration in seconds.
	AkReal32		segmentInfo_fGridOffset;			///< Grid offset in seconds.
	// END_AKSEGMENTINFO_EXPANSION

	AkCallbackType musicSyncType;	///< Would be either AK_MusicSyncEntry, AK_MusicSyncBeat, AK_MusicSyncBar, AK_MusicSyncExit, AK_MusicSyncGrid, AK_MusicSyncPoint or AK_MusicSyncUserCue.
	char  userCueName[1];
};

struct AkSerializedMusicPlaylistCallbackInfo : public AkSerializedEventCallbackInfo
{
	AkUniqueID playlistID;			///< ID of playlist node
	AkUInt32 uNumPlaylistItems;		///< Number of items in playlist node (may be segments or other playlists)
	AkUInt32 uPlaylistSelection;	///< Selection: set by sound engine, modified by callback function (if not in range 0 <= uPlaylistSelection < uNumPlaylistItems then ignored).
	AkUInt32 uPlaylistItemDone;		///< Playlist node done: set by sound engine, modified by callback function (if set to anything but 0 then the current playlist item is done, and uPlaylistSelection is ignored)
};

struct AkSerializedBankCallbackInfo
{
	AkUInt32 bankID;
	void* inMemoryBankPtr; // changed from AkUIntPtr to 'void*'
	AKRESULT loadResult;
	AkMemPoolId memPoolId;
};

struct AkSerializedMonitoringCallbackInfo
{
	AK::Monitor::ErrorCode errorCode;
	AK::Monitor::ErrorLevel errorLevel;
	AkPlayingID playingID;
	AkGameObjectID gameObjID;
	AkOSChar message[1];
};

struct AkSerializedAudioInterruptionCallbackInfo
{
	bool bEnterInterruption;
};

struct AkSerializedAudioSourceChangeCallbackInfo
{
	bool bOtherAudioPlaying;
};

#ifdef SWIG
%feature("immutable", "");
#endif // SWIG
RESUME_SWIG_EXCEPTIONS

/// This class allows the Sound Engine callbacks to be processed in the user thread instead of the audio thread.
/// This is done by accumulating the callback generating events until the game calls CallbackSerializer::PostCallbacks().
/// All pending callbacks will be done at that point.  This removes the need for external synchronization for the callback
/// functions that the game would need otherwise.
class AkCallbackSerializer
{
public:
	static AKRESULT Init(void * in_pMemory, AkUInt32 in_uSize);

	PAUSE_SWIG_EXCEPTIONS
	static void Term();
	RESUME_SWIG_EXCEPTIONS

	static void* Lock();

	static void SetLocalOutput(AkUInt32 in_uErrorLevel);

	static void Unlock();

	static void EventCallback(AkCallbackType in_eType, AkCallbackInfo* in_pCallbackInfo);
	static void BankCallback(AkUInt32 in_bankID, void* in_pInMemoryBankPtr, AKRESULT in_eLoadResult, AkMemPoolId in_memPoolId, void *in_pCookie);

	static AKRESULT AudioInterruptionCallbackFunc(bool in_bEnterInterruption, void* in_pCookie);

	static AKRESULT AudioSourceChangeCallbackFunc(bool in_bOtherAudioPlaying, void* in_pCookie);
};
