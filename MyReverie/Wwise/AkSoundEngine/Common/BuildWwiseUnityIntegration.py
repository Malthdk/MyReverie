# //////////////////////////////////////////////////////////////////////
# //
# // Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
# //
# //////////////////////////////////////////////////////////////////////
import sys, os, os.path, argparse, platform, copy, time
from os import pardir
from os.path import dirname, abspath, join, exists, normpath
from argparse import RawTextHelpFormatter
import BuildUtil

ScriptDir = abspath(dirname(__file__))

class PlatformBuilder(object):
	def __init__(self, platformName, arches, configs, generateSwig):
		self.platformName = platformName
		self.compiler = None
		self.solution = None
		self.arches = arches
		self.configs = configs
		self.tasks = ['Clean', 'Build']
		self.logger = BuildUtil.CreateLogger(BuildUtil.DefaultLogFile, __file__, self.__class__.__name__)
		self.generateSwig = generateSwig

	def Build(self):
		'''Prototype of single-architecture build.'''
		results = []
		for c in self.configs:
			self.logger.info('Building: {} ({}) ...'.format(self.platformName, c))
			cmds = self._CreateCommands(arch=None, config=c)
			isSuccess = self._RunCommands(cmds)
			if isSuccess:
				resultCode = 'Success'
			else:
				resultCode = 'Fail'
			result = {'resultCode':resultCode, 'Message': '{}({})'.format(self.platformName, c)}
			results.append(result)
		return results

	def _ValidateEnvVar(self, varName, entityName):
		if not varName in os.environ:
			msg = 'Undefined {} environment variable: {}'.format(entityName, varName)
			self.logger.error(msg)
			raise RuntimeError(msg)
		elif not exists(os.environ[varName]):
			msg = 'Failed to find {} with environment variable {} = {}'.format(entityName, varName, os.environ[varName])
			self.logger.error(msg)
			raise RuntimeError(msg)

	def _CreateCommands(self, arch=None, config='Profile'):
		msg = BuildUtil.Messages['NotImplemented']
		self.logger.error(msg)
		raise NotImplementedError()

	def _RunCommands(self, cmds):
		'''The atomic build task to evaluate is a list of commands run in order.'''
		isSuccess = True
		for cmd in cmds:
			try:
				(stdOut, stdErr, returnCode) = BuildUtil.RunCommand(cmd)
				self.logger.debug('stdout:\n{}'.format(stdOut))
				self.logger.debug('stderr:\n{}'.format(stdErr))
				isCmdFailed = returnCode != 0
				if isCmdFailed:
					msg = 'Command failed with return code: {}'.format(returnCode)
					raise RuntimeError(msg)
			except Exception as err:
				isSuccess = False
				self.logger.error('Build task failed: command: {}, on-screen command: {}, error: {}. Skipped.'.format(cmd, ' '.join(cmd), err))
		return isSuccess

	def _FindLatestDotNetVersionDir(self):
		dotNetDir = '{}\\Microsoft.NET\\Framework'.format(os.environ['SYSTEMROOT'])
		contents = os.listdir(dotNetDir)
		versionDirs = []
		for d in contents:
			p = join(dotNetDir, d)
			isVersionDir = os.path.isdir(p) and d.startswith('v') and d[1].isdigit()
			if isVersionDir:
				versionDirs.append(d)
		versionDirs.sort()
		return join(dotNetDir, versionDirs[len(versionDirs)-1])

	def GenerateSWIGOutput(self, platformName, arch, config):
		'''Generate the SWIG API binding for a given platform, arch and config'''
		pathMan = BuildUtil.PathManager(platformName)
		if(arch == None):
			self.logger.critical('Generating SWIG binding [{}] [{}]'.format(platformName, config))
			cmdToRunNow = 'python {} {}'.format(pathMan.Paths['Src_Script_GenApi'] ,platformName)
		else:
			self.logger.critical('Generating SWIG binding [{}] [{}] [{}]'.format(platformName, arch, config))
			cmdToRunNow = 'python {} -a {} {}'.format(pathMan.Paths['Src_Script_GenApi'],arch ,platformName)
		self.logger.critical('Command is ' + cmdToRunNow )
		os.system(cmdToRunNow)


