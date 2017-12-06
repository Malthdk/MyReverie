//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2017 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

#ifndef AK_SOUNDENGINE_STUBS_H_
#define AK_SOUNDENGINE_STUBS_H_

struct AkAuxSendValueProxy : AkAuxSendValue
{
	void Set(AkGameObjectID listener, AkAuxBusID id, AkReal32 value)
	{
		listenerID = listener, auxBusID = id, fControlValue = value;
	}

	bool IsSame(AkGameObjectID listener, AkAuxBusID id)
	{
		return listenerID == listener && auxBusID == id;
	}

	static int GetSizeOf() { return sizeof(AkAuxSendValue); }
};

extern "C"
{
	AKRESULT Init(AkMemSettings* in_pMemSettings,
		AkStreamMgrSettings* in_pStmSettings,
		AkDeviceSettings* in_pDefaultDeviceSettings,
		AkInitSettings* in_pSettings,
		AkPlatformInitSettings* in_pPlatformSettings,
		AkMusicSettings* in_pMusicSettings,
		AkUInt32 in_preparePoolSizeByte);

	void Term();

	AKRESULT RegisterGameObjInternal(AkGameObjectID in_GameObj);
	AKRESULT UnregisterGameObjInternal(AkGameObjectID in_GameObj);

#ifdef AK_SUPPORT_WCHAR
	AKRESULT RegisterGameObjInternal_WithName(AkGameObjectID in_GameObj, const wchar_t* in_pszObjName);
	AKRESULT SetBasePath(const wchar_t* in_pszBasePath);
	AKRESULT SetCurrentLanguage(const wchar_t* in_pszAudioSrcPath);
	AKRESULT LoadFilePackage(const wchar_t* in_pszFilePackageName, AkUInt32 & out_uPackageID, AkMemPoolId in_memPoolID);
	AKRESULT AddBasePath(const wchar_t* in_pszBasePath);
	AKRESULT SetGameName(const wchar_t* in_GameName);
	AKRESULT SetDecodedBankPath(const wchar_t* in_DecodedPath);
	AKRESULT LoadAndDecodeBank(const wchar_t* in_pszString, bool in_bSaveDecodedBank, AkBankID& out_bankID);
	AKRESULT LoadAndDecodeBankFromMemory(void* in_BankData, AkUInt32 in_BankDataSize, bool in_bSaveDecodedBank, const wchar_t* in_DecodedBankName, bool in_bIsLanguageSpecific, AkBankID& out_bankID);
#else
	AKRESULT RegisterGameObjInternal_WithName(AkGameObjectID in_GameObj, const char* in_pszObjName);
	AKRESULT SetBasePath(const char* in_pszBasePath);
	AKRESULT SetCurrentLanguage(const char* in_pszAudioSrcPath);
	AKRESULT LoadFilePackage(const char* in_pszFilePackageName, AkUInt32 & out_uPackageID, AkMemPoolId in_memPoolID);
	AKRESULT AddBasePath(const char* in_pszBasePath);
	AKRESULT SetGameName(const char* in_GameName);
	AKRESULT SetDecodedBankPath(const char* in_DecodedPath);
	AKRESULT LoadAndDecodeBank(const char* in_pszString, bool in_bSaveDecodedBank, AkBankID& out_bankID);
	AKRESULT LoadAndDecodeBankFromMemory(void* in_BankData, AkUInt32 in_BankDataSize, bool in_bSaveDecodedBank, const char* in_DecodedBankName, bool in_bIsLanguageSpecific, AkBankID& out_bankID);
#endif

	AKRESULT UnloadFilePackage(AkUInt32 in_uPackageID);
	AKRESULT UnloadAllFilePackages();

	//Override for SetPosition to avoid filling a AkSoundPosition in C# and marshal that. 
	AKRESULT SetObjectPosition(AkGameObjectID in_GameObjectID,
		AkReal32 PosX, AkReal32 PosY, AkReal32 PosZ,
		AkReal32 FrontX, AkReal32 FrontY, AkReal32 FrontZ,
		AkReal32 TopX, AkReal32 TopY, AkReal32 TopZ)
	{
		if (!AK::SoundEngine::IsInitialized())
			return AK_Fail;

		AkSoundPosition transform;
		transform.Set(PosX, PosY, PosZ,
			FrontX, FrontY, FrontZ,
			TopX, TopY, TopZ);

		return AK::SoundEngine::SetPosition(in_GameObjectID, transform);
	}

	AKRESULT GetSourceMultiplePlayPositions(
		AkPlayingID		in_PlayingID,				///< Playing ID returned by AK::SoundEngine::PostEvent()
		AkUniqueID *	out_audioNodeID,			///< Audio Node IDs of sources associated with the specified playing ID. Indexes in this array match indexes in out_msTime.
		AkUniqueID *	out_mediaID,				///< Media ID of playing item. (corresponds to 'ID' attribute of 'File' element in SoundBank metadata file)
		AkTimeMs *		out_msTime,					///< Audio positions of sources associated with the specified playing ID. Indexes in this array match indexes in out_audioNodeID.
		AkUInt32 *		io_pcPositions,				///< Number of entries in out_puPositions. Needs to be set to the size of the array: it is adjusted to the actual number of returned entries
		bool			in_bExtrapolate = true		///< Position is extrapolated based on time elapsed since last sound engine update
		)
	{
		if (*io_pcPositions == 0)
		{
			return AK_Fail;
		}

		AkSourcePosition * sourcePositionInfo = (AkSourcePosition*)AkAlloca((*io_pcPositions) * sizeof(AkSourcePosition));
		if (!sourcePositionInfo)
		{
			return AK_Fail;
		}

		AKRESULT res = AK::SoundEngine::GetSourcePlayPositions(in_PlayingID, sourcePositionInfo, io_pcPositions, in_bExtrapolate);

		for (AkUInt32 i = 0; i < *io_pcPositions; ++i)
		{
			out_audioNodeID[i] = sourcePositionInfo[i].audioNodeID;
			out_mediaID[i] = sourcePositionInfo[i].mediaID;
			out_msTime[i] = sourcePositionInfo[i].msTime;
		}

		return res;
	}

	AKRESULT SetListeners(AkGameObjectID in_emitterGameObj, AkGameObjectID* in_pListenerGameObjs, AkUInt32 in_uNumListeners)
	{
		return AK::SoundEngine::SetListeners(in_emitterGameObj, in_pListenerGameObjs, in_uNumListeners);
	}

	AKRESULT SetDefaultListeners(AkGameObjectID* in_pListenerObjs, AkUInt32 in_uNumListeners)
	{
		return AK::SoundEngine::SetDefaultListeners(in_pListenerObjs, in_uNumListeners);
	}

	void GetDefaultStreamSettings(AkStreamMgrSettings & out_settings)
	{
		AK::StreamMgr::GetDefaultSettings(out_settings);
	}

	void GetDefaultDeviceSettings(AkDeviceSettings & out_settings)
	{
		AK::StreamMgr::GetDefaultDeviceSettings(out_settings);
	}

	void GetDefaultMusicSettings(AkMusicSettings &out_settings)
	{
		AK::MusicEngine::GetDefaultInitSettings(out_settings);
	}

	void GetDefaultInitSettings(AkInitSettings & out_settings)
	{
		AK::SoundEngine::GetDefaultInitSettings(out_settings);
	}

	void GetDefaultPlatformInitSettings(AkPlatformInitSettings &out_settings)
	{
		AK::SoundEngine::GetDefaultPlatformInitSettings(out_settings);
	}

	AkUInt32 GetMajorMinorVersion()
	{
		return (AK_WWISESDK_VERSION_MAJOR << 16) | AK_WWISESDK_VERSION_MINOR;
	}

	AkUInt32 GetSubminorBuildVersion()
	{
		return (AK_WWISESDK_VERSION_SUBMINOR << 16) | AK_WWISESDK_VERSION_BUILD;
	}

#ifdef AK_WIN
	AkUInt32 GetDeviceIDFromName(wchar_t* in_szToken)
	{
		return AK::GetDeviceIDFromName(in_szToken);
	}

	const wchar_t* GetWindowsDeviceName(AkInt32 index, AkUInt32 &out_uDeviceID)
	{
		return AK::GetWindowsDeviceName(index, out_uDeviceID);
	}
#endif
}

#endif //AK_SOUNDENGINE_STUBS_H_
