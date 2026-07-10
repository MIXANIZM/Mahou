using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using NLog;

namespace Mahou
{
    class MMain
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [DllImport("user32.dll")]
        public static extern uint RegisterWindowMessage(string message);

        public const string appGUid = "ec511418-1d57-4dbe-a0c3-c6022b33735b";
        public static uint ao = RegisterWindowMessage("AlderyOpenedMahou!");

        public static List<KMHook.YuKey> c_word = new List<KMHook.YuKey>();
        public static List<List<KMHook.YuKey>> c_words = new List<List<KMHook.YuKey>>();
        public static IntPtr _hookID = IntPtr.Zero;
        public static IntPtr _mouse_hookID = IntPtr.Zero;
        public static KMHook.LowLevelProc _proc = KMHook.HookCallback;
        public static KMHook.LowLevelProc _mouse_proc = KMHook.MouseHookCallback;
        public static Locales.Locale[] locales = Locales.AllList();
        public static Configs MyConfs = new Configs();
        public static MahouForm mahou;
        public static List<string> lcnmid = new List<string>();
        public static string[] UI = { };
        public static string[] TTips = { };
        public static string[] Msgs = { };

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LogHelper.ConfigureNlog();
            log.Trace("Program start");

            using (var mutex = new Mutex(false, "Global\\" + appGUid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    KMHook.PostMessage((IntPtr)0xffff, ao, 0, 0);
                    return;
                }

                if (locales.Length < 2)
                {
                    Locales.IfLessThan2();
                    return;
                }

                InitLanguage();
                mahou = new MahouForm();
                UserDataPaths.MigrateSnippets(mahou.moreConfigs);
                StartupManager.MigrateLegacyShortcutSafe();
                mahou.icon.RefreshText(UI[44], UI[42], UI[43]);
                KMHook.ReInitSnippets();

                if (!StartHook())
                {
                    MessageBox.Show(
                        "Mahou could not install its input hooks. Restart the application or check security software.",
                        "MIXANIZM Mahou",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                InputOperationQueue.Start();
                AdaptiveLayoutLearning.Start();

                if (MyConfs.Read("Locales", "locale1Lang") == "" && MyConfs.Read("Locales", "locale2Lang") == "")
                {
                    MyConfs.Write("Locales", "locale1uId", locales[0].uId.ToString());
                    MyConfs.Write("Locales", "locale2uId", locales[1].uId.ToString());
                    MyConfs.Write("Locales", "locale1Lang", locales[0].Lang);
                    MyConfs.Write("Locales", "locale2Lang", locales[1].Lang);
                }

                try
                {
                    Application.Run();
                }
                catch (Exception ex)
                {
                    log.Fatal(ex, "Global error handler caught the exception in app");
                }
                finally
                {
                    AdaptiveLayoutLearning.Stop();
                    InputOperationQueue.Stop();
                    StopHook();
                }
            }
        }

        public static void InitLanguage()
        {
            string language = MyConfs.Read("Locales", "LANGUAGE");
            if (String.Equals(language, "RU", StringComparison.OrdinalIgnoreCase))
            {
                UI = Translation.UIRU;
                TTips = Translation.ToolTipsRU;
                Msgs = Translation.MessagesRU;
                return;
            }

            UI = Translation.UIEN;
            TTips = Translation.ToolTipsEN;
            Msgs = Translation.MessagesEN;
            if (!String.Equals(language, "EN", StringComparison.OrdinalIgnoreCase))
                MyConfs.Write("Locales", "LANGUAGE", "EN");
        }

        public static bool StartHook()
        {
            if (!CheckHook())
                return true;

            _mouse_hookID = KMHook.SetHook(_mouse_proc, (int)KMHook.KMMessages.WH_MOUSE_LL);
            _hookID = KMHook.SetHook(_proc, (int)KMHook.KMMessages.WH_KEYBOARD_LL);
            Thread.Sleep(10);

            bool success = _mouse_hookID != IntPtr.Zero && _hookID != IntPtr.Zero;
            if (!success)
            {
                log.Error("Failed to install one or more input hooks");
                StopHookHandles();
            }
            return success;
        }

        public static void StopHook()
        {
            if (CheckHook() && _mouse_hookID == IntPtr.Zero)
                return;
            StopHookHandles();
            Thread.Sleep(10);
        }

        private static void StopHookHandles()
        {
            if (_hookID != IntPtr.Zero)
                KMHook.UnhookWindowsHookEx(_hookID);
            if (_mouse_hookID != IntPtr.Zero)
                KMHook.UnhookWindowsHookEx(_mouse_hookID);
            _hookID = _mouse_hookID = IntPtr.Zero;
        }

        public static bool CheckHook()
        {
            return _hookID == IntPtr.Zero;
        }
    }
}