class MultiArchBuilder(PlatformBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		PlatformBuilder.__init__(self, platformName, arches, configs, generateSwig)

		isMultiArchPlatform = self.platformName in BuildUtil.SupportedArches.keys()
		if not isMultiArchPlatform:
			msg = 'Not a supported multi-architecture platform: {}. Instantiate from a single-arch prototype (PlatformBuilder) instead, or add {} to supported multi-arch platform list, available options: {}.'.format(self.platformName, self.platformName, ', '.join(BuildUtil.SupportedArches.keys()))
			self.logger.error(msg)
			raise RuntimeError(msg)

		isBuildAllArches = self.arches is None
		if isBuildAllArches:
			self.arches = BuildUtil.SupportedArches[self.platformName]

	def Build(self):
		'''Prototype of multi-architecture build.'''
		results = []
		for a in self.arches:
			for c in self.configs:
				self.logger.info('Building: {} ({}, {}) ...'.format(self.platformName, a, c))
				cmds = self._CreateCommands(a, c)
				isSuccess = self._RunCommands(cmds)
				if isSuccess:
					resultCode = 'Success'
				else:
					resultCode = 'Fail'
				result = {'resultCode': resultCode, 'Message': '{}({}, {})'.format(self.platformName, a, c)}
				results.append(result)
		return results

# Class for new platforms requiring VS2012
class VS2012Builder(PlatformBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		PlatformBuilder.__init__(self, platformName, arches, configs, generateSwig)
		
		CompilerKey = 'VS110COMNTOOLS'
		if CompilerKey in os.environ:
			self.compilerVersion = 'VS2012'
			self.compiler = join(self._FindLatestDotNetVersionDir(), 'MSBuild.exe')
			if not exists(self.compiler):
				msg = 'Failed to find compiler at: {}. Aborted.'.format(self.compiler)
				self.logger.error(msg)
				raise RuntimeError(msg)
		else:
			msg = 'VS2012 needed to compile, but not found. Aborted.'
			self.logger.error(msg)
			raise RuntimeError(msg)

		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], '{}{}.sln'.format(pathMan.ProductName, self.platformName))
		
	def _CreateCommands(self, arch=None, config='Profile'):
		# Edit name back to Visual Studio arch name.
		cmds = []
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)
		
		for t in self.tasks:
			cmd = ['{}'.format(self.compiler), '{}'.format(self.solution), '/t:{}'.format(t), '/p:Configuration={}'.format(config)]
			cmds.append(cmd)
		return cmds
		
class VS2015Builder(PlatformBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		PlatformBuilder.__init__(self, platformName, arches, configs, generateSwig)
		
		CompilerKey = 'VS140COMNTOOLS'
		if CompilerKey in os.environ:
			# Compiler command does not include .exe. Do not add it to the path.
			self.compilerVersion = 'VS2015'
			self.compiler = 'c:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\MSBuild.exe'
			# self.compiler = normpath(join(os.environ[CompilerKey], pardir, 'ide', 'devenv.exe'))
			if not exists(self.compiler):
				msg = 'Failed to find compiler at: {} based on environment variable {} = {}. Aborted.'.format(self.compiler, CompilerKey, os.environ[CompilerKey])
				self.logger.error(msg)
				raise RuntimeError(msg)
		else:
			msg = 'Could not find Visual Studio 2015. You need it to compile Windows Store plugins. Aborted.'
			self.logger.error(msg)
			raise RuntimeError(msg)

		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], '{}{}.sln'.format(pathMan.ProductName, self.platformName))
		
	def _CreateCommands(self, arch=None, config='Profile'):
		# Edit name back to Visual Studio arch name.
		cmds = []
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)
		
		for t in self.tasks:
			cmd = ['{}'.format(self.compiler), '{}'.format(self.solution), '/t:{}'.format(t), '/p:Configuration={}'.format(config)]
			cmds.append(cmd)
		return cmds

