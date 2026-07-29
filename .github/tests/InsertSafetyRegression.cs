using System;
using System.Reflection;

static class InsertSafetyRegression {
    static int failures;
    static MethodInfo findWordBounds;
    static MethodInfo supportedStandardEditClass;
    static MethodInfo findFreshTextBeforeCaret;

    static void Check(bool condition, string message) {
        if (condition) return;
        failures++;
        Console.Error.WriteLine("FAIL: " + message);
    }

    static int[] Bounds(string text, int caret) {
        var args = new object[] { text, caret, 256, 0, 0 };
        var found = (bool)findWordBounds.Invoke(null, args);
        return found ? new[] { (int)args[3], (int)args[4] } : null;
    }

    static int[] FreshBounds(string text, int caret, string expected, int maxBoundaryCharacters) {
        var args = new object[] { text, caret, expected, maxBoundaryCharacters, 0, 0 };
        var found = (bool)findFreshTextBeforeCaret.Invoke(null, args);
        return found ? new[] { (int)args[4], (int)args[5] } : null;
    }

    static void CheckFreshRange(string text, int caret, string expected, int expectedStart, int expectedEnd) {
        var bounds = FreshBounds(text, caret, expected, 4);
        Check(bounds != null, "fresh word not found before caret " + caret + " in [" + text + "]");
        if (bounds == null) return;
        Check(bounds[0] == expectedStart && bounds[1] == expectedEnd,
              "unexpected fresh range: " + bounds[0] + ".." + bounds[1] +
              ", expected " + expectedStart + ".." + expectedEnd);
    }

    static void CheckRange(string text, int caret, int expectedStart, int expectedEnd) {
        var bounds = Bounds(text, caret);
        Check(bounds != null, "word not found at caret " + caret + " in [" + text + "]");
        if (bounds == null) return;
        Check(bounds[0] == expectedStart && bounds[1] == expectedEnd,
              "unexpected range at caret " + caret + ": " + bounds[0] + ".." + bounds[1] +
              ", expected " + expectedStart + ".." + expectedEnd);

        var replacement = text.Substring(expectedStart, expectedEnd - expectedStart).ToUpperInvariant();
        var result = text.Substring(0, expectedStart) + replacement + text.Substring(expectedEnd);
        Check(result.Substring(0, expectedStart) == text.Substring(0, expectedStart),
              "left neighbor changed at caret " + caret);
        Check(result.Substring(expectedStart + replacement.Length) == text.Substring(expectedEnd),
              "right neighbor or separator changed at caret " + caret);
    }

    static int Main(string[] args) {
        if (args.Length != 1) {
            Console.Error.WriteLine("Usage: InsertSafetyRegression <Mahou.exe>");
            return 2;
        }

        var assembly = Assembly.LoadFrom(args[0]);
        var probe = assembly.GetType("Mahou.SelectionProbe", true);
        findWordBounds = probe.GetMethod("TryFindWordBounds", BindingFlags.Static | BindingFlags.NonPublic);
        supportedStandardEditClass = probe.GetMethod("IsSupportedStandardEditClass",
                                                     BindingFlags.Static | BindingFlags.NonPublic);
        findFreshTextBeforeCaret = probe.GetMethod("TryFindFreshTextBeforeCaret",
                                                    BindingFlags.Static | BindingFlags.NonPublic);
        if (findWordBounds == null || supportedStandardEditClass == null || findFreshTextBeforeCaret == null) {
            Console.Error.WriteLine("FAIL: required SelectionProbe safety helpers not found");
            return 1;
        }

        Check((bool)supportedStandardEditClass.Invoke(null, new object[] { "Edit" }),
              "classic Edit class must remain supported");
        Check((bool)supportedStandardEditClass.Invoke(null, new object[] { "EDIT" }),
              "classic Edit class matching must be case-insensitive");
        foreach (var unsupportedClass in new[] {
            "RichEditD2DPT", "RichEdit20W", "RICHEDIT50W", "RichEdit", "Scintilla", null
        }) {
            Check(!(bool)supportedStandardEditClass.Invoke(null, new object[] { unsupportedClass }),
                  "non-classic editor must be rejected: " + (unsupportedClass ?? "<null>"));
        }

        const string words = "one two three four";
        CheckRange(words, 1, 0, 3);
        CheckRange(words, 3, 0, 3);
        CheckRange(words, 5, 4, 7);
        CheckRange(words, 7, 4, 7);
        CheckRange(words, 10, 8, 13);
        CheckRange(words, 13, 8, 13);
        CheckRange(words, 16, 14, 18);
        CheckRange(words, 18, 14, 18);

        CheckRange("one\ttwo\u00a0three", 3, 0, 3);
        CheckRange("one\ttwo\u00a0three", 7, 4, 7);
        CheckRange("one\ttwo\u00a0three", 13, 8, 13);
        CheckRange("word, next", 4, 0, 4);
        CheckRange("word, next", 8, 6, 10);

        CheckFreshRange("ПРивет ", 7, "ПРивет", 0, 6);
        CheckFreshRange("ПРивет,", 7, "ПРивет", 0, 6);
        CheckFreshRange("ПРивет. ", 8, "ПРивет", 0, 6);
        CheckFreshRange("one ПРивет\r\n", 12, "ПРивет", 4, 10);
        Check(FreshBounds("XПРивет ", 8, "ПРивет", 4) == null,
              "fresh exact lookup matched the tail of a longer word");
        Check(FreshBounds("ПРивет abc", 10, "ПРивет", 4) == null,
              "fresh exact lookup crossed non-boundary characters");

        var repeated = words;
        for (var i = 0; i < 20; i++) {
            var bounds = Bounds(repeated, 7);
            Check(bounds != null && bounds[0] == 4 && bounds[1] == 7,
                  "repeated Insert target escaped the second word on iteration " + i);
            if (bounds == null) break;
            var source = repeated.Substring(bounds[0], bounds[1] - bounds[0]);
            var replacement = i % 2 == 0 ? source.ToUpperInvariant() : source.ToLowerInvariant();
            repeated = repeated.Substring(0, bounds[0]) + replacement + repeated.Substring(bounds[1]);
            Check(repeated.Substring(0, 4) == "one ", "repeated Insert changed left neighbor");
            Check(repeated.Substring(7) == " three four", "repeated Insert changed right neighbor");
        }

        if (failures != 0) return 1;
        Console.WriteLine("Insert safety regression passed.");
        return 0;
    }
}
