//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

%module AkSoundEngine	//Names the C# namespace.  Must be the same thing as the name of the DLL.
%csconst(1);		//Generate static const variables in C# for all defines and const variables seen on the C++ side.

%include "../../SWIG/Lib/csharp/wchar.i"				//Treat wchar* as strings in C#.
%include "../../SWIG/Lib/csharp/typemaps.i"

//Remove warning about duplicate functions because of char and wchar use in the interface.
#pragma SWIG nowarn=516
#pragma SWIG nowarn=844

// Translate custom SWIG command defines into AK defines.
// Order: Must be before any dependent #ifs
// For compatibility with Wwise 2012.2 where <AK/AkPlatforms.h> no longer contains importable defines.
#ifdef NN_PLATFORM_CTR
	#define AK_3DS
#elif defined( _XBOX_ONE )
	#define AK_XBOXONE
	#define AK_SUPPORT_WCHAR
#elif defined( WIN32 ) || defined ( WIN64 ) || defined( WINAPI_FAMILY )
	#define AK_WIN
#elif defined( __APPLE__ )
	#define AK_APPLE 
#elif defined( __ANDROID__ )
	#define AK_ANDROID
#elif defined( __native_client__ )
	#define AK_NGP
#elif defined( __SCE__ ) && defined( __arm__ )
	#define AK_VITA
	#define AK_SUPPORT_WCHAR
#elif defined(__ORBIS__)
	#define AK_PS4
#elif defined(__linux__)
	#define AK_LINUX
#endif

// Disable warnings that appear in the SWIG generated code
#if defined (AK_ANDROID)
%{
#pragma GCC diagnostic ignored "-Wuninitialized"
#pragma GCC diagnostic ignored "-Wmaybe-uninitialized"
%}
#elif defined(AK_XBOXONE)
%{
// Disable "unreferenced formal parameter" (4100) and "potentially uninitialized" (4701) warnings.
#pragma warning(disable: 4100)
#pragma warning(disable: 4701)
%}
#elif defined (AK_NX)
%{
#pragma GCC diagnostic ignored "-Wsometimes-uninitialized"
%}
#endif // #if defined (AK_NX)


%ignore AkEmitterListenerPair;
%ignore AkPolarCoord;
%ignore AkSphericalCoord;
%ignore AkListener;
%ignore AkSourcePosition;

//This typemap will map the "void*" to a IntPtr on the C# side.
// Order: Must be before %include <AK/SoundEngine/Common/AkTypes.h>
	%typemap(ctype) void * "void *"
	%typemap(imtype) void * "IntPtr"
	%typemap(cstype) void * "IntPtr"
	%typemap(csin) void * "$csinput"
	%typemap(in) void * %{ $1 = $input; %}
	%typemap(out) void * %{ $result = $1; %}
	%typemap(csout) void * { return $imcall; }
	%typemap(csvarin, excode=SWIGEXCODE2)  void* %{ set { $imcall;$excode } %}
	%typemap(csvarout, excode=SWIGEXCODE2)  void* %{ get { return $imcall;$excode }%}

	%typemap(ctype) void** "void**"
	%typemap(imtype) void** "IntPtr[]"
	%typemap(cstype) void** "IntPtr[]"
	%typemap(csin) void** "$csinput"
	%typemap(in) void** %{ $1 = $input; %}
	%typemap(out) void** %{ $result = $1; %}
	%typemap(csout) void** { return $imcall; }
	%typemap(csvarin, excode=SWIGEXCODE2)  void** %{ set { $imcall;$excode } %}
	%typemap(csvarout, excode=SWIGEXCODE2)  void** %{ get { return $imcall;$excode }%}

//This typemap will map the "AkUInt8*" to a IntPtr on the C# side.
// Order: Must be before %include <AK/SoundEngine/Common/AkTypes.h>
	%typemap(ctype) AkUInt8 * "AkUInt8 *"
	%typemap(imtype) AkUInt8 * "IntPtr"
	%typemap(cstype) AkUInt8 * "IntPtr"
	%typemap(csin) AkUInt8 * "$csinput"
	%typemap(in) AkUInt8 * %{ $1 = $input; %}
	%typemap(out) AkUInt8 * %{ $result = $1; %}
	%typemap(csout) AkUInt8 * { return $imcall; }
	%typemap(csvarin, excode=SWIGEXCODE2)  AkUInt8* %{ set { $imcall;$excode } %}
	%typemap(csvarout, excode=SWIGEXCODE2)  AkUInt8* %{ get { return $imcall;$excode }%}
	
// Order: Must be before %include <AK/SoundEngine/Platforms/Windows/AkTypes.h>, for e.g., _w64
#if defined(AK_WIN) || defined(AK_XBOXONE)
	%include <windows.i>
#endif // #if defined(AK_WIN) || defined(AK_XBOXONE)

// Order: Must be before %include <AK/SoundEngine/Common/AkTypes.h>
#if defined( AK_3DS )
	%include <AK/SoundEngine/Platforms/3DS/AkTypes.h>
#elif defined( AK_XBOXONE )
	%include <AK/SoundEngine/Platforms/XBoxOne/AkTypes.h>
#elif defined( AK_WIN )
	%ignore AK::VirtualAllocHook;
	%ignore AK::VirtualFreeHook;
	%include <AK/SoundEngine/Platforms/Windows/AkTypes.h>
#if defined (AK_WIN_UNIVERSAL_APP)
	// Ripped from winnt.h in the Windows 10 SDK, in order to avoid SWIGTYPE when building AkThreadProperties struct.
	typedef struct _PROCESSOR_NUMBER {
		WORD   Group;
		BYTE  Number;
		BYTE  Reserved;
	} PROCESSOR_NUMBER;
#endif
#elif defined( AK_APPLE )
	%import <sys/cdefs.h>
	%import <sys/_types/_int8_t.h>
	%import <sys/_types/_int16_t.h>
	%import <sys/_types/_int32_t.h>
	%import <sys/_types/_int64_t.h>
	%import <sys/_types/_intptr_t.h>
	%import <sys/_types/_uintptr_t.h>
	%import <_types/_uint8_t.h>
	%import <_types/_uint16_t.h>
	%import <_types/_uint32_t.h>
	%import <_types/_uint64_t.h>
	%import <sys/_types.h>
	%import <sys/types.h>
	%import <stdint.h>
	%import <machine/types.h>
	%include <AK/SoundEngine/Platforms/Mac/AkTypes.h>
	%include <AK/SoundEngine/Platforms/POSIX/AkTypes.h>
#elif defined ( AK_PS4 )
	%include <PS4_types.h>
	%include <AK/SoundEngine/Platforms/PS4/AkTypes.h>
#elif defined( AK_VITA )
	%include <AK/SoundEngine/Platforms/Vita/AkTypes.h>
#elif defined( AK_ANDROID )
	#define	__signed signed
	// %import <sys/cdefs.h>
	%import <machine/_types.h>
	%import <sys/_types.h>
	%import <sys/types.h>
	%import <stdint.h>
	%include <AK/SoundEngine/Platforms/POSIX/AkTypes.h>
	%include <AK/SoundEngine/Platforms/Android/AkTypes.h>
