
newoption {
   trigger     = "akplatform",
   value       = "VALUE",
   description = "Choose one platform to generate",
   allowed = {
      { "Mac",		"Mac" },
      { "Linux",	"Linux" },
      { "iOS", 		"iOS" },
      { "Windows",  "Windows" },
	  { "Android",  "Android" },
	  { "nacl",		"nacl" },
	  { "Vita",  	"Vita" },
	  { "XboxOne",  "XboxOne" },
	  { "XboxOneADK",  "XboxOneADK" },
	  { "Metro",  "Metro" },
	  { "WinPhone",	"WinPhone" },
	  { "PS4",  "PS4" },
	  { "3DS",  "3DS" },
	  { "QNX",  "QNX" },
	  { "Switch",	"Switch" },
   }
}

function SoundEngineInternals(suffix, communication)
	local d = {
		"AkSoundEngine"..suffix,
		"AkMemoryMgr"..suffix,
		"AkStreamMgr"..suffix,
		"AkMusicEngine"..suffix
	}

	if communication then
		d[#d+1] = "CommunicationCentral"..suffix
	end

	d[#d+1] = "AkSilenceSource"..suffix
	d[#d+1] = "AkSineSource"..suffix
	d[#d+1] = "AkToneSource"..suffix
	d[#d+1] = "AkSynthOne"..suffix
	d[#d+1] = "AkVorbisDecoder"..suffix

	return d
end

function Dependencies(suffix, motion, communication)
	local d = SoundEngineInternals(suffix, communication)

	d[#d+1] = "AkAudioInputSource"..suffix
	d[#d+1] = "AkCompressorFX"..suffix
	d[#d+1] = "AkDelayFX"..suffix
	d[#d+1] = "AkExpanderFX"..suffix
	d[#d+1] = "AkFlangerFX"..suffix
	d[#d+1] = "AkGainFX"..suffix
	d[#d+1] = "AkGuitarDistortionFX"..suffix
	d[#d+1] = "AkHarmonizerFX"..suffix
	d[#d+1] = "AkMatrixReverbFX"..suffix
	d[#d+1] = "AkMeterFX"..suffix
	d[#d+1] = "AkParametricEQFX"..suffix
	d[#d+1] = "AkPeakLimiterFX"..suffix
	d[#d+1] = "AkPitchShifterFX"..suffix
	d[#d+1] = "AkRecorderFX"..suffix
	d[#d+1] = "AkRoomVerbFX"..suffix
	d[#d+1] = "AkStereoDelayFX"..suffix
	d[#d+1] = "AkTimeStretchFX"..suffix
	d[#d+1] = "AkTremoloFX"..suffix

	if motion then
		d[#d+1] = "AkRumble"..suffix
		d[#d+1] = "AkMotionGenerator"..suffix
	end

	return d
end

function CreateCommonProject(platformName, suffix, motion)
	-- A project defines one build target
	local pfFolder = "../"..platformName.."/";
	local prj = project ("AkSoundEngine"..platformName)
		targetname "AkSoundEngine"
		kind "SharedLib"
		language "C++"
		files {
			"../Common/AkCallbackSerializer.cpp",
			pfFolder .. "AkDefaultIOHookBlocking.cpp",
			"../Common/AkFileLocationBase.cpp",
			"../Common/AkFilePackage.cpp",
			"../Common/AkFilePackageLUT.cpp",
			"../Common/AkSoundEngineStubs.cpp",
			pfFolder .. "SoundEngine_wrap.cxx",
			pfFolder .. "stdafx.cpp",
			}
		defines {
			"AUDIOKINETIC",
			}

		includedirs {
			"../"..platformName,
			"$(WWISESDK)/include",
			"."
			}

		configuration "Debug"
			defines { "_DEBUG" }
			flags { "Symbols" }
			links(Dependencies(suffix, motion, true))

		configuration "Profile"
			defines { "NDEBUG" }
			links(Dependencies(suffix, motion, true))

		configuration "Release"
			defines { "NDEBUG", "AK_OPTIMIZED" }
			flags { "Optimize" }
			links(Dependencies(suffix, motion, false))

		configuration "*"
	return prj
end

function CreateLinuxSln()
	-- A solution contains projects, and defines the available configurations
	solution "AkSoundEngineLinux"
	configurations { "Debug", "Profile", "Release" }
	platforms { "x64", "x32" }

	local prj = CreateCommonProject("Linux", "", false)
		defines "__linux__"
		location "../Linux"
		links "SDL2"

		buildoptions {
			"-fvisibility=hidden",
			"-fdata-sections",
			"-ffunction-sections"
			}

		linkoptions {
			"-Wl,--gc-sections"
		}

		flags {
			"EnableSSE2" 
			}

		local baseTargetDir = "../../Integration/Assets/Wwise/Deployment/Plugins/Linux/"

		configuration { "Debug", "x32" }
			libdirs {
				"$(WWISESDK)/Linux_x32/Debug/lib"
				}
			targetdir( baseTargetDir .. "x86/Debug")

		configuration { "Profile", "x32" }
			libdirs {
				"$(WWISESDK)/Linux_x32/Profile/lib"
				}
			targetdir( baseTargetDir .. "x86/Profile")
			postbuildcommands ("strip " .. baseTargetDir .. "x86/Profile/libAkSoundEngine.so")

		configuration { "Release", "x32" }
			libdirs {
				"$(WWISESDK)/Linux_x32/Release/lib"
				}
			targetdir( baseTargetDir .. "x86/Release")
			postbuildcommands ("strip " .. baseTargetDir .. "x86/Release/libAkSoundEngine.so")

		configuration { "Debug", "x64" }
			libdirs {
				"$(WWISESDK)/Linux_x64/Debug/lib"
				}
			targetdir( baseTargetDir .. "x86_64/Debug")

		configuration { "Profile", "x64" }
			libdirs {
				"$(WWISESDK)/Linux_x64/Profile/lib"
				}
			targetdir( baseTargetDir .. "x86_64/Profile")
			postbuildcommands ( "strip " .. baseTargetDir .. "x86_64/Profile/libAkSoundEngine.so")

		configuration { "Release", "x64" }
			libdirs {
				"$(WWISESDK)/Linux_x64/Release/lib"
				}
			targetdir( baseTargetDir .. "x86_64/Release")
			postbuildcommands ( "strip " .. baseTargetDir .. "x86_64/Release/libAkSoundEngine.so")

		configuration "*x32"
			buildoptions { "-m32", "-msse" }

		configuration "*x64"
			buildoptions { "-m64" }
end

function CreateXboxOneSln()
	-- A solution contains projects, and defines the available configurations
	solution "AkSoundEngineXboxOne"
	location "../XboxOne"
	configurations { "Debug", "Profile", "Release" }
	platforms { "XboxOne" }
	links { "uuid", "acphal", "xaudio2", "combase", "kernelx", "ws2_32", "MMDevApi" }
	
	local prj = CreateCommonProject("XboxOne", "", false)
		defines { "_XBOX_ONE", "UNICODE", "_UNICODE" }
		location "../XboxOne"

		local baseTargetDir = "../../Integration/Assets/Wwise/Deployment/Plugins/XboxOne/"
		
		-- Style sheets.
		configuration { "*Debug*" }
			vs_propsheet("../XboxOne/PropertySheets/Debug_vc140.props")
		configuration { "Profile* or *Release*" }
			vs_propsheet("../XboxOne/PropertySheets/NDebug_vc140.props")

		configuration { "Debug", "XboxOne" }
			libdirs {
				"$(WWISESDK)/XboxOne_vc140/Debug/lib"
				}
			targetdir( baseTargetDir .. "Debug")

		configuration { "Profile", "XboxOne" }
			libdirs {
				"$(WWISESDK)/XboxOne_vc140/Profile/lib"
				}
			targetdir( baseTargetDir .. "Profile")
	
		configuration { "Release", "XboxOne" }
			libdirs {
				"$(WWISESDK)/XboxOne_vc140/Release/lib"
				}
			targetdir( baseTargetDir .. "Release")
end

function CreateSwitchSln()
	-- A solution contains projects, and defines the available configurations
	solution "AkSoundEngineSwitch"
	location "../Switch"
	configurations { "Debug", "Profile", "Release" }
	platforms { "NX64" }

	local SwitchHasMotion = false
	local targetName = "AkSoundEngineWrapper"

	local function ConvertDependenciesToPostBuildStep(dependencies)

		local addLogs = true

		local newLine = "\r\n"
		local saveRootDirectory = "pushd ."
		local returnToRootDirectory = "popd"
		local ar = "$(NINTENDO_SDK_ROOT)/Compilers/NX/nx/aarch64/bin/aarch64-nintendo-nx-elf-ar.exe"

		local output = ""

		if addLogs then
			output = output .. "echo This script is generated within the premake4.lua file." .. newLine
		end

		for i,v in ipairs(dependencies) do

			local tempDirectory = "$(IntDir)" .. v

			local currentLibPath = "$(WWISESDK)/$(Platform)/$(Configuration)/lib/lib" .. v .. ".a"

			local createDirectory = 
				"mkdir " .. tempDirectory .. newLine ..
				"cd " .. tempDirectory

			local extract = ar .. " x \"" .. currentLibPath .. "\""

			local addObjectsToArchive = ar .. " r $(TargetPath) " .. tempDirectory .. "/*.o"

			local removeDirectory = "rmdir /s /q " .. tempDirectory

			if addLogs then
				extract = "echo Extracting objects from library \"" .. currentLibPath .. "\" into temporary directory \"" .. tempDirectory .. "\"." .. newLine .. extract
				addObjectsToArchive = "echo Adding objects to library \"$(TargetFileName)\"." .. newLine .. addObjectsToArchive
			end

			output = output
				.. saveRootDirectory .. newLine
				.. createDirectory .. newLine
				.. extract .. newLine
				.. returnToRootDirectory .. newLine
				.. addObjectsToArchive .. newLine
				.. removeDirectory .. newLine
		end

		return output
	end

	local prj = CreateCommonProject("Switch", ".a", SwitchHasMotion)
		targetname(targetName)
		location "../Switch"
		kind "StaticLib"
		flags { "Unicode" }
		uuid "CED61298-E616-2841-B574-03A3EFE03719"
		links {}

		local depends = SoundEngineInternals("", true)
		depends[#depends+1] = "AkOpusDecoder"
		
		local baseTargetDir = "../../Integration/Assets/Wwise/Deployment/Plugins/Switch/"

		-- Standard configuration settings.
		configuration ("*")
			vs_propsheet("../Switch/NintendoSdkSpec_NX.props")
			vs_propsheet("../Switch/NintendoSdkVcProjectSettings.props")

		configuration ("Debug")
			defines ("_DEBUG")
			flags ("Symbols")
			vs_propsheet("../Switch/NintendoSdkBuildType_Debug.props")
			targetdir( baseTargetDir .. "NX64/Debug")
			postbuildcommands(ConvertDependenciesToPostBuildStep(depends))

		configuration ("Profile")
			defines ("NDEBUG")
			flags ({"Symbols", "OptimizeSpeed"})
			vs_propsheet("../Switch/NintendoSdkBuildType_Develop.props")
			targetdir( baseTargetDir .. "NX64/Profile")
			postbuildcommands(ConvertDependenciesToPostBuildStep(depends))

		configuration ("Release")
			defines ({"NDEBUG","AK_OPTIMIZED"})
			flags ({"Symbols", "OptimizeSpeed"})
			vs_propsheet("../Switch/NintendoSdkBuildType_Release.props")
			targetdir( baseTargetDir .. "NX64/Release")
			postbuildcommands(ConvertDependenciesToPostBuildStep(depends))

end

if _OPTIONS["akplatform"] == "Linux" then
	CreateLinuxSln();
end
if _OPTIONS["akplatform"] == "Switch" then
	CreateSwitchSln();
end
if _OPTIONS["akplatform"] == "XboxOne" then
	CreateXboxOneSln();
end
