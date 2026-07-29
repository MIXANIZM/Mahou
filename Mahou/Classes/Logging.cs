// MIXANIZM hardening: non-blocking, bounded, AppData-safe logging.
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace Mahou {
    public static class Logging {
        static readonly object Sync = new object();
        static readonly ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();
        const long MaxLogSize = 5L * 1024L * 1024L;
        const int MaxQueuedMessages = 5000;

        public static string logdir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MIXANIZM Mahou", "Logs");
        public static string log = Path.Combine(logdir, DateTime.Today.ToString("yyyy.MM.dd") + ".txt");

        public static void SetDirectory(string directory) {
            if (String.IsNullOrWhiteSpace(directory)) return;
            lock (Sync) {
                logdir = directory;
                log = Path.Combine(logdir, DateTime.Today.ToString("yyyy.MM.dd") + ".txt");
            }
        }

        public static void Log(string logmsg, int msgtype = 0) {
            if (!MahouUI.LoggingEnabled && msgtype == 0) return;
            var kind = msgtype == 1 ? "E" : msgtype == 2 ? "W" : "I";
            var message = DateTime.Now.ToString("HH:mm:ss.fff") + " [" + kind + "]: " +
                          (logmsg ?? String.Empty) + Environment.NewLine;
            while (Messages.Count >= MaxQueuedMessages) {
                string ignored;
                if (!Messages.TryDequeue(out ignored)) break;
            }
            Messages.Enqueue(message);
        }

        public static void UpdateLog() {
            lock (Sync) {
                Directory.CreateDirectory(logdir);
                log = Path.Combine(logdir, DateTime.Today.ToString("yyyy.MM.dd") + ".txt");
                RotateIfNeeded();
                string message;
                while (Messages.TryDequeue(out message)) {
#if VSCDEBUG
                    Console.Write(message);
#elif DEBUG
                    Debug.Write(message);
#else
                    File.AppendAllText(log, message);
#endif
                }
            }
        }

        static void RotateIfNeeded() {
            try {
                if (!File.Exists(log) || new FileInfo(log).Length < MaxLogSize) return;
                var archive = Path.Combine(logdir,
                    DateTime.Now.ToString("yyyy.MM.dd-HHmmss") + ".txt");
                File.Move(log, archive);
                var files = new DirectoryInfo(logdir).GetFiles("*.txt");
                Array.Sort(files, (a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
                for (var i = 10; i < files.Length; i++) files[i].Delete();
            } catch (Exception ex) {
                Debug.WriteLine("Log rotation failed: " + ex.Message);
            }
        }
    }
}