#elif defined( AK_NACL )
	%include <AK/SoundEngine/Platforms/nacl/AkTypes.h>
#elif defined( AK_LINUX )
	%include <stdint.i>
	%include <AK/SoundEngine/Platforms/POSIX/AkTypes.h>
	%include <AK/SoundEngine/Platforms/Linux/AkTypes.h>
	#undef AK_SUPPORT_WCHAR
#elif defined( AK_NX )
	// The following defines are taken from stdint.h
	#define __NEED_int8_t
	#define __NEED_int16_t
	#define __NEED_int32_t
	#define __NEED_int64_t

	#define __NEED_uint8_t
	#define __NEED_uint16_t
	#define __NEED_uint32_t
	#define __NEED_uint64_t

	#define __NEED_intptr_t
	#define __NEED_uintptr_t

	#define __NEED_intmax_t
	#define __NEED_uintmax_t

	%include <bits/alltypes.h>

	// For Bit64
	%import <nn/nn_BitTypes.h>

	%ignore AkThread;
	%ignore AK::AlignedAllocHook;
	%ignore AK::AlignedFreeHook;
	%include <AK/SoundEngine/Platforms/NX/AkTypes.h>
	#undef AK_SUPPORT_WCHAR
#endif

//Transform calls with AkOSChar properly.  On the C# side, they are simple strings.  They transfer as Unicode to C++ (Windows), then converted to the proper native type.
// Order: Must be before %include <AK/SoundEngine/Common/AkTypes.h>, for e.g., AkExternalSourceInfo::szFile
	%typemap(cstype) AkOSChar* "string"
	%typemap(csin) AkOSChar* "$csinput"
	%typemap(csout) AkOSChar* { return $imcall; }
	%typemap(csvarin, excode=SWIGEXCODE2)  AkOSChar* %{	set { $imcall;$excode } %}
	%typemap(csvarout, excode=SWIGEXCODE2)  AkOSChar* %{ get { return $imcall;$excode } %}
#if defined(AK_WIN) || defined(AK_XBOXONE) || defined (AK_VITA)
	%typemap(ctype) AkOSChar* "wchar_t*"
	%typemap(imtype, inattributes="[MarshalAs(UnmanagedType.LPWStr)]") AkOSChar* "string"
	%typemap(in) AkOSChar* %{ CONVERT_WIDE_TO_OSCHAR($input, $1); %}
	%typemap(out) AkOSChar* %{ CONVERT_OSCHAR_TO_WIDE($1, $result); %}
#else // Current Non-Windows platforms all use char as native char. 
	%typemap(ctype) AkOSChar* "char*"
	%typemap(imtype, inattributes="[MarshalAs(UnmanagedType.LPStr)]") AkOSChar* "string"
	%typemap(in) AkOSChar* %{ $1 = (char*) $input; %}
	%typemap(out) AkOSChar* %{ $result = $1; %}
#endif // #if defined(AK_WIN) || defined(AK_XBOXONE) || defined (AK_VITA)

%typemap(ctype) AkGameObjectID "AkGameObjectID"
%typemap(imtype) AkGameObjectID "ulong"
%typemap(cstype, out="ulong") AkGameObjectID "UnityEngine.GameObject"
%typemap(in) AkGameObjectID %{ $1 = $input; %}
%typemap(out) AkGameObjectID %{ $result = $1; %}
%typemap(csout) AkGameObjectID { return $imcall; }
%typemap(csvarin, excode=SWIGEXCODE2)  AkGameObjectID %{ set { $imcall;$excode } %}
%typemap(csvarout, excode=SWIGEXCODE2)  AkGameObjectID %{ get { return $imcall;$excode } %}

%typemap(ctype) AkGameObjectID* "AkGameObjectID*"
%typemap(imtype) AkGameObjectID* "ulong[]"
%typemap(cstype) AkGameObjectID* "ulong[]"
%typemap(csin) AkGameObjectID* "$csinput"
%typemap(csout) AkGameObjectID* { return $imcall; }
%typemap(in) AkGameObjectID* %{ $1 = $input; %}
%typemap(out) AkGameObjectID* %{ $result = $1; %}

// Declaring our constants to avoid getter functions
// Order: Must be before %include <AK/SoundEngine/Common/AkTypes.h>
// %constant must occur before %ignore.

// Constants taken from <AK/SoundEngine/Common/AkTypes.h>
%constant AkPluginID				AK_INVALID_PLUGINID					= unchecked((uint)(-1));	///< Invalid FX ID
%constant AkGameObjectID			AK_INVALID_GAME_OBJECT				= unchecked((ulong)(-1));	///< Invalid game object (may also mean all game objects)
%constant AkUniqueID				AK_INVALID_UNIQUE_ID				= 0;						///< Invalid unique 32-bit ID
%constant AkRtpcID					AK_INVALID_RTPC_ID					= AK_INVALID_UNIQUE_ID;		///< Invalid RTPC ID
%constant AkUInt32					AK_INVALID_LISTENER_INDEX			= unchecked((uint)(-1));	///< Invalid listener index
%constant AkPlayingID				AK_INVALID_PLAYING_ID				= AK_INVALID_UNIQUE_ID;		///< Invalid playing ID
%constant AkUInt32					AK_DEFAULT_SWITCH_STATE				= 0;						///< Switch selected if no switch has been set yet
%constant AkMemPoolId				AK_INVALID_POOL_ID					= -1;						///< Invalid pool ID
%constant AkMemPoolId				AK_DEFAULT_POOL_ID					= -1;						///< Default pool ID, same as AK_INVALID_POOL_ID
%constant AkAuxBusID				AK_INVALID_AUX_ID					= AK_INVALID_UNIQUE_ID;		///< Invalid auxiliary bus ID (or no Aux bus ID)
%constant AkFileID					AK_INVALID_FILE_ID					= unchecked((uint)(-1));	///< Invalid file ID
%constant AkDeviceID				AK_INVALID_DEVICE_ID				= unchecked((uint)(-1));	///< Invalid streaming device ID
%constant AkBankID					AK_INVALID_BANK_ID					= AK_INVALID_UNIQUE_ID;		///< Invalid bank ID
%constant AkArgumentValueID			AK_FALLBACK_ARGUMENTVALUE_ID		= 0;						///< Fallback argument value ID
%constant AkChannelMask				AK_INVALID_CHANNELMASK				= 0;						///< Invalid channel mask
%constant AkUInt32					AK_INVALID_OUTPUT_DEVICE_ID			= AK_INVALID_UNIQUE_ID;		///< Invalid Device ID
%constant AkUInt32					AK_MIXER_FX_SLOT					= unchecked((uint)(-1));	///< Mixer slot
%constant AkGameObjectID			AK_DEFAULT_LISTENER_OBJ				= 0;						///< Default listener ID 0

