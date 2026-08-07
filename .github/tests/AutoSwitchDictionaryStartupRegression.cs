using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

static class AutoSwitchDictionaryStartupRegression {
    const int BundledAliasCount = 151429;
    const int TimeoutMilliseconds = 12000;
    static int failures;
    static Type hookType;
    static Type parserType;
    static Type uiType;
    static MethodInfo parse;
    static MethodInfo load;
    static MethodInfo reload;

    static void Check(bool condition, string message) {
        if (condition) return;
        failures++;
        Console.Error.WriteLine("FAIL: " + message);
    }

    static object Field(object value, string name) {
        return value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(value);
    }

    static T StaticField<T>(Type type, string name) {
        return (T)type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .GetValue(null);
    }

    static void SetStaticField(Type type, string name, object value) {
        type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SetValue(null, value);
    }

    static bool Success(object result) { return (bool)Field(result, "Success"); }
    static string[] Sources(object result) { return (string[])Field(result, "Sources"); }
    static string[] Replacements(object result) { return (string[])Field(result, "Replacements"); }
    static int AliasCount(object result) { return (int)Field(result, "AliasCount"); }
    static int RuleCount(object result) { return (int)Field(result, "RuleCount"); }
    static int CommentCount(object result) { return (int)Field(result, "CommentCount"); }

    static int InvocationCount() {
        return (int)parserType.GetProperty("InvocationCount", BindingFlags.Static | BindingFlags.NonPublic)
            .GetValue(null, null);
    }

    static object ParseWithin(string dictionary, string label) {
        var watch = Stopwatch.StartNew();
        var task = Task.Factory.StartNew(() => parse.Invoke(null, new object[] { dictionary }));
        if (Task.WaitAny(new Task[] { task }, TimeoutMilliseconds) != 0) {
            Check(false, label + " exceeded " + TimeoutMilliseconds + " ms");
            return null;
        }
        watch.Stop();
        if (task.IsFaulted) {
            Check(false, label + " threw: " + task.Exception.GetBaseException().Message);
            return null;
        }
        Console.WriteLine(label + " completed in " + watch.ElapsedMilliseconds + " ms.");
        return task.Result;
    }

    static void CheckSimpleFormats() {
        const string literal = "__delay(100)__execute(calc)|literal snippet-like text";
        var lf = "# comment\n->first|alias\n// comment between source and replacement\n====>" + literal + "<====\n" +
                 "->first\n====>second replacement<====\n";
        var parsed = parse.Invoke(null, new object[] { lf });
        Check(Success(parsed), "LF dictionary did not parse");
        Check(RuleCount(parsed) == 2 && AliasCount(parsed) == 3,
              "rule/alias counts do not preserve aliases");
        Check(CommentCount(parsed) == 2, "comment handling between rule markers changed");
        var sources = Sources(parsed);
        var replacements = Replacements(parsed);
        Check(sources.SequenceEqual(new[] { "first", "alias", "first" }),
              "alias, duplicate, or source ordering changed");
        Check(replacements[0] == literal && replacements[1] == literal,
              "literal snippet-like replacement text was interpreted");
        Check(replacements[2] == "second replacement",
              "duplicate first-match ordering changed");

        var crlf = "// comment\r\n->one|two\r\n====>line1\r\nline2<====\r\n";
        parsed = parse.Invoke(null, new object[] { crlf });
        Check(Success(parsed), "CRLF dictionary did not parse");
        Check(Sources(parsed).SequenceEqual(new[] { "one", "two" }),
              "CRLF aliases changed");
        Check(Replacements(parsed)[0] == "line1\nline2",
              "CRLF replacement did not preserve legacy LF normalization");
    }

    static void CheckFailClosed() {
        var valid = "->safe\n====>replacement<====\n";
        var validResult = load.Invoke(null, new object[] { valid });
        Check(Success(validResult), "valid setup dictionary failed");
        Check(StaticField<string[]>(hookType, "as_wrongs").Length == 1,
              "valid setup dictionary was not published");

        var malformed = valid + "->broken\n====>incomplete";
        var failed = load.Invoke(null, new object[] { malformed });
        Check(!Success(failed), "malformed trailing rule was accepted");
        Check(StaticField<string[]>(hookType, "as_wrongs").Length == 0 &&
              StaticField<string[]>(hookType, "as_corrects").Length == 0,
              "partial data was published after malformed trailing rule");
    }

    static void CheckRepeatedParsing() {
        var dictionary = "->repeat|again\n====>literal<====\n";
        var first = parse.Invoke(null, new object[] { dictionary });
        var second = parse.Invoke(null, new object[] { dictionary });
        Check(Success(first) && Success(second), "repeated parsing failed");
        Check(Sources(first).SequenceEqual(Sources(second)) &&
              Replacements(first).SequenceEqual(Replacements(second)),
              "repeated parsing produced different data");
    }