#
# Windows platforms
#
class WindowsBuilder(MultiArchBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		MultiArchBuilder.__init__(self, platformName, arches, configs, generateSwig)

		CompilerKey = 'VS120COMNTOOLS'
		if CompilerKey in os.environ:
			self.compilerVersion = 'VS2013'
			self.compiler = normpath(join(os.environ[CompilerKey], pardir, 'ide', 'devenv.exe'))
			if not exists(self.compiler):
				msg = 'Failed to find compiler at: {}. Aborted.'.format(self.compiler)
				self.logger.error(msg)
				raise RuntimeError(msg)
		else:
			msg = 'Visual Studio 2013 not found. Aborted.'
			self.logger.error(msg)
			raise RuntimeError(msg)

		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], '{}{}.sln'.format(pathMan.ProductName, self.platformName))

	def _CreateCommands(self, arch=None, config='Profile'):
		cmds = []
		
		# Call the GenerateAPIBinding script if required
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)
		
		for t in self.tasks:
			cmd = ['{}'.format(self.compiler), '{}'.format(self.solution), '/{}'.format(t), '{}|{}'.format(config, arch)]
			cmds.append(cmd)
			
		return cmds

class PS4Builder(VS2012Builder):
	def __init__(self, platformName, arches, configs, generateSwig):
		VS2012Builder.__init__(self, platformName, arches, configs, generateSwig)

		self.FixedArch = 'PS4'

		self._ValidateEnvVar('SCE_ORBIS_SDK_DIR', 'SCE_ROOT_DIR')
		
class SwitchBuilder(PlatformBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		PlatformBuilder.__init__(self, platformName, arches, configs, generateSwig)
		
		CompilerKey = 'VS140COMNTOOLS'
		if CompilerKey in os.environ:
			# Compiler command does not include .exe. Do not add it to the path.
			self.compilerVersion = 'VS2015'
			self.compiler = 'c:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\MSBuild.exe'
			# self.compiler = normpath(join(os.environ[CompilerKey], pardir, 'ide', 'devenv.exe'))
			if not exists(self.compiler):
				msg = 'Failed to find compiler at: {} based on environment variable {} = {}. Aborted.'.format(self.compiler, CompilerKey, os.environ[CompilerKey])
				self.logger.error(msg)
				raise RuntimeError(msg)
		else:
			msg = 'Could not find Visual Studio 2015. You need it to compile Windows Store plugins. Aborted.'
			self.logger.error(msg)
			raise RuntimeError(msg)

		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], '{}Switch.sln'.format(pathMan.ProductName))
		self.FixedArch = 'Switch'
		
	def _CreateCommands(self, arch=None, config='Profile'):
		
		# Call the GenerateAPIBinding script if required
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)
		
		cmds = []
		for t in self.tasks:
			cmd = ['{}'.format(self.compiler), '{}'.format(self.solution), '/t:{}'.format(t), '/p:Configuration={}'.format(config, arch)]
			cmds.append(cmd)
		return cmds

class XboxOneBuilder(VS2015Builder):
	def __init__(self, platformName, arches, configs, generateSwig):
		VS2015Builder.__init__(self, platformName, arches, configs, generateSwig)

		self.FixedArch = 'Xbox One'

class VitaBuilder(PlatformBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		PlatformBuilder.__init__(self, platformName, arches, configs, generateSwig)

		CompilerKey = 'VS110COMNTOOLS'
		if CompilerKey in os.environ:
			# Compiler command does not include .exe. Do not add it to the path.
			self.compiler = normpath(join(os.environ[CompilerKey], pardir, 'ide', 'devenv'))
			if not exists(self.compiler+'.exe'):
				msg = 'Failed to find compiler at: {} based on environment variable {} = {}. Aborted.'.format(self.compiler, CompilerKey, os.environ[CompilerKey])
				self.logger.error(msg)
				raise RuntimeError(msg)
		else:
			msg = 'Visual Studio 2012 not found. Aborted.'
			self.logger.error(msg)
			raise RuntimeError(msg)

		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], '{}{}.sln'.format(pathMan.ProductName, self.platformName))

		self.FixedArch = 'PSVita'

		# For each software config, add its hardware config to build too.
		configs = copy.deepcopy(self.configs)
		for c in self.configs:
			configs.append('{}_HW'.format(c))
		self.configs = copy.deepcopy(configs)

	def _CreateCommands(self, arch=None, config='Profile'):
		cmds = []
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)

		for t in self.tasks:
			cmd = ['{}'.format(self.compiler), '{}'.format(self.solution), '/{}'.format(t), '{}|{}'.format(config, self.FixedArch)]
			cmds.append(cmd)
		return cmds