// changed from AkPriority to unsigned int
%constant unsigned int				AK_DEFAULT_PRIORITY					=  50;						///< Default sound / I/O priority
%constant unsigned int				AK_MIN_PRIORITY						=  0;						///< Minimal priority value [0,100]
%constant unsigned int				AK_MAX_PRIORITY						=  100;						///< Maximal priority value [0,100]
%constant unsigned int				AK_DEFAULT_BANK_IO_PRIORITY			= AK_DEFAULT_PRIORITY;		///<  Default bank load I/O priority

// changed from AkReal32 to double
%constant double					AK_DEFAULT_BANK_THROUGHPUT			= 1*1024*1024/1000.0;		///<  Default bank load throughput (1 Mb/ms)

%constant unsigned int		AKCOMPANYID_AUDIOKINETIC			= 0;
%constant unsigned int		AKMOTIONDEVICEID_RUMBLE 			= 406;
%constant unsigned int		AK_LISTENERS_MASK_ALL 				= 0xFFFFFFFF;


%ignore AK_INVALID_PLUGINID;
%ignore AK_INVALID_GAME_OBJECT;
%ignore AK_INVALID_UNIQUE_ID;
%ignore AK_INVALID_RTPC_ID;
%ignore AK_INVALID_LISTENER_INDEX;
%ignore AK_INVALID_PLAYING_ID;
%ignore AK_DEFAULT_SWITCH_STATE;
%ignore AK_INVALID_POOL_ID;
%ignore AK_DEFAULT_POOL_ID;
%ignore AK_INVALID_AUX_ID;
%ignore AK_INVALID_FILE_ID;
%ignore AK_INVALID_DEVICE_ID;
%ignore AK_INVALID_BANK_ID;
%ignore AK_FALLBACK_ARGUMENTVALUE_ID;
%ignore AK_INVALID_CHANNELMASK;
%ignore AK_INVALID_OUTPUT_DEVICE_ID;
%ignore AK_MIXER_FX_SLOT;
%ignore AK_DEFAULT_LISTENER_OBJ;

%ignore AK_DEFAULT_PRIORITY;
%ignore AK_MIN_PRIORITY;
%ignore AK_MAX_PRIORITY;

%ignore AK_DEFAULT_BANK_IO_PRIORITY;
%ignore AK_DEFAULT_BANK_THROUGHPUT;

%ignore AKCOMPANYID_AUDIOKINETIC;
%ignore AKMOTIONDEVICEID_RUMBLE;
%ignore AK_LISTENERS_MASK_ALL;


// Must be before %include <AK/SoundEngine/Common/AkTypes.h>
%rename(IsEqualTo) WwiseObjectIDext::operator==;
// WG-21816
%rename(GetNodeType) WwiseObjectIDext::GetType();

//%ignore HWND;	//AkPlatformInitSettings has this on Windows, we ignore it for now. REVIEW!  We need to find a way to avoid exposing the specifics of each platforms.
#define __cdecl


%include "arrays_csharp.i"

// Rules for input/output arguments
// Note: typedefs cannot be used in %apply rule name. Use define instead

#if defined(AK_WIN) || defined(AK_XBOXONE) || defined(AK_VITA)
	#define ApplyAkUInt32 unsigned long
	#define ApplyAkInt32 long
#else
	#define ApplyAkUInt32 unsigned int
	#define ApplyAkInt32 int
#endif // #if defined(AK_WIN) || defined(AK_XBOXONE) || defined(AK_VITA)

// AkReal32 apply aliase
#define ApplyAkReal32 float

// bool and enum arguments
#define ApplyBoolEnum int

// AkUInt8 arguments
#define ApplyAkUInt8 unsigned char

// enum arguments
#define ApplyOutputEnum int

#if defined(AK_PS4)
	#define ApplyAkUInt64 unsigned long
	%apply ApplyAkUInt64 { SceKernelCpumask };
#endif // #if defined(AK_PS4)

%ignore AK::ATRAC9;

// AkUInt32 arguments
// - as input array
%apply ApplyAkUInt32 INPUT[] { AkUInt32* in_paGameSyncID, AkUInt32*	in_paGameSyncID, AkUniqueID*, AkArgumentValueID* };
// - as output array
%apply ApplyAkUInt32 OUTPUT[] { AkPlayingID* out_aPlayingIDs, AkUniqueID* out_audioNodeID, AkUniqueID* out_mediaID };
// - as single output
%apply ApplyAkUInt32 &OUTPUT { AkStateID& out_rState, AkSwitchStateID& out_rSwitchState, AkBankID& out_bankID, AkUInt32& out_uChannelConfig};
// - as single input/output
%apply ApplyAkUInt32 &INOUT { AkUInt32&	io_ruNumEnvValues, AkUInt32& io_ruNumItems, AkUInt32& io_ruNumIDs, AkUInt32& io_ruNumSendValues, AkUInt32 &io_uChannelMask, AkUInt32& io_uNumAngles};
// - as pointer input/output
%apply ApplyAkUInt32 *INOUT {AkUInt32* io_pcPositions};

// AkInt32 arguments
// - as single output
%apply ApplyAkInt32 *OUTPUT { AkTimeMs*, AkMemPoolId* };
%apply ApplyAkInt32	OUTPUT[] { AkTimeMs* out_msTime };
%apply ApplyAkInt32 &OUTPUT { AkInt32& out_iValue, AkTimeMs& out_buffering };
%apply ApplyAkUInt32 &OUTPUT { AkUInt32& };

// AkReal32 arguments
%apply ApplyAkReal32 &OUTPUT { AkReal32&, AkRtpcValue& };
%apply ApplyAkReal32 INOUT[] { AkReal32* io_pfSpeakerAngles };
%apply ApplyAkReal32 INPUT[] { AkReal32* in_pfSpeakerAngles };
%apply ApplyAkReal32 INPUT[] { AK::SpeakerVolumes::VectorPtr in_pVolumeOffsets };

%typemap(in) AkGroupType INPUT { $1 = (AkGroupType)$input; }

// Misc arguments
%apply ApplyBoolEnum &OUTPUT {bool&};
%apply ApplyBoolEnum &INOUT {AK::SoundEngine::Query::RTPCValue_type&};
%apply ApplyAkUInt8 &OUTPUT { AkUInt8& };
%apply ApplyOutputEnum &OUTPUT { AkPanningRule & }; // GetPanningRule()

//Typemap that will route the AkCallbackFunc parameter through the AkCallbackSerializer
//The cookie must point to a CallbackPackage in C#.  The package will have the real C# call back plus the C# cookie.

#ifdef AK_PS4
	%typemap(cstype) SceKernelCpumask dwAffinityMask "ulong"
	%typemap(imtype) SceKernelCpumask dwAffinityMask "ulong"
	%typemap(csvarout) SceKernelCpumask, ulong 
	%{ 
	get
	{ 
		return (ulong)($imcall); 
	} 
	%} 
