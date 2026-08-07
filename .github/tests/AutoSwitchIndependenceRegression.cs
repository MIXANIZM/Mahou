using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

static class AutoSwitchIndependenceRegression {
    static int failures;

    static void Check(bool condition, string message) {
        if (condition) return;
        failures++;
        Console.Error.WriteLine("FAIL: " + message);
    }

    static object Field(object value, string name) {
        return value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .GetValue(value);
    }

    static int Main(string[] args) {
        if (args.Length != 1) {
            Console.Error.WriteLine("Usage: AutoSwitchIndependenceRegression <Mahou.exe>");
            return 2;
        }

        var bytes = File.ReadAllBytes(args[0]);
        var binaryText = Encoding.Unicode.GetString(bytes) + Encoding.UTF8.GetString(bytes);
        foreach (var forbidden in new[] {
            "snippets.txt", "SnippetsEnabled", "ExpandSnippet", "CheckSnippet",
            "__execute", "__delay", "__keyboard", "__paste", "__selection", "__setlayout"
        }) {
            Check(binaryText.IndexOf(forbidden, StringComparison.OrdinalIgnoreCase) < 0,
                  "removed snippets token remains in executable: " + forbidden);
        }

        var assembly = Assembly.LoadFrom(args[0]);
        var hook = assembly.GetType("Mahou.KMHook", true);
        var builder = hook.GetMethod("BuildAutoSwitchLiteralInputs",
            BindingFlags.Static | BindingFlags.NonPublic);
        var replacement = hook.GetMethod("PerformAutoSwitchLiteralReplacement",
            BindingFlags.Static | BindingFlags.NonPublic);
        var reload = hook.GetMethod("ReloadAutoSwitchDictionary",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Check(builder != null, "literal AutoSwitch input builder missing");
        Check(replacement != null, "dedicated AutoSwitch replacement primitive missing");
        Check(reload != null, "independent AutoSwitch dictionary reload missing");

        if (builder != null) {
            const string literal = "__delay(100)__execute(calc)|привет";
            var inputs = (Array)builder.Invoke(null, new object[] { literal });
            Check(inputs.Length == literal.Length * 2,
                  "literal replacement text was parsed, shortened, delayed, or expanded");
            for (var index = 0; index < literal.Length && inputs.Length >= (index + 1) * 2; index++) {
                var down = inputs.GetValue(index * 2);
                var up = inputs.GetValue(index * 2 + 1);
                var downKeyboard = Field(Field(down, "Data"), "Keyboard");
                var upKeyboard = Field(Field(up, "Data"), "Keyboard");
                var downScan = Convert.ToUInt16(Field(downKeyboard, "Scan"));
                var upScan = Convert.ToUInt16(Field(upKeyboard, "Scan"));
                Check(downScan == literal[index] && upScan == literal[index],
                      "dictionary value was not emitted literally at index " + index);
            }
        }

        var methodNames = hook.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(method => method.Name).ToArray();
        Check(!methodNames.Any(name => name.IndexOf("Snippet", StringComparison.OrdinalIgnoreCase) >= 0),
              "snippet execution method remains in KMHook");

        if (failures != 0) return 1;
        Console.WriteLine("AutoSwitch independence and literal replacement regression passed.");
        return 0;
    }
}
