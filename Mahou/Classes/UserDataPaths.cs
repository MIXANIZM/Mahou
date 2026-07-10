using System;
using System.IO;

namespace Mahou
{
    internal static class UserDataPaths
    {
        public static readonly string DataDirectory = Configs.dataPath;
        public static readonly string SnippetsFile = Path.Combine(DataDirectory, "snippets.txt");
        public static readonly string LegacySnippetsFile = Path.Combine(Update.nPath, "snippets.txt");

        public static void MigrateSnippets(MoreConfigs moreConfigs)
        {
            Directory.CreateDirectory(DataDirectory);

            if (!File.Exists(SnippetsFile) && File.Exists(LegacySnippetsFile))
            {
                try
                {
                    File.Copy(LegacySnippetsFile, SnippetsFile, false);
                }
                catch
                {
                    // Keep running; the user can save snippets again into AppData later.
                }
            }

            if (moreConfigs != null)
                moreConfigs.snipfile = SnippetsFile;
        }
    }
}