#endif // AK_PS4

	//This will force AkCallbackSerializer::EventCallback to be the callback on the C++ side.  Thus, it forces serialization through AkCallbackSerializer
	%typemap(in) AkCallbackFunc %{ $1 = $input != NULL ? AkCallbackSerializer::EventCallback : NULL; %}

	//This overrides the void* typemap for parameters named "in_pCookie" on the C# side.  It maps to "object" as input parameter.
	%typemap(cstype) void * in_pCookie "object"
	%typemap(csin) void * in_pCookie "$csinput != null ? (IntPtr)$csinput.GetHashCode() : IntPtr.Zero"

	//This maps AkCallbackFunc to AkCallbackManager.EventCallback delegate type on the C# side.
	%typemap(cstype) AkCallbackFunc "AkCallbackManager.EventCallback"
	%typemap(imtype) AkCallbackFunc "IntPtr"

	//This typemap triggers when receiving a AkCallbackFunc(AkCallbackManager.EventCallback) as input parameter on the C# side and injects the conversion code.
	//The callback pointer itself is treated like a boolean, telling the C++ side if the callback pointer is really needed.  It will be handled by a static function on the other side.
	%typemap(	csin,
				pre="	in_pCookie = AkCallbackManager.EventCallbackPackage.Create($csinput, in_pCookie, ref in_uFlags);"
				) AkCallbackFunc "in_uFlags != 0 ? (IntPtr)1 : IntPtr.Zero"

	%typemap(csout) AkPlayingID {
		uint ret = $imcall;
		AkCallbackManager.SetLastAddedPlayingID(ret);
		return ret;
	}

	%typemap(csout) void CancelEventCallback {
		AkCallbackManager.RemoveEventCallback(in_playingID);
	}

	%typemap(csout) void CancelEventCallbackCookie {
		AkCallbackManager.RemoveEventCallbackCookie(in_pCookie);
	}

//Typemap that will route the AkBankCallbackFunc parameter through the AkCallbackSerializer
//The cookie must point to a CallbackPackage in C#.  The package will have the real C# call back plus the C# cookie.

	//This will force AkCallbackSerializer::BankCallback to be the callback on the C++ side.  Thus, it forces serialization through AkCallbackSerializer
	%typemap(in) AkBankCallbackFunc %{ $1 = (AkBankCallbackFunc) AkCallbackSerializer::BankCallback; %}

	//This maps AkBankCallbackFunc to AkCallbackManager.EventCallback delegate type on the C# side.
	%typemap(cstype) AkBankCallbackFunc "AkCallbackManager.BankCallback"
	%typemap(imtype) AkBankCallbackFunc "IntPtr"

	//This typemap triggers when receiving a AkBankCallbackFunc(AkCallbackManager.BankCallback) as input parameter on the C# side and injects the conversion code.
	%typemap(csin, pre="		in_pCookie = new AkCallbackManager.BankCallbackPackage($csinput, in_pCookie);") AkBankCallbackFunc "IntPtr.Zero"

	%typemap(csout) void CancelBankCallbackCookie {
		AkCallbackManager.RemoveBankCallback(in_pCookie);
	}

// Include all our public interfaces: Must be below WwiseObjectIDext operators
%{
#include <AK/SoundEngine/Common/AkTypes.h>
%}
%include <AK/SoundEngine/Common/AkTypes.h>

