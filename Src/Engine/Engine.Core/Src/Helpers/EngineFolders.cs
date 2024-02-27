using System;
using System.IO;
using System.Reflection;
using AngryWasp.Helpers;

namespace Engine.Helpers
{
	/// <summary>
	/// For interacting with virtual file paths relative to the content directory
	/// </summary>
	public class EngineFolders
	{
		private static string contentPath;
		private static string binaryPath;
		private static string logPath;
		private static string settingPath;

		public static string ContentPath => contentPath;
        
		public static string LogPath => logPath;

		public static string SettingPath => settingPath;

		public static void Initialize(string engineRoot)
		{
            if (string.IsNullOrEmpty(engineRoot))
                throw new Exception("Engine directory not specified");

            if (!Directory.Exists(engineRoot))
                throw new Exception("Engine directory does not exist");

			binaryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			contentPath = Path.Combine(engineRoot, "Content").NormalizeFilePath();
			logPath = Path.Combine(engineRoot, "Logs").NormalizeFilePath();
			settingPath = Path.Combine(engineRoot, "Settings").NormalizeFilePath();

			if (!Directory.Exists(contentPath))
				Directory.CreateDirectory(contentPath);

			if (!Directory.Exists(logPath))
				Directory.CreateDirectory(logPath);

			if (!Directory.Exists(settingPath))
				Directory.CreateDirectory(settingPath);
		}

		public static string ContentPathRealToVirtual(string realPath) => FileHelper.MakeRelative(realPath, contentPath).NormalizeFilePath();

        public static string BinaryPathVirtualToReal(string virtualPath) => Path.Combine(binaryPath, virtualPath).NormalizeFilePath();

		public static string ContentPathVirtualToReal(string virtualPath) => Path.Combine(contentPath, virtualPath).NormalizeFilePath();

        public static string LogPathVirtualToReal(string virtualPath) => Path.Combine(logPath, virtualPath).NormalizeFilePath();

        public static string SettingsPathVirtualToReal(string virtualPath) => Path.Combine(settingPath, virtualPath).NormalizeFilePath();
	}
}