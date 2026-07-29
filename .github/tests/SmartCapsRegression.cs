using System;
using System.Reflection;

static class SmartCapsRegression {
    static int failures;
    static MethodInfo tryBuildCorrection;
    static MethodInfo normalizeExceptionKey;

    static void Check(bool condition, string message) {
        if (condition) return;
        failures++;
        Console.Error.WriteLine("FAIL: " + message);
    }

    static void Converts(string source, string expected) {
        var args = new object[] { source, null };
        var matched = (bool)tryBuildCorrection.Invoke(null, args);
        Check(matched, "candidate was not corrected: " + source);
        Check(String.Equals((string)args[1], expected, StringComparison.Ordinal),
              "unexpected correction for " + source + ": " + (string)args[1]);
    }

    static void Skips(string source) {
        var args = new object[] { source, null };
        var matched = (bool)tryBuildCorrection.Invoke(null, args);
        Check(!matched, "safe no-op candidate was corrected: " + source);
    }

    static int Main(string[] args) {
        if (args.Length != 1) {
            Console.Error.WriteLine("Usage: SmartCapsRegression <Mahou.exe>");
            return 2;
        }

        var assembly = Assembly.LoadFrom(args[0]);
        var smartCaps = assembly.GetType("Mahou.SmartCaps", true);
        tryBuildCorrection = smartCaps.GetMethod("TryBuildCorrection", BindingFlags.Static | BindingFlags.NonPublic);
        normalizeExceptionKey = smartCaps.GetMethod("NormalizeExceptionKey", BindingFlags.Static | BindingFlags.NonPublic);
        if (tryBuildCorrection == null || normalizeExceptionKey == null) {
            Console.Error.WriteLine("FAIL: Smart Caps safety helpers not found");
            return 1;
        }

        Converts("ПРивет", "Привет");
        Converts("ПРИвет", "Привет");
        Converts("ПРИВет", "Привет");
        Converts("окоРОчка", "окорочка");
        Converts("КуРиные", "Куриные");
        Converts("ABc", "Abc");
        Converts("aBc", "abc");
        Converts("ÉTUde", "Étude");
        Converts("САнкт-ПЕтербург", "Санкт-Петербург");
        Converts("McDonald", "Mcdonald");
        Converts("СДЭКом", "Сдэком");

        Skips("США");
        Skips("USA");
        Skips("Привет");
        Skips("привет");
        Skips("Санкт-Петербург");
        Skips("A1b");
        Skips("ПRивет");
        Skips("AБc");
        Skips("ΑΒγ");
        Skips("ПР");
        Skips("test@example.com");
        Skips("https://Example.test");

        var normalized = (string)normalizeExceptionKey.Invoke(null, new object[] { "  СДЭКом  " });
        Check(String.Equals(normalized, "сдэком", StringComparison.Ordinal),
              "exception normalization is not trim + lowercase");

        if (failures != 0) return 1;
        Console.WriteLine("Smart Caps regression passed.");
        return 0;
    }
}