// These typemaps are placed after AkTypes because inside of AkAuxSendValue "listenerID > set > value" was being converted to tempvalue
	%typemap(csin, pre="
	var $csinput_id = AkSoundEngine.GetAkGameObjectID($csinput);
	AkSoundEngine.PreGameObjectAPICall($csinput, $csinput_id);
") AkGameObjectID "$csinput_id"

%typemap(csin) AkGameObjectID in_GameObjectID "AkSoundEngine.GetAkGameObjectID($csinput)"
%typemap(csin) AkGameObjectID in_GameObj "AkSoundEngine.GetAkGameObjectID($csinput)"
%typemap(csin) AkGameObjectID gameObjID "$csinput"

%pragma(csharp) moduleclassmodifiers="public partial class"

//This typemap replaces the AkAuxSendArray(in pre-2012.2, AkEnvironmentValue) struct array to a hand-written object on the C# side that takes care of the marshalling.
//Unfortunately, SWIG doesn't seem to handle contiguous arrays of structs.
	%typemap(cstype) AkAuxSendValue* "AkAuxSendArray"
	%typemap(csin) AkAuxSendValue* "$csinput.GetBuffer()"
	%typemap(imtype) AkAuxSendValue* "IntPtr"
	
//This typemap replaces the AkSoundPosition* struct array to a hand-written object on the C# side that takes care of the marshalling.
//Unfortunately, SWIG doesn't seem to handle contiguous arrays of structs.
	%typemap(cstype) const AkSoundPosition* "AkPositionArray"
	%typemap(csin) const AkSoundPosition* "$csinput.m_Buffer"
	%typemap(imtype) const AkSoundPosition* "IntPtr"

//This typemap replaces the AkChannelEmitter* struct array to a hand-written object on the C# side that takes care of the marshalling.
//Unfortunately, SWIG doesn't seem to handle contiguous arrays of structs.
	%typemap(cstype) const AkChannelEmitter* "AkChannelEmitterArray"
	%typemap(csin) const AkChannelEmitter* "$csinput.m_Buffer"
	%typemap(imtype) const AkChannelEmitter* "IntPtr"

//This typemap replaces the bitfield enum AkMemPoolAttributes with a simple int.  This avoids a "SWIGTYPE" file.
	%typemap(cstype) AkMemPoolAttributes "int"
	%typemap(csin) AkMemPoolAttributes "$csinput"
	%typemap(ctype) AkMemPoolAttributes "int"
	%typemap(imtype) AkMemPoolAttributes "int"
	%typemap(in) AkMemPoolAttributes %{ $1 = (AkMemPoolAttributes)$input; %}
	%typemap(out) AkMemPoolAttributes %{ $result = (AkMemPoolAttributes)$1; %}
	%typemap(csout) AkMemPoolAttributes { return $imcall; }
	%typemap(csvarin, excode=SWIGEXCODE2)  AkMemPoolAttributes %{ set { $imcall;$excode } %}
	%typemap(csvarout, excode=SWIGEXCODE2)  AkMemPoolAttributes %{ get { return $imcall;$excode } %}

%clear AkOSChar*;

// char
%typemap(out) char*, char[ANY], char[] %{ $result = $1; %}
%typemap(imtype,
	out="IntPtr",
	inattributes="[MarshalAs(UnmanagedType.LPStr)]") char*, char[ANY], char[] "string"

%typemap(csin) char*, char[ANY], char[] "$csinput"
%typemap(csout) char*, char[ANY], char[] { return AkSoundEngine.StringFromIntPtrString($imcall); }

%typemap(csvarin, excode=SWIGEXCODE2) char*, char[ANY], char[] %{ set { $imcall;$excode } %}
%typemap(csvarout, excode=SWIGEXCODE2) char*, char[ANY], char[] %{ get { return AkSoundEngine.StringFromIntPtrString($imcall);$excode } %}

// wchar_t
%typemap(ctype) wchar_t*, wchar_t[ANY], wchar_t[] "wchar_t*"
%typemap(imtype,
	out="IntPtr",
	inattributes="[MarshalAs(UnmanagedType.LPWStr)]") wchar_t*, wchar_t[ANY], wchar_t[] "string"

// Apple platform wchar_t is encoded with UTF-32, while Windows uses UTF-16.
#ifdef __APPLE__
	%typemap(in) wchar_t*, wchar_t[ANY], wchar_t[] %{ CONVERT_UTF16_TO_WCHAR($input, $1); %}
	%typemap(out) wchar_t*, wchar_t[ANY], wchar_t[] %{ CONVERT_WCHAR_TO_UTF16($1, $result); %}
#else
	%typemap(in) wchar_t*, wchar_t[ANY], wchar_t[] %{ $1 = ($1_ltype)$input; %}
	%typemap(out) wchar_t*, wchar_t[ANY], wchar_t[] %{ $result = $1; %}
#endif // #ifdef __APPLE__

%typemap(cstype) wchar_t*, wchar_t[ANY], wchar_t[] "string"
%typemap(csin) wchar_t*, wchar_t[ANY], wchar_t[] "$csinput"
%typemap(csout) wchar_t*, wchar_t[ANY], wchar_t[] { return AkSoundEngine.StringFromIntPtrWString($imcall); }

%typemap(csvarin, excode=SWIGEXCODE2) wchar_t*, wchar_t[ANY], wchar_t[] %{ set { $imcall;$excode } %}
%typemap(csvarout, excode=SWIGEXCODE2) wchar_t*, wchar_t[ANY], wchar_t[] %{ get { return AkSoundEngine.StringFromIntPtrWString($imcall);$excode } %}

// AkOSChar
%typemap(csout) AkOSChar*, AkOSChar[ANY], AkOSChar[] { return AkSoundEngine.StringFromIntPtrOSString($imcall); }
%typemap(csvarout, excode=SWIGEXCODE2) AkOSChar*, AkOSChar[ANY], AkOSChar[] %{ get { return AkSoundEngine.StringFromIntPtrOSString($imcall);$excode } %}


//AkAssertHook can't be used.  Just make sure we avoid polluting the interface with a "SWIGTYPE" file.
	%typemap(cstype) AkAssertHook "int"
	%typemap(csin) AkAssertHook "0"
	%typemap(csout) AkAssertHook { return 0; }
	%typemap(csvarout, excode=SWIGEXCODE2)  AkAssertHook %{ get { return 0; }%}
	%typemap(csvarin, excode=SWIGEXCODE2)  AkAssertHook %{ set { }%}

//Transform HWND into HandleRef.  This avoids a "SWIGTYPE" file.
	%typemap(cstype) HWND "HandleRef"
	%typemap(csin) HWND "$csinput"
	%typemap(csout) HWND { return $imcall; }
	%typemap(csvarin, excode=SWIGEXCODE2) HWND %{ set { $imcall;$excode } %}
	%typemap(csvarout, excode=SWIGEXCODE2) HWND %{ get { return new HandleRef(null, IntPtr.Zero); } %}

//These typemaps make array of strings come accross properly.
#ifdef AK_SUPPORT_WCHAR
	// C# marshalling: platform independent
	%typemap(ctype) wchar_t const ** "wchar_t const*";
	%typemap(cstype) wchar_t const ** "string []"
	%typemap(imtype) wchar_t const ** "IntPtr"
	%typemap(csin, pre="			
		//Find the required size
		int size = 0;
		foreach(string s in $csinput)
			size += s.Length + 1;
				
		int sizeofChar = 2;	//Unicode
		IntPtr pMem = Marshal.AllocHGlobal(size * sizeofChar);
		
		//Write the length of array
		Marshal.WriteInt16(pMem, (short)$csinput.Length);
		IntPtr pCurrent = (IntPtr)(pMem.ToInt64() + sizeofChar);
		
		//Copy the strings one after the other.
		foreach(string s in $csinput)
		{
			Marshal.Copy(s.ToCharArray(), 0, pCurrent, s.Length);
			pCurrent = (IntPtr)(pCurrent.ToInt64() + sizeofChar * s.Length);
			Marshal.WriteInt16(pCurrent, 0);	//Null-terminated string
			pCurrent = (IntPtr)(pCurrent.ToInt64() + sizeofChar);
		}		
		", post="	Marshal.FreeHGlobal(pMem);") wchar_t const ** "pMem";
	
	#ifndef __APPLE__
		%typemap(in) wchar_t const ** %{
		   //Create a string array of the proper size
		   unsigned short size = *$input;
		   $input++; // skip size word
		   $1 = (wchar_t**)AkAlloca(size*sizeof(wchar_t*));
		   unsigned long len = 0;
		   //Make all pointers point to the right place.
		   for(unsigned long i = 0; i < size; i++)
		   {
				$1[i] = (wchar_t*)$input + len;
				len += (unsigned long)wcslen($1[i]) + 1;
		   }
		%}
	#else // #ifndef __APPLE__
		%typemap(in) wchar_t const ** %{
		   //Create a string array of the proper size
		   unsigned short size = *$input;
		   AkUtf16* csharpString = (AkUtf16*)$input; 
		   ++csharpString; // skip size word
		   $1 = (wchar_t**)AkAlloca(size*sizeof(wchar_t*));
		   unsigned long len = 0;
		   //Make all pointers point to the right place.
		   for(unsigned long i = 0; i < size; i++)
		   {
				AkUtf16* winWcharString = csharpString + len;
				size_t srcLen = AKPLATFORM::AkUtf16StrLen(winWcharString);
				wchar_t* appleWcharString = NULL;
				CONVERT_UTF16_TO_WCHAR(winWcharString, appleWcharString);
				$1[i] = appleWcharString;

				len += (unsigned long)srcLen + 1;
		   }
		%}
	#endif // #ifndef __APPLE__
	
#endif // #ifdef AK_SUPPORT_WCHAR
	
//Typemaps to make char string array come across PINVOKE properly
	%typemap(ctype) char const ** "wchar_t const*";
	%typemap(cstype) char const ** "string []"
	%typemap(imtype) char const ** "IntPtr"
	%typemap(csin, pre="
		//Find the required size
		int size = 0;
		foreach(string s in $csinput)
			size += s.Length + 1;

		int sizeofChar = 2;	// UNICODE
		IntPtr pMem = Marshal.AllocHGlobal(size * sizeofChar);

		//Write the length of array
		Marshal.WriteInt16(pMem, (short)$csinput.Length);
		IntPtr pCurrent = (IntPtr)(pMem.ToInt64() + sizeofChar);

		//Copy the strings one after the other.
		foreach(string s in $csinput)
		{
			Marshal.Copy(s.ToCharArray(), 0, pCurrent, s.Length);
			pCurrent = (IntPtr)(pCurrent.ToInt64() + sizeofChar * s.Length);
			Marshal.WriteInt16(pCurrent, 0);	//Null-terminated string
			pCurrent = (IntPtr)(pCurrent.ToInt64() + sizeofChar);
		}
		", post="	Marshal.FreeHGlobal(pMem);") char const ** "pMem";

	// Only Android uses char-based APIs and Android uses UTF-32 for wchar_t.
	%typemap(in) char const ** %{
	   //Create a string array of the proper size
	   unsigned short size = *$input;
	   AkUtf16* csharpString = (AkUtf16*)$input; 
	   ++csharpString; // skip size word
	   $1 = (char**)AkAlloca(size*sizeof(wchar_t*));
	   unsigned long len = 0;
	   //Make all pointers point to the right place.
	   for(unsigned long i = 0; i < size; i++)
	   {
			AkUtf16* winWcharString = csharpString + len;
			size_t srcLen = AKPLATFORM::AkUtf16StrLen(winWcharString);
			char* charString = NULL;
			CONVERT_UTF16_TO_CHAR(winWcharString, charString);
			$1[i] = charString;

			len += (unsigned long)srcLen + 1;
	   }
	%}

%ignore RegisterBusVolumeCallback;
%ignore AkBusCallbackFunc;
%ignore RegisterBusMeteringCallback;
%ignore AkSpeakerVolumeMatrixCallbackInfo;	//Not supported.
%ignore AkSpeakerVolumeMatrixBusCallbackInfo;	//Not supported.
%ignore GetListenerSpatialization;

%ignore SetPosition;	//SetPosition is replaced by SetObjectPosition below
%ignore GetSourcePlayPositions;
%ignore AK::SoundEngine::RegisterGameObj;
%ignore AK::SoundEngine::UnregisterGameObj;
%ignore AK::SoundEngine::PostEvent(AkUniqueID, AkGameObjectID, AkUInt32, AkCallbackFunc );		//In C#, the Callback and the cookie must be defined.
%ignore AK::SoundEngine::PostEvent(AkUInt32, AkGameObjectID, AkUInt32, AkCallbackFunc );		//In C#, the Callback and the cookie must be defined.
%ignore AK::SoundEngine::PostEvent(char const *, AkGameObjectID, AkUInt32, AkCallbackFunc);	//In C#, the Callback and the cookie must be defined.
%ignore AK::SoundEngine::PostEvent(wchar_t const *,AkGameObjectID, AkUInt32, AkCallbackFunc);	//In C#, the Callback and the cookie must be defined.
%ignore AK::SoundEngine::DecodeBank;
%ignore AK::SoundEngine::GetContainerHistory;
%ignore AK::SoundEngine::SetContainerHistory;
%ignore AK::SoundEngine::DynamicSequence::GetPlayingItem;

%ignore AK::Monitor::SetLocalOutput;
%ignore AK::SoundEngine::AddSecondaryOutput(AkUInt32,AkAudioOutputType,const AkGameObjectID *,AkUInt32,AkUInt32,AkUniqueID);
%ignore AK::SoundEngine::AddSecondaryOutput(AkUInt32,AkAudioOutputType,const AkGameObjectID *,AkUInt32,AkUInt32);
%ignore AK::SoundEngine::AddSecondaryOutput(AkUInt32,AkAudioOutputType,const AkGameObjectID *,AkUInt32,AkUniqueID);
%ignore AK::SoundEngine::AddSecondaryOutput(AkUInt32,AkAudioOutputType,const AkGameObjectID *,AkUInt32);
%ignore LocalOutputFunc;
%ignore pad;

// %ignore is used to remove the constructors from the C# implementations of AkAuxSendValue and AkAuxSendValueProxy
%ignore AkAuxSendValue::AkAuxSendValue();
%ignore AkAuxSendValueProxy::AkAuxSendValueProxy();

%ignore s_aszErrorCodes;

%ignore AkAudioBuffer;
%ignore IAkMetering;
%ignore HasLFE;
%ignore HasCenter;
%ignore GetNumberOfAnglesForConfig;

// PS4 and Vita compilers don't like __attribute__ for inline
#undef AkForceInline
#define AkForceInline

%include <AK/SoundEngine/Common/AkCommonDefs.h>


// C# bindings allow to use only one of wchar_t*/char* API pairs (multiple-definition problem). 
// For platforms that support wchar_t*, in favor of wchar_t* APIs, ignore char* APIs; and vice versa.
#ifdef AK_SUPPORT_WCHAR
	#define IgnoredCharType char
#else
	#define IgnoredCharType wchar_t
#endif
%ignore AK::SoundEngine::PrepareEvent(PreparationType, const IgnoredCharType**, AkUInt32);
%ignore AK::SoundEngine::PrepareEvent(PreparationType, const IgnoredCharType**, AkUInt32, AkBankCallbackFunc, void*);
%ignore AK::SoundEngine::PrepareGameSyncs(PreparationType, AkGroupType, const IgnoredCharType*, const IgnoredCharType**, AkUInt32);
%ignore AK::SoundEngine::PrepareGameSyncs(PreparationType, AkGroupType, const IgnoredCharType*, const IgnoredCharType**, AkUInt32, AkBankCallbackFunc, void *);
// %ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const IgnoredCharType*, const IgnoredCharType**, AkUInt32);
// %ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const IgnoredCharType*, const IgnoredCharType**, AkUInt32, AkPlayingID);
%ignore AK::SoundEngine::LoadBank(const IgnoredCharType*, AkMemPoolId, AkBankID &);
%ignore AK::SoundEngine::LoadBank(const IgnoredCharType*, AkBankCallbackFunc, void *, AkMemPoolId, AkBankID &);
%ignore AK::SoundEngine::Query::GetRTPCValue(const IgnoredCharType*, AkGameObjectID, AkRtpcValue&, RTPCValue_type&);
%ignore AK::SoundEngine::Query::GetSwitch(const IgnoredCharType*, AkGameObjectID, AkSwitchStateID&);
%ignore AK::SoundEngine::Query::GetState(const IgnoredCharType*, AkStateID&);
%ignore AK::SoundEngine::Query::QueryAudioObjectIDs(const IgnoredCharType*, AkUInt32&, AkObjectInfo*);
%ignore AK::SoundEngine::Query::GetListeners(AkGameObjectID, AkGameObjectID*,	AkUInt32& );

// As of Unity3.5.2 and Wwise 22012.1.4, ignore this particular string-based API to avoid a crash
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const char*, const char**, AkUInt32);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const char*, const char**, AkUInt32, AkPlayingID);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const char*, const char**, AkUInt32, AkPlayingID, AkCandidateCallbackFunc);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const char*, const char**, AkUInt32, AkPlayingID, AkCandidateCallbackFunc, void*);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const char*, const char**, AkUInt32, AkPlayingID);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const wchar_t*, const wchar_t**, AkUInt32);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const wchar_t*, const wchar_t**, AkUInt32, AkPlayingID);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const wchar_t*, const wchar_t**, AkUInt32, AkPlayingID, AkCandidateCallbackFunc);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(const wchar_t*, const wchar_t**, AkUInt32, AkPlayingID, AkCandidateCallbackFunc, void*);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(AkUniqueID, AkArgumentValueID*, AkUInt32, AkPlayingID, AkCandidateCallbackFunc);
%ignore AK::SoundEngine::DynamicDialogue::ResolveDialogueEvent(AkUniqueID, AkArgumentValueID*, AkUInt32, AkPlayingID, AkCandidateCallbackFunc, void*);

