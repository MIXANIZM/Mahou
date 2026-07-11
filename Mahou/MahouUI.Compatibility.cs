using System.Collections.Generic;

namespace Mahou {
    /// <summary>
    /// Non-network fields retained from the modern upstream UI. The legacy
    /// updater and public-sync endpoints stay removed; only shared state still
    /// referenced by keyboard, translation and local import/export code lives
    /// here.
    /// </summary>
    public partial class MahouUI {
        public static List<int> HKBlockAlt = new List<int>();
        public static bool BlockAltUpNOW = false;
        static bool isold = true, snip_checking, as_checking;
        public static Dictionary<string, string> TrSetsValues = new Dictionary<string, string>();
        static string latestSwitch = "null";

        const string SYNC_SEP = "#------>";
        readonly string[] SYNC_NAMES = { "Mahou.ini", "snippets.txt", "history.txt", "TSDict.txt", "Mahou.mm" };
        readonly string[] SYNC_TYPES = { "ini", "sni", "his", "tdi", "mm" };
    }
}