class WSABuilder(MultiArchBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		MultiArchBuilder.__init__(self, platformName, arches, configs, generateSwig)

		CompilerKey = 'VS140COMNTOOLS'
		if CompilerKey in os.environ:
			# Compiler command does not include .exe. Do not add it to the path.
			self.compilerVersion = 'VS2015'
			self.compiler = 'c:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\MSBuild.exe'
			# self.compiler = normpath(join(os.environ[CompilerKey], pardir, 'ide', 'devenv.exe'))
			if not exists(self.compiler):
				msg = 'Failed to find compiler at: {} based on environment variable {} = {}. Aborted.'.format(self.compiler, CompilerKey, os.environ[CompilerKey])
				self.logger.error(msg)
				raise RuntimeError(msg)
		else:
			msg = 'Could not find Visual Studio 2015. You need it to compile Windows Store plugins. Aborted.'
			self.logger.error(msg)
			raise RuntimeError(msg)

		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], '{}WSA.sln'.format(pathMan.ProductName))

	def _CreateCommands(self, arch=None, config='Profile'):
		
		# Call the GenerateAPIBinding script if required
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)
		
		# Edit name back to Visual Studio arch name.
		if arch in BuildUtil.SupportedArches['WSA']:
			prefix = 'WSA_'
			arch = arch[len(prefix):]
			arch = arch.replace('WindowsPhone81_', '');
			arch = arch.replace('UWP_', '');
		else:
			msg = 'Undefined architecture: {}, available options: {}'.format(arch, ', '.join(BuildUtil.SupportedArches['WSA']))
			self.logger.error(msg)
			raise RuntimeError(msg)
		
		cmds = []
		for t in self.tasks:
			cmd = ['{}'.format(self.compiler), '{}'.format(self.solution), '/t:{}'.format(t), '/p:Configuration={};Platform={}'.format(config, arch)]
			cmds.append(cmd)
		return cmds


class AndroidMacBuilder(MultiArchBuilder):

	def __init__(self, platformName, arches, configs, generateSwig):
		MultiArchBuilder.__init__(self, platformName, arches, configs, generateSwig)
		self.compiler = '/bin/sh'
		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], 'BuildAndroidPluginMac.sh')
		self._ValidateEnvVar(varName='ANDROID_NDK_ROOT', entityName='Android NDK')
		self._ValidateEnvVar(varName='ANDROID_HOME', entityName='Android SDK')
		self._ValidateEnvVar(varName='ANT_HOME', entityName='Apache Ant')

	def _CreateCommands(self, arch=None, config='Profile'):
		
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)

		cmd = [self.compiler, self.solution, arch, config]
		return [cmd]


class AndroidWinBuilder(AndroidMacBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		AndroidMacBuilder.__init__(self, platformName, arches, configs, generateSwig)
		self._ValidateEnvVar(varName='CYGWIN_HOME', entityName='Cygwin')
		self.compiler = 'cmd'
		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], 'BuildAndroidPluginWin.cmd')

	def _CreateCommands(self, arch=None, config='Profile'):
	
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)
		
		# Note on Windows, do not double-quote program paths.
		cmd = [self.solution, arch, config]
		return [cmd]

class LinuxBuilder(MultiArchBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		MultiArchBuilder.__init__(self, platformName, arches, configs, generateSwig)
		self.compiler = 'make'
		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = 'AkSoundEngineLinux.make'

	def _CreateCommands(self, arch=None, config='Profile'):
		pathMan = BuildUtil.PathManager(self.platformName)

		# Generate only the API if we are on Windows, the plugins have already been built
		# by the Linux job.
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)
		
		if platform.system() == 'Windows':
			return []
		
		if arch == 'x86':
			configString = 'config=' + config.lower() + '32'
			chrootPlat = 'steamrt_scout_i386'
		else:
			configString = 'config=' + config.lower() + '64'
			chrootPlat = 'steamrt_scout_amd64'
		
		cmd = ['schroot', '--chroot', chrootPlat, '--', self.compiler, '-f', self.solution, configString]
		#cmd = ['schroot', '--chroot', chrootPlat, self.compiler, '-f', self.solution, configString]
		return [cmd]
	