%ignore AkCallbackSerializer::EventCallback;
%ignore AkCallbackSerializer::BankCallback;
%ignore AkCallbackSerializer::AudioInterruptionCallback;
%ignore AkCallbackSerializer::BGMCallback;
%ignore AK::SoundEngine::DynamicSequence::Open(AkGameObjectID, AkUInt32, AkCallbackFunc );		//In C#, the Callback and the cookie must be defined.

//These ignores are necessary to avoid the IsInitialized check added by the %exception statement.
%ignore AK::SoundEngine::GetDefaultInitSettings;
%ignore AK::SoundEngine::GetDefaultPlatformInitSettings;

//These ignores hide the useless Init/Term of the MusicEngine
%ignore AK::MusicEngine::Init;
%ignore AK::MusicEngine::Term;
%ignore AK::MusicEngine::GetDefaultInitSettings;

// Remove modules that were to be removed in old GenC++.cmd script
%ignore AK::SoundEngine::RegisterPlugin;
%ignore AK::SoundEngine::RegisterCodec;
%ignore AK::SoundEngine::Init;
%ignore AK::SoundEngine::Term;
%ignore AK::SoundEngine::RegisterGlobalCallback;
%ignore AK::SoundEngine::UnregisterGlobalCallback;
%ignore AK::SoundEngine::GetDefaultDeviceSettings;
%ignore AK::StreamMgr::GetDefaultDeviceSettings;
%ignore AkCallbackInfo;
%ignore AkEventCallbackInfo;
%ignore AkDurationCallbackInfo;
%ignore AkDynamicSequenceItemCallbackInfo;
%ignore AkMarkerCallbackInfo;
%ignore AkMIDIEvent;
%ignore AkMIDIEventCallbackInfo;
%ignore AkMusicPlaylistCallbackInfo;
%ignore AkMusicSyncCallbackInfo;
%ignore AkMidiNoteChannelPair;
// Explicit ignores from generated API.
%ignore AK::Monitor::s_aszErrorCodes;
%ignore AkAssertHook;
%ignore AkCallbackFunc;
%ignore AkGlobalCallbackFunc;
%ignore AK::MemoryMgr::Init;


