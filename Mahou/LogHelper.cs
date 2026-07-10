using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Mahou
{
    internal static class LogHelper
    {
        private const long MaxLogFileBytes = 5L * 1024L * 1024L;
        private const int MaxArchiveFiles = 10;

        public static string LogDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Configs.DataDirectoryName,
                    "Logs");
            }
        }

        public static void ConfigureNlog()
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                string archiveDirectory = Path.Combine(LogDirectory, "Archive");
                Directory.CreateDirectory(archiveDirectory);

                var config = new LoggingConfiguration();
                var fileTarget = new FileTarget
                {
                    FileName = Path.Combine(LogDirectory, "${shortdate}.log"),
                    Layout = "${longdate} ${uppercase:${level}} ${message} ${exception:format=toString}",
                    ArchiveFileName = Path.Combine(archiveDirectory, "mahou.{#}.log"),
                    ArchiveAboveSize = MaxLogFileBytes,
                    MaxArchiveFiles = MaxArchiveFiles,
                    KeepFileOpen = false,
                    EnableFileDelete = true
                };

                config.AddTarget("file", fileTarget);

#if DEBUG
                var rule = new LoggingRule("*", LogLevel.Trace, fileTarget);
#else
                var rule = new LoggingRule("*", LogLevel.Warn, fileTarget);
#endif
                config.LoggingRules.Add(rule);
                LogManager.Configuration = config;
            }
            catch
            {
                // Logging must never prevent the keyboard utility from starting.
                LogManager.Configuration = new LoggingConfiguration();
            }
        }
    }
}