#
# Mac platforms
#
class MacBuilder(PlatformBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		PlatformBuilder.__init__(self, platformName, arches, configs, generateSwig)
		self.compiler = 'xcodebuild'
		pathMan = BuildUtil.PathManager(self.platformName)
		self.solution = join(pathMan.Paths['Src_Platform'], '{}{}.xcodeproj'.format(pathMan.ProductName, self.platformName))

	def _CreateCommands(self, arch=None, config='Profile'):
		
		# Call the GenerateAPIBinding script if required
		if self.generateSwig:
			self.GenerateSWIGOutput(self.platformName, arch, config)

		cmd = [self.compiler, '-project', self.solution, '-configuration', config, 'WWISESDK={}'.format(os.environ['WWISESDK']), 'clean', 'build']
		return [cmd]
		
		
class iOSBuilder(MacBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		MacBuilder.__init__(self, platformName, arches, configs, generateSwig)

class tvOSBuilder(MacBuilder):
	def __init__(self, platformName, arches, configs, generateSwig):
		MacBuilder.__init__(self, platformName, arches, configs, generateSwig)

class WwiseUnityBuilder(object):
	'''Main console utility for building Wwise Unity Integration.'''

	def __init__(self, args):
		self.platforms = args.platforms
		self.arches = args.arches
		self.configs = args.configs
		self.wwiseSdkDir = args.wwiseSdkDir
		self.androidSdkDir = args.androidSdkDir
		self.androidNdkDir = args.androidNdkDir
		self.apacheAntdir = args.apacheAntdir
		self.isUpdatePref = args.isUpdatePref
		self.isGenerateswigbinding = args.isGenerateswigbinding

		self.logger = BuildUtil.CreateLogger(BuildUtil.DefaultLogFile, __file__, self.__class__.__name__)

	def CreatePlatformBuilder(self, platformName, arches, configs, generateSwig):
		builder = None
		if platformName == 'Windows':
			builder = WindowsBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'XBoxOne':
			builder = XboxOneBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'PS4':
			builder = PS4Builder(platformName, arches, configs, generateSwig)
		elif platformName == 'Android' and platform.system() == 'Darwin':
			builder = AndroidMacBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'Android' and platform.system() == 'Windows':
			builder = AndroidWinBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'Mac':
			builder = MacBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'iOS':
			builder = iOSBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'tvOS':
			builder = tvOSBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'WSA':
			builder = WSABuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'Vita':
			builder = VitaBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'Linux':
			builder = LinuxBuilder(platformName, arches, configs, generateSwig)
		elif platformName == 'Switch':
			builder = SwitchBuilder(platformName, arches, configs, generateSwig)
		else:
			# NOTE: Do not raise error here. Skip to allow batch build to continue with next platform.
			self.logger.error('Undefined platform: {}. Skipped.'.format(platformName))
		return builder

	def Build(self):
		self.logger.critical('Build started. {}'.format(BuildUtil.Messages['CheckLogs']))

		self._SetupEnvironment(name=BuildUtil.WwiseSdkEnvVar, value=self.wwiseSdkDir)
		if 'Android' in self.platforms:
			self._SetupEnvironment(name=BuildUtil.AndroidSdkEnvVar, value=self.androidSdkDir)
			self._SetupEnvironment(name=BuildUtil.AndroidNdkEnvVar, value=self.androidNdkDir)
			self._SetupEnvironment(name=BuildUtil.ApacheAntEnvVar, value=self.apacheAntdir)

		results = [] # Each platformResult is a pair of [code, msg]
		for p in self.platforms:
			builder = self.CreatePlatformBuilder(p, self.arches, self.configs, self.isGenerateswigbinding)
			if builder is None:
				platformResults = self._GetUndefinedPlatformBuildResults()
			else:
				platformResults = builder.Build()
			results += platformResults

		return self._Report(results)

	def _SetupEnvironment(self, name, value):
		'''Update preference file and export to environment for the build process.'''
		if self.isUpdatePref:
			try:
				BuildUtil.WritePreferenceField(name, value)
			except Exception as e:
				msg = 'Failed to update preference field: {}, with error: {}. Aborted.'.format(name, e)
				self.logger.error(msg)
				raise RuntimeError(msg)

		# Export this env var because solution inclue/library paths depend on it.
		os.environ[name] = value
		self.logger.info('Environment variable updated: now {} = {}.'.format(name, value))

	def _GetUndefinedPlatformBuildResults(self):
		return ['Fail', 'UnsupportedPlatform']

	def _Report(self, results):
		'''Log build results. Result = ['Fail'/'Success', 'Platform(arch, config)']'''

		reports = { 'Fail': [], 'Success': []}
		for r in results:
			msg = '{}'.format(r['Message'])
			key = r['resultCode']
			reports[key].append(msg)

		msg = 'List of Failed Builds: {}'.format(', '.join(reports['Fail']))
		self.logger.critical(msg)
		msg = 'List of Succeeded Builds: {}'.format(', '.join(reports['Success']))
		self.logger.critical(msg)
		isNoFails = len(reports['Fail']) == 0
		if isNoFails:
			self.logger.critical('*BUILD SUCCEEDED*.{}'.format(BuildUtil.SpecialChars['PosixLineEnd']*2))
			return True
		else:
			msg = '***BUILD FAILED***. {}{}'.format(BuildUtil.Messages['CheckLogs'], BuildUtil.SpecialChars['PosixLineEnd']*3)
			self.logger.critical(msg)
			return False


def ParseAndValidateArgs(argv=None):
	logger = BuildUtil.CreateLogger(BuildUtil.DefaultLogFile, __file__, ParseAndValidateArgs.__name__)

	ProgName = os.path.splitext(os.path.basename(__file__))[0]

	Epilog = '''
	Examples:
	# Build for Windows 32bit Debug.
	python {} -p Windows -a Win32 -c Debug
	# Build for all configurations for Android armeabi-v7a architecture.
	python {} -p Android -a armeabi-v7a
	# Build for Mac Profile with a specified Wwise SDK location and update the preference.
	python {} -p Mac -c Profile -w /Users/me/Wwise/wwise_v2013.2_build_4800/SDK -u
	# Build for all supported platforms by the current Unity Editor platform.
	python {}
	'''.format(__file__, __file__, __file__, __file__)
	Epilog = '\n'.join(Epilog.splitlines(True))

	parser = argparse.ArgumentParser(prog=ProgName, description='Main console utility for building Wwise Unity Integration.', add_help=True, epilog=Epilog, formatter_class=RawTextHelpFormatter)
	supportedPlatforms = BuildUtil.SupportedPlatforms[platform.system()]
	parser.add_argument('-v', '--version', action='version', version='%(prog)s {}'.format(BuildUtil.Version))
	parser.add_argument('-p', '--platforms', nargs='+', dest='platforms', default=supportedPlatforms, help='One or more target platforms to build, default to {} if the option or its arguments are not specified.'.format(', '.join(supportedPlatforms)))
	parser.add_argument('-a', '--arches', nargs='+', dest='arches', default=None, help='One or more target architectures to build for certain platforms, available options: Windows: {}, Android: {}; WSA: {}, Linux: {}, default to None if none given. Here None represents all supported architectures. When combined with -p, -p must receive only one platform and it must be a multi-architecture one.'.format(', '.join(BuildUtil.SupportedArches['Windows']), ', '.join(BuildUtil.SupportedArches['Android']), ', '.join(BuildUtil.SupportedArches['WSA']), ', '.join(BuildUtil.SupportedArches['Linux'])))
	parser.add_argument('-c', '--configs', nargs='+', dest='configs', default=BuildUtil.SupportedConfigs, help='One or more target configurations to build, available options: {}, default to all configurations if none given.'.format(', '.join(BuildUtil.SupportedConfigs)))

	Undefined = 'Undefined'
	envWwiseSdkDir = Undefined
	if BuildUtil.WwiseSdkEnvVar in os.environ:
		envWwiseSdkDir = os.environ[BuildUtil.WwiseSdkEnvVar]
	parser.add_argument('-w', '--wwisesdkdir', nargs=1, dest='wwiseSdkDir', default=Undefined, help='The Wwise SDK folder to build the Integration against. Required if -u is used. Abort if the specified path is not found; if no path specified in arguments, fallback first to the preference file, then to the environment variable WWISESDK if the preference file is unavailable. Preference file location: {}, current WWISESDK = {}. For Android, no white spaces are allowed in the Wwise SDK folder path.'.format(BuildUtil.DefaultPrefFile, envWwiseSdkDir))
	parser.add_argument('-u', '--updatepref', action='store_true', dest='isUpdatePref', default=False, help='Flag to set whether or not to overwrite the relevant fields in the preference file with the user specified command options and arguments, default to unset (not to update).')
	parser.add_argument('-V', '--verbose', action='store_true', dest='isVerbose', default=False, help='Set the flag to show all log messages to console, default to unset (quiet).')
	parser.add_argument('-g', '--generateswigbinding', action='store_true', dest='isGenerateswigbinding', default=False, help='Generate SWIG binding before build process.')

	# Android only
	envSdkDir = Undefined
	if BuildUtil.AndroidSdkEnvVar in os.environ:
		envSdkDir = os.environ[BuildUtil.AndroidSdkEnvVar]
	envNdkDir = Undefined
	if BuildUtil.AndroidNdkEnvVar in os.environ:
		envNdkDir = os.environ[BuildUtil.AndroidNdkEnvVar]
	envAntDir = Undefined
	if BuildUtil.ApacheAntEnvVar in os.environ:
		envAntDir = os.environ[BuildUtil.ApacheAntEnvVar]
	parser.add_argument('-s', '--androidsdkdir', nargs=1, dest='androidSdkDir', default=Undefined, help='The Android SDK folder to build the Integration with. Required if Android is among the input platforms. Abort if the specified path is not found; if not specified in arguments, fallback first to the preference file, then to the environment variable {} if the preference file is unavailable. Preference file location: {}, current {} = {}'.format(BuildUtil.AndroidSdkEnvVar, BuildUtil.DefaultPrefFile, BuildUtil.AndroidSdkEnvVar, envSdkDir))
	parser.add_argument('-n', '--androidndkdir', nargs=1, dest='androidNdkDir', default=Undefined, help='The Android NDK folder to build the Integration with. Required if Android is among the input platforms. Abort if the specified path is not found; if not specified in arguments, fallback first to the preference file, then to the environment variable {} if the preference file is unavailable. Preference file location: {}, current {} = {}'.format(BuildUtil.AndroidNdkEnvVar, BuildUtil.DefaultPrefFile, BuildUtil.AndroidNdkEnvVar, envNdkDir))
	parser.add_argument('-t', '--apacheantdir', nargs=1, dest='apacheAntdir', default=Undefined, help='The Apache Ant folder to build the Integration with. Required if Android is among the input platforms. Abort if the specified path is not found; if not specified in arguments, fallback first to the preference file, then to the environment variable {} if the preference file is unavailable. Preference file location: {}, current {} = {}'.format(BuildUtil.ApacheAntEnvVar, BuildUtil.DefaultPrefFile, BuildUtil.ApacheAntEnvVar, envAntDir))

	if argv is None:
		args = parser.parse_args()
	else: # for unittest
		args = parser.parse_args(argv[1:])

	DefaultMsg = 'Use -h for help. Aborted'
	for p in args.platforms:
		if not p in BuildUtil.SupportedPlatforms[platform.system()]:
			logger.error('Found unsupported platform: {}. {}.'.format(p, DefaultMsg))
			return None

	# Arch only works for a single specified platform, and is ignored otherwise
	if not args.arches is None:
		if len(args.platforms) != 1:
			logger.error('Found {} platform(s) when using -a. Must use only one multi-architecture platform instead. {}.'.format(len(args.platforms), DefaultMsg))
			return None
		for a in args.arches:
			if not args.platforms[0] in BuildUtil.SupportedArches.keys():
				logger.error('Found single-architecture platform {} when using -a. Must not use any single-architecture platforms. Use only one multi-architecture platform when using -a. {}.'.format(args.platforms[0], DefaultMsg))
				return None
			if not a in BuildUtil.SupportedArches[args.platforms[0]]:
				logger.error('Found unsupported architecture {} for the platform {}. {}.'.format(a, args.platforms[0], DefaultMsg))
				return None

	for c in args.configs:
		if not c in BuildUtil.SupportedConfigs:
			logger.error('Found unsupported configuration {}. {}.'.format(c, DefaultMsg))
			return None

	# Policy: When calling as external process from Unity Editor (Mono), env var won't work.
	# So first check if Wwise SDK folder is specified in command;
	# if specified, check if the path exists and then if -u is on,
	# export to the preference file.
	# if not specified, then try to parse from the preference file;
	# if all above fail, check env var.
	# if -w is not specified, do not update pref at all.
	[args.wwiseSdkDir, isWwiseSdkSpecifiedInCmd] = ValidateLocationVar(varName=BuildUtil.WwiseSdkEnvVar, inVarValue=args.wwiseSdkDir, entityName='Wwise SDK folder', cliSwitch='-w')
	if args.wwiseSdkDir is None:
		return None
	if exists(args.wwiseSdkDir):
		if 'Android' in args.platforms and BuildUtil.SpecialChars['WhiteSpace'] in args.wwiseSdkDir:
			logger.error('Wwise SDK folder contains white spaces. Android build will fail. Consider removing all white spaces in Wwise SDK folder path: {}. {}.'.format(args.wwiseSdkDir, DefaultMsg))
			return None
	else:
		logger.error('Failed to find Wwise SDK folder: {}. {}.'.format(args.wwiseSdkDir, DefaultMsg))
		return None

	# Android only: See policy for -w
	isAndroidSdkSpecifiedInCmd = False
	isAndroidNdkSpecifiedInCmd = False
	isApacheAntSpecifiedInCmd = False
	if 'Android' in args.platforms:
		[args.androidSdkDir, isAndroidSdkSpecifiedInCmd] = ValidateLocationVar(varName=BuildUtil.AndroidSdkEnvVar, inVarValue=args.androidSdkDir, entityName='Android SDK folder', cliSwitch='-s')
		if args.androidSdkDir is None:
			return None
		[args.androidNdkDir, isAndroidNdkSpecifiedInCmd] = ValidateLocationVar(varName=BuildUtil.AndroidNdkEnvVar, inVarValue=args.androidNdkDir, entityName='Android NDK folder', cliSwitch='-n')
		if args.androidNdkDir is None:
			return None
		[args.apacheAntdir, isApacheAntSpecifiedInCmd] = ValidateLocationVar(varName=BuildUtil.ApacheAntEnvVar, inVarValue=args.apacheAntdir, entityName='Android Apache Ant folder', cliSwitch='-t')
		if args.apacheAntdir is None:
			return None

	if args.isUpdatePref and [isWwiseSdkSpecifiedInCmd, isAndroidSdkSpecifiedInCmd, isAndroidNdkSpecifiedInCmd, isApacheAntSpecifiedInCmd] == [False, False, False, False]:
		logger.error('Found -u without one of -w, -n, -s, or -t. Cannot update preference without those options specified. {}.'.format(DefaultMsg))
		return None

	BuildUtil.IsVerbose = args.isVerbose

	return args

def ValidateLocationVar(varName, inVarValue, entityName, cliSwitch):
	'''Retrieve an environment variable with our 3-layer setup and also check if the variable is from CLI.'''
	logger = BuildUtil.CreateLogger(BuildUtil.DefaultLogFile, __file__, ValidateLocationVar.__name__)
	DefaultMsg = 'Use -h for help. Aborted'

	Undefined = 'Undefined'
	location = Undefined
	isLocationSpecifiedInCmd = inVarValue != Undefined
	if isLocationSpecifiedInCmd:
		# Assume input value is the argparse vector arg.
		location = inVarValue[0]
	else:
		prefLocation = BuildUtil.ReadPreferenceField(varName)
		if not prefLocation is None:
			logger.warn('No {} specified ({}). Fall back to use preference ({}): {}'.format(entityName, cliSwitch, BuildUtil.DefaultPrefFile, prefLocation))
			location = prefLocation
		else:
			if varName in os.environ:
				logger.warn('No {} specified ({}) and no preference found. Fall back to use environment variable: {} = {}'.format(entityName, cliSwitch, varName, os.environ[varName]))
				location = os.environ[varName]
			else:
				logger.error('Undefined environment variable: {}. {}.'.format(varName, DefaultMsg))
				return [None, isLocationSpecifiedInCmd]
	if not exists(location):
		logger.error('Failed to find {}: {}. {}.'.format(entityName, location, DefaultMsg))
		return [None, isLocationSpecifiedInCmd]

	return [location, isLocationSpecifiedInCmd]


def main(argv=None):
	args = ParseAndValidateArgs(argv)
	if args is None:
		return 1

	builder = WwiseUnityBuilder(args)
	result = builder.Build()
	if result == True:
		return 0
	else:
		return 1

if __name__ == '__main__':
	sys.exit(main(sys.argv))