// Windows, Metro:
// Until user has specific audio performance requirements, ignore low-level platformInitSettings members for now.
%ignore hWnd;
%ignore IDirectSound8;
%ignore pXAudio2;
%ignore IXAudio2;
%ignore GetWwiseXAudio2Interface;
%ignore GetDirectSoundInstance;
%ignore IMMDevice;
%ignore GetDeviceID;

// Motion: Registration already handled in handwritten Init.
%ignore AkCreatePluginCallback;
%ignore AkCreateSinkPluginCallback;
%ignore pfSinkPluginFactory;
%ignore AK::MotionEngine::RegisterMotionDevice;

// Query: Ignore AkArray-related features, to avoid too much SWIG hacks: SWIG need separate fully-qualified AkArray classes for each typedef/subclass.
%ignore AK::SoundEngine::Query::AkGameObjectsList;
%ignore AK::SoundEngine::Query::GetActiveGameObjects(AkGameObjectsList&);
%ignore AK::SoundEngine::Query::GameObjDst;
%ignore AK::SoundEngine::Query::AkRadiusList;
%ignore AK::SoundEngine::Query::GetMaxRadius(AkRadiusList &);

// DynaimcDialogue: Ignore unsupported AkArray operations (WG-23783)
%ignore AkArray::BinarySearch;

// iOS: Ignore audio input related features
%ignore kAudioSessionCategory_PlayAndRecord;
%ignore AudioInputCallbackFunc;
%ignore AudioInterruptionCallbackFunc;
%ignore BGMCallbackFunc;
%ignore inputCallback;
%ignore inputCallbackCookie;
%ignore interruptionCallback;
%ignore interruptionCallbackCookie;
%ignore BGMCallback;
%ignore BGMCallbackCookie;
%ignore AudioBufferList;
%ignore audioCallbacks;
%ignore AkAudioCallbacks;

// Android: Ignore low-level audio settings and API
%ignore pSLEngine;
%ignore GetWwiseOpenSLInterface;
%ignore pJavaVM;
%ignore jNativeActivity;

%{
#if defined(AK_VITA) || defined(AK_PS4)
#undef SWIGEXPORT
#define SWIGEXPORT __declspec(dllexport)
#endif

#include <AK/SoundEngine/Common/AkSoundEngine.h>
#include <AK/SoundEngine/Common/AkModule.h>
#include <AK/SoundEngine/Common/AkStreamMgrModule.h>
#include <AK/MusicEngine/Common/AkMusicEngine.h>
#include "../Common/AkCallbackSerializer.h"
#include <AK/Tools/Common/AkMonitorError.h>
#include <AK/Tools/Common/AkPlatformFuncs.h>
#include <AK/SoundEngine/Common/AkDynamicDialogue.h>
#include "../Common/AkDynamicSequence_PlaylistItem.h"
#include "../Common/AkDynamicSequence_Playlist.h"
#include <AK/SoundEngine/Common/AkQueryParameters.h>
#include <AK/MotionEngine/Common/AkMotionEngine.h>

#if defined(AK_LINUX) || defined(AK_NX)
#undef AK_SUPPORT_WCHAR
#endif

using namespace AK::SoundEngine::DynamicSequence;
using namespace AK::SoundEngine::Query;
%}

%ignore AkExternalSourceArray;
%ignore GetExternalSources();
%ignore AK::GetDeviceIDFromName;
%ignore AK::GetWindowsDeviceName;

// Translate C++ operators into function calls
%rename(Assign) AK::SoundEngine::DynamicSequence::PlaylistItem::operator=;
%rename(IsEqualTo) AK::SoundEngine::DynamicSequence::PlaylistItem::operator==;
%rename(ItemAtIndex) AkArray::operator[];

// Rename DynamicSequence APIs for clarity after SWIG removes namespaces for clarity.
%rename(DynamicSequenceOpen) AK::SoundEngine::DynamicSequence::Open;
%rename(DynamicSequenceClose) AK::SoundEngine::DynamicSequence::Close;
%rename(DynamicSequencePlay) AK::SoundEngine::DynamicSequence::Play;
%rename(DynamicSequencePause) AK::SoundEngine::DynamicSequence::Pause;
%rename(DynamicSequenceResume) AK::SoundEngine::DynamicSequence::Resume;
%rename(DynamicSequenceStop) AK::SoundEngine::DynamicSequence::Stop;
%rename(DynamicSequenceBreak) AK::SoundEngine::DynamicSequence::Break;
%rename(DynamicSequenceLockPlaylist) AK::SoundEngine::DynamicSequence::LockPlaylist;
%rename(DynamicSequenceUnlockPlaylist) AK::SoundEngine::DynamicSequence::UnlockPlaylist;
%rename(DynamicSequenceGetPauseTimes) AK::SoundEngine::DynamicSequence::GetPauseTimes;