    static void CheckConfigReloadAndDisable(string dictionaryPath) {
        const string startupDictionary = "->startup\n====>ready<====\n";
        SetStaticField(uiType, "AutoSwitchEnabled", true);
        SetStaticField(uiType, "AutoSwitchDictionaryTooBig", false);
        SetStaticField(uiType, "AutoSwitchDictionaryRaw", startupDictionary);
        var before = InvocationCount();
        var result = reload.Invoke(null, null);
        var after = InvocationCount();
        Check(Success(result) && after - before == 1,
              "actual configuration reload path did not invoke the parser exactly once");

        var bytesBefore = File.ReadAllBytes(dictionaryPath);
        SetStaticField(uiType, "AS_dictfile", dictionaryPath);
        SetStaticField(uiType, "AutoSwitchEnabled", false);
        SetStaticField(hookType, "as_wrongs", new[] { "active" });
        SetStaticField(hookType, "as_corrects", new[] { "data" });
        before = InvocationCount();
        reload.Invoke(null, null);
        after = InvocationCount();
        Check(StaticField<string[]>(hookType, "as_wrongs") == null &&
              StaticField<string[]>(hookType, "as_corrects") == null,
              "disabled AutoSwitch did not clear active dictionary data");
        Check(after == before, "disabled AutoSwitch reparsed the dictionary");
        Check(File.Exists(dictionaryPath) && bytesBefore.SequenceEqual(File.ReadAllBytes(dictionaryPath)),
              "disabled AutoSwitch changed or deleted AS_dict.txt");
    }

    static string BuildLargeDictionary(int count) {
        var builder = new StringBuilder(count * 48);
        builder.Append("# synthetic multi-megabyte startup dictionary\n");
        for (var index = 0; index < count; index++) {
            builder.Append("->source").Append(index.ToString("D6")).Append('\n');
            builder.Append("====>replacement").Append(index.ToString("D6")).Append("<====\n");
        }
        return builder.ToString();
    }

    static int Main(string[] args) {
        if (args.Length != 2) {
            Console.Error.WriteLine("Usage: AutoSwitchDictionaryStartupRegression <Mahou.exe> <AS_dict.txt>");
            return 2;
        }

        var assembly = Assembly.LoadFrom(args[0]);
        hookType = assembly.GetType("Mahou.KMHook", true);
        parserType = assembly.GetType("Mahou.AutoSwitchDictionaryParser", true);
        uiType = assembly.GetType("Mahou.MahouUI", true);
        parse = parserType.GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic);
        load = hookType.GetMethod("LoadAutoSwitchDictionary", BindingFlags.Static | BindingFlags.NonPublic);
        reload = hookType.GetMethod("ReloadAutoSwitchDictionary", BindingFlags.Static | BindingFlags.NonPublic);
        Check(parse != null && load != null && reload != null,
              "required startup parser/configuration methods are missing");
        if (failures != 0) return 1;

        var bundled = File.ReadAllText(args[1], Encoding.UTF8);
        var bundledResult = ParseWithin(bundled, "bundled AS_dict.txt");
        if (bundledResult != null) {
            var sources = Sources(bundledResult);
            var replacements = Replacements(bundledResult);
            Check(Success(bundledResult), "bundled AS_dict.txt did not parse successfully");
            Check(AliasCount(bundledResult) == BundledAliasCount &&
                  RuleCount(bundledResult) == BundledAliasCount &&
                  sources.Length == BundledAliasCount && replacements.Length == BundledAliasCount,
                  "bundled source/replacement count is not exactly 151429");
            Check(CommentCount(bundledResult) == 18,
                  "bundled comment handling changed");
            Check(sources[0] == "ффквмфкл" && replacements[0] == "aardvark",
                  "first bundled rule changed");
            var middle = BundledAliasCount / 2;
            Check(sources[middle] == "ljvjdbnjcnm" && replacements[middle] == "домовитость",
                  "middle bundled rule changed");
            Check(sources[BundledAliasCount - 1] == "yjhvfkmyj" &&
                  replacements[BundledAliasCount - 1] == "нормально",
                  "final bundled rule changed");
        }

        CheckSimpleFormats();
        CheckFailClosed();
        CheckRepeatedParsing();
        CheckConfigReloadAndDisable(args[1]);

        var synthetic = BuildLargeDictionary(150000);
        Check(synthetic.Length >= 4 * 1024 * 1024,
              "synthetic dictionary is not multi-megabyte");
        var syntheticResult = ParseWithin(synthetic, "150000-rule synthetic dictionary");
        if (syntheticResult != null) {
            Check(Success(syntheticResult) && AliasCount(syntheticResult) == 150000,
                  "synthetic dictionary count or parse result changed");
        }

        if (failures != 0) return 1;
        Console.WriteLine("AutoSwitch dictionary startup regression passed for bundled and synthetic dictionaries.");
        return 0;
    }
}
