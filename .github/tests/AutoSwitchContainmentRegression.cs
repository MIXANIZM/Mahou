using System;
using System.Collections.Generic;
using System.Reflection;

static class AutoSwitchContainmentRegression {
    static int failures;
    static Type contextType;
    static ConstructorInfo contextConstructor;
    static MethodInfo canMutate;
    static MethodInfo isModernNotepadSurface;
    static MethodInfo isKnownContext;

    sealed class Simulation {
        internal string Text;
        internal readonly List<string> Events = new List<string>();
    }

    static void Check(bool condition, string message) {
        if (condition) return;
        failures++;
        Console.Error.WriteLine("FAIL: " + message);
    }

    static object Context(long foreground, long focused, uint processId, string executable,
                          string controlClass, bool protectedControl = false) {
        return contextConstructor.Invoke(new object[] {
            new IntPtr(foreground), new IntPtr(focused), processId, executable, controlClass,
            protectedControl
        });
    }

    static bool CanMutate(object expected, object current) {
        return (bool)canMutate.Invoke(null, new[] { expected, current });
    }

    static Simulation Simulate(object expected, object current, string source, string replacement) {
        var result = new Simulation { Text = source };
        if (!CanMutate(expected, current)) return result;
        for (var i = 0; i < source.Length; i++) result.Events.Add("Backspace");
        result.Events.Add("Replacement:" + replacement);
        result.Text = replacement;
        return result;
    }

    static int Main(string[] args) {
        if (args.Length != 1) {
            Console.Error.WriteLine("Usage: AutoSwitchContainmentRegression <Mahou.exe>");
            return 2;
        }

        var assembly = Assembly.LoadFrom(args[0]);
        contextType = assembly.GetType("Mahou.AutoSwitchSourceContext", true);
        var safetyType = assembly.GetType("Mahou.AutoSwitchSafety", true);
        contextConstructor = contextType.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(IntPtr), typeof(IntPtr), typeof(uint), typeof(string), typeof(string), typeof(bool) },
            null);
        canMutate = safetyType.GetMethod("CanMutate", BindingFlags.Static | BindingFlags.NonPublic);
        isModernNotepadSurface = safetyType.GetMethod(
            "IsModernNotepadSurface", BindingFlags.Static | BindingFlags.NonPublic);
        isKnownContext = safetyType.GetMethod("IsKnownContext", BindingFlags.Static | BindingFlags.NonPublic);
        if (contextConstructor == null || canMutate == null ||
            isModernNotepadSurface == null || isKnownContext == null) {
            Console.Error.WriteLine("FAIL: required AutoSwitch containment helpers not found");
            return 1;
        }

        var notepad = Context(100, 101, 200, "notepad.exe", "RichEditD2DPT");
        Check((bool)isModernNotepadSurface.Invoke(null, new[] { "notepad.exe", "RichEditD2DPT" }),
              "modern Notepad surface must be classified as blocked");
        Check((bool)isModernNotepadSurface.Invoke(null, new[] { "NOTEPAD.EXE", "richeditd2dpt" }),
              "modern Notepad classification must be case-insensitive");
        Check(!CanMutate(notepad, notepad), "modern Notepad must be rejected before deletion");

        var blocked = Simulate(notepad, notepad, "ghbdtn", "привет");
        Check(blocked.Events.Count == 0, "blocked Notepad emitted Backspace/Delete or replacement events");
        Check(blocked.Text == "ghbdtn", "ghbdtn became converted or partially converted in blocked Notepad");
        Check(blocked.Text.Length == 6, "blocked Notepad lost the complete typed token");

        var changedFocus = Context(300, 301, 200, "notepad.exe", "RichEditD2DPT");
        var delayed = Simulate(notepad, changedFocus, "ghbdtn", "привет");
        Check(delayed.Events.Count == 0 && delayed.Text == "ghbdtn",
              "a delayed AutoSwitch operation survived a focus/control change");

        var changedControl = Context(100, 102, 200, "notepad.exe", "RichEditD2DPT");
        Check(!CanMutate(notepad, changedControl), "focused-control change was not rejected");

        var chrome = Context(400, 401, 500, "chrome.exe", "Chrome_RenderWidgetHostHWND");
        var word = Context(600, 601, 700, "WINWORD.exe", "_WwG");
        Check(CanMutate(chrome, chrome), "Chrome routing changed");
        Check(CanMutate(word, word), "Microsoft Word routing changed");

        var protectedEdit = Context(800, 801, 900, "sample.exe", "Edit", true);
        Check(!CanMutate(protectedEdit, protectedEdit), "protected standard Edit did not fail closed");

        var unknown = Context(0, 0, 0, null, null);
        Check(!(bool)isKnownContext.Invoke(null, new[] { unknown }), "unknown context was accepted");
        Check(!CanMutate(unknown, unknown), "unknown context did not fail closed");

        var classicNotepad = Context(1000, 1001, 1100, "notepad.exe", "Edit");
        Check(CanMutate(classicNotepad, classicNotepad),
              "containment expanded beyond the exact RichEditD2DPT surface");

        if (failures != 0) return 1;
        Console.WriteLine("AutoSwitch modern Notepad containment regression passed.");
        return 0;
    }
}