// Renamed so that the types have names that users expect within C#
%rename(AkCallbackInfo) AkSerializedCallbackInfo;
%rename(AkEventCallbackInfo) AkSerializedEventCallbackInfo;
%rename(AkMIDIEventCallbackInfo) AkSerializedMIDIEventCallbackInfo;
%rename(AkMarkerCallbackInfo) AkSerializedMarkerCallbackInfo;
%rename(AkDurationCallbackInfo) AkSerializedDurationCallbackInfo;
%rename(AkDynamicSequenceItemCallbackInfo) AkSerializedDynamicSequenceItemCallbackInfo;
%rename(AkMusicSyncCallbackInfo) AkSerializedMusicSyncCallbackInfo;
%rename(AkMusicPlaylistCallbackInfo) AkSerializedMusicPlaylistCallbackInfo;
%rename(AkBankCallbackInfo) AkSerializedBankCallbackInfo;
%rename(AkMonitoringCallbackInfo) AkSerializedMonitoringCallbackInfo;
%rename(AkAudioInterruptionCallbackInfo) AkSerializedAudioInterruptionCallbackInfo;
%rename(AkAudioSourceChangeCallbackInfo) AkSerializedAudioSourceChangeCallbackInfo;

// NOTE: From Unity side, script execution order is crucial: API calls must be made after sound engine is initialized and before it is terminated.
// This code will check that the sound engine is initialized for every function call.  Must be done AFTER "Init" is declared (in the inline section above). It's inserted to all APIs.
// Order: Must be before %include <AkUnityApiHeader.h>
%include "SwigExceptionSwitch.h"

// Expose AkArray Iterator: Step 1: Tell SWIG to use a global proxy class for the nested AkArray::Iterator class.
%include "AkArrayIterator.h";
%{
#include "../Common/AkArrayProxy.h"
%}
%include "AkArrayProxy.h"

RESUME_SWIG_EXCEPTIONS

%include "../Common/AkDynamicSequence_PlaylistItem.h"
%template(AkPlaylistArray) AkArray<AK::SoundEngine::DynamicSequence::PlaylistItem, AK::SoundEngine::DynamicSequence::PlaylistItem const &, ArrayPoolDefault,4>;
// Expose AkArray Iterator: Step 2: Tell C++ compiler to use proxy class.
%{
typedef AkArray<AK::SoundEngine::DynamicSequence::PlaylistItem, AK::SoundEngine::DynamicSequence::PlaylistItem const &, ArrayPoolDefault, 4>::Iterator Iterator;
%}
%include "../Common/AkDynamicSequence_Playlist.h"


CANCEL_SWIG_EXCEPTIONS(AkMemSettings::AkMemSettings());
CANCEL_SWIG_EXCEPTIONS(AkChannelConfig::AkChannelConfig);
CANCEL_SWIG_EXCEPTIONS(AK::SoundEngine::IsInitialized);

//SWIG_EXCEPTION_WITH_AKRESULT(PlaylistItem::SetExternalSources);
SWIG_EXCEPTION_WITH_AKRESULT(GetAudioSettings);

SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::GetPanningRule);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetPanningRule);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::GetSpeakerAngles);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetSpeakerAngles);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetVolumeThreshold);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetMaxNumVoicesLimit);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::RenderAudio);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::RegisterPluginDLL);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::ExecuteActionOnEvent);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::PostMIDIOnEvent);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::StopMIDIOnEvent);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::PinEventInStreamCache);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::UnpinEventInStreamCache);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::GetBufferStatusForPinnedEvent);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SeekOnEvent);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::GetSourcePlayPosition);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::GetSourceStreamBuffering);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SendPluginCustomGameData);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::UnregisterAllGameObj);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetMultiplePositions);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetScalingFactor);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::ClearBanks);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetBankLoadIOSettings);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::LoadBank);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::UnloadBank);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::PrepareBank);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::ClearPreparedEvents);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::PrepareEvent);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetMedia);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::UnsetMedia);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::PrepareGameSyncs);

// ignored AK::SoundEngine::SetListeners() and AK::SoundEngine::SetDefaultListeners() to fix WG-31581
%ignore AK::SoundEngine::SetListeners;
%ignore AK::SoundEngine::SetDefaultListeners;

SWIG_EXCEPTION_WITH_AKRESULT(SetListeners);
SWIG_EXCEPTION_WITH_AKRESULT(SetDefaultListeners);

SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetListenerSpatialization);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetListenerPipeline);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetRTPCValue);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetRTPCValueByPlayingID);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::ResetRTPCValue);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetSwitch);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::PostTrigger);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetState);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetGameObjectAuxSendValues);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetGameObjectOutputBusVolume);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetActorMixerEffect);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetBusEffect);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetMixer);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetBusConfig);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetObjectObstructionAndOcclusion);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::StartOutputCapture);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::StopOutputCapture);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::AddOutputCaptureMarker);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::StopProfilerCapture);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::RemoveSecondaryOutput);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::SetSecondaryOutputVolume);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Suspend);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::WakeupFromSuspend);

SWIG_EXCEPTION_WITH_AKRESULT(AK::MusicEngine::GetPlayingSegmentInfo);

SWIG_EXCEPTION_WITH_AKRESULT(AkCallbackSerializer::Init);
SWIG_EXCEPTION_WITH_AKRESULT(AkCallbackSerializer::AudioSourceChangeCallbackFunc);

SWIG_EXCEPTION_WITH_AKRESULT(AK::Monitor::PostCode);
SWIG_EXCEPTION_WITH_AKRESULT(AK::Monitor::PostString);

SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::DynamicDialogue::GetDialogueEventCustomPropertyValue);

SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetPosition);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetListenerPosition);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetRTPCValue);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetSwitch);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetState);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetGameObjectAuxSendValues);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetGameObjectDryLevelValue);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetObjectObstructionAndOcclusion);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::QueryAudioObjectIDs);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetPositioningInfo);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetPlayingIDsFromGameObject);
SWIG_EXCEPTION_WITH_AKRESULT(AK::SoundEngine::Query::GetCustomPropertyValue);

SWIG_EXCEPTION_WITH_AKRESULT(AK::MotionEngine::AddPlayerMotionDevice);

// API declarations blob.
%include <AkUnityApiHeader.h>

//Reset the exception to nothing.  This is IMPORTANT, the constructors/destructors are generated after all the interface 
//so to avoid the IsInitialized check in the AkInitSetting structure constructor, the exception must be reset.
PAUSE_SWIG_EXCEPTIONS

%include "AkSoundEngineStubs.h"
%{
#include "../Common/AkSoundEngineStubs.h"
%}
