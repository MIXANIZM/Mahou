using System;
using System.Collections.Generic;
using Mahou.InputSurfaceProbe;

namespace Mahou.InputSurfaceProbe.Tests {
    internal static class Program {
        private static int failures;

        private static int Main() {
            TestRedaction();
            TestClassificationDeterminism();
            TestUnknownRemainsUnknown();
            TestPasswordSuppression();
            TestSchemaAndCommit();
            TestSensitiveValuesAbsent();

            if (failures == 0) {
                Console.WriteLine("Input surface probe contract tests passed.");
                return 0;
            }
            Console.Error.WriteLine(failures + " contract test(s) failed.");
            return 1;
        }

        private static void TestRedaction() {
            AssertContains(Redaction.SafeIdentifier(@"C:\Users\Alice\Documents\secret.txt"), "[redacted:", "Windows user path is redacted");
            AssertContains(Redaction.SafeIdentifier("/home/alice/private.txt"), "[redacted:", "Unix user path is redacted");
            AssertContains(Redaction.SafeIdentifier("https://example.test/private?q=chat"), "[redacted:", "URL is redacted");
            AssertContains(Redaction.SafeIdentifier("alice@example.test"), "[redacted:", "email is redacted");
            AssertEqual("17-64", Redaction.NameLengthBucket("Confidential conversation"), "UIA Name is reduced to a length bucket");
        }

        private static void TestClassificationDeterminism() {
            var facts = new ProbeFacts {
                ProcessFilename = "chrome.exe",
                Architecture = "x64",
                TopLevelClass = "Chrome_WidgetWin_1",
                FocusedClass = "Chrome_RenderWidgetHostHWND",
                UiaFrameworkId = "Chrome",
                UiaClassName = "Chrome_RenderWidgetHostHWND",
                TextPatternAvailable = true,
                CaretReadable = true,
                SelectionReadable = true,
                SelectionCollapsed = true,
                TextLengthReadable = true,
                TextLengthSource = "uia-text-pattern-range-count"
            };
            var first = Classifier.Classify(facts);
            var second = Classifier.Classify(facts);
            AssertEqual("CHROMIUM_BROWSER", first.Classification, "Chrome classification");
            AssertEqual(first.Classification, second.Classification, "classification is deterministic");
            AssertEqual(Classifier.Fingerprint(facts, first), Classifier.Fingerprint(facts, second), "fingerprint is deterministic");
        }

        private static void TestUnknownRemainsUnknown() {
            var facts = new ProbeFacts {
                ProcessFilename = "customapp.exe",
                FocusedClass = "Canvas42",
                UiaFrameworkId = "Custom",
                TextPatternAvailable = true
            };
            var result = Classifier.Classify(facts);
            AssertEqual("CUSTOM_UNKNOWN", result.Classification, "unknown custom controls stay unknown");
            AssertEqual("low", result.Confidence, "unknown confidence stays low");
        }

        private static void TestPasswordSuppression() {
            var report = BaseReport();
            report.protection.protected_or_password = true;
            report.read_capabilities.caret_readable = true;
            report.read_capabilities.selection_readable = true;
            report.read_capabilities.selection_collapsed = true;
            report.read_capabilities.text_length_readable = true;
            report.read_capabilities.text_length = 47;
            report.read_capabilities.text_length_source = "test";
            report.word_object_model.available = true;
            report.word_object_model.caret_or_selection_offsets_readable = true;
            report.word_object_model.text_length_readable = true;
            report.word_object_model.text_length = 47;

            ReportPolicy.ApplyProtection(report);
            AssertFalse(report.read_capabilities.caret_readable, "password caret is suppressed");
            AssertFalse(report.read_capabilities.selection_readable, "password selection is suppressed");
            AssertFalse(report.read_capabilities.text_length_readable, "password length is suppressed");
            AssertNull(report.read_capabilities.text_length, "password length value is removed");
            AssertTrue(report.protection.password_content_suppressed, "password suppression is recorded");
        }

        private static void TestSchemaAndCommit() {
            var report = BaseReport();
            var json = JsonReportWriter.Serialize(report);
            AssertContains(json, "\"schema_version\":\"1\"", "report contains schema version");
            AssertContains(json, "\"probe_commit\":\"0123456789012345678901234567890123456789\"", "report contains probe commit");
        }

        private static void TestSensitiveValuesAbsent() {
            const string secretText = "TOP SECRET TYPED TEXT";
            const string privateTitle = "Alice - Confidential Chat";
            const string privatePath = @"C:\Users\Alice\Documents\private.txt";
            const string privateUrl = "https://chat.example.test/alice";
            var report = BaseReport();
            report.process.filename = Redaction.SafeIdentifier(privatePath);
            report.uia.automation_id = Redaction.RedactedToken(privateTitle);
            report.uia.name_present = true;
            report.uia.name_length_bucket = Redaction.NameLengthBucket(secretText);
            report.evidence.Add(Redaction.RedactedToken(privateUrl));
            var json = JsonReportWriter.Serialize(report);
            AssertNotContains(json, secretText, "typed text is absent");
            AssertNotContains(json, privateTitle, "window or UIA title content is absent");
            AssertNotContains(json, privatePath, "user path is absent");
            AssertNotContains(json, privateUrl, "URL is absent");
        }

        private static ProbeReport BaseReport() {
            return new ProbeReport {
                schema_version = ProbeConstants.SchemaVersion,
                probe_commit = "0123456789012345678901234567890123456789",
                captured_at_utc = "2026-07-25T00:00:00Z",
                status = "CAPTURED",
                classification = "CUSTOM_UNKNOWN",
                confidence = "low",
                evidence = new List<string>(),
                read_capabilities = new ReadCapabilities { actual_text_collected = false },
                write_capabilities_unverified = new List<string> { "no-write-capability-verified-by-this-probe" },
                limitations = ReportPolicy.DefaultLimitations(),
                process = new ProcessEvidence(),
                window = new WindowEvidence(),
                uia = new UiaEvidence { supported_patterns = new List<string>() },
                msaa = new MsaaEvidence(),
                ia2 = new Ia2Evidence(),
                word_object_model = new WordEvidence(),
                protection = new ProtectionEvidence { signals = new List<string>() }
            };
        }

        private static void AssertEqual(string expected, string actual, string name) {
            if (!String.Equals(expected, actual, StringComparison.Ordinal)) Fail(name + ": expected=" + expected + ", actual=" + actual);
        }

        private static void AssertContains(string value, string expected, string name) {
            if (value == null || value.IndexOf(expected, StringComparison.Ordinal) < 0) Fail(name);
        }

        private static void AssertNotContains(string value, string forbidden, string name) {
            if (value != null && value.IndexOf(forbidden, StringComparison.Ordinal) >= 0) Fail(name);
        }

        private static void AssertTrue(bool value, string name) {
            if (!value) Fail(name);
        }

        private static void AssertFalse(bool value, string name) {
            if (value) Fail(name);
        }

        private static void AssertNull(object value, string name) {
            if (value != null) Fail(name);
        }

        private static void Fail(string name) {
            failures++;
            Console.Error.WriteLine("FAIL: " + name);
        }
    }
}
