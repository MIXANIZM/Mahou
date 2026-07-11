#!/usr/bin/env python3
from pathlib import Path

root = Path(__file__).resolve().parents[2]
path = root / "Mahou" / "Classes" / "NativeClipboard.cs"
text = path.read_text(encoding="utf-8-sig")


def replace_once(old, new, label):
    global text
    count = text.count(old)
    if count != 1:
        raise SystemExit("Expected %s exactly once, found %d" % (label, count))
    text = text.replace(old, new, 1)


replace_once(
'''            public bool Restore() {
                if (restored) return true;
                try {
                    var ok = wasEmpty ? Clear() : dataObject != null && OleSetClipboard(dataObject) >= 0;
                    restored = ok;
                    return ok;
                } catch (Exception ex) {
                    Logging.Log("OLE clipboard restore failed: " + ex.Message, 2);
                    return false;
                }
            }
''',
'''            public bool Restore() {
                if (restored) return true;
                try {
                    if (wasEmpty) {
                        restored = Clear();
                        return restored;
                    }
                    if (dataObject == null) return false;
                    for (var attempt = 0; attempt < OpenAttempts; attempt++) {
                        if (OleSetClipboard(dataObject) >= 0) {
                            restored = true;
                            return true;
                        }
                        Thread.Sleep(RetryDelayMs);
                    }
                    Logging.Log("OLE clipboard restore remained unavailable after bounded retries.", 2);
                    return false;
                } catch (Exception ex) {
                    Logging.Log("OLE clipboard restore failed: " + ex.Message, 2);
                    return false;
                }
            }
''',
    "OLE restore method",
)

replace_once(
'''        public static OleSnapshot CaptureOleSnapshot() {
            try {
                IDataObject dataObject;
                var result = OleGetClipboard(out dataObject);
                if (result >= 0 && dataObject != null) return new OleSnapshot(dataObject, false);
                if (IsEmpty()) return new OleSnapshot(null, true);
            } catch (Exception ex) {
                Logging.Log("OLE clipboard snapshot failed: " + ex.Message, 2);
            }
            return null;
        }
''',
'''        public static OleSnapshot CaptureOleSnapshot() {
            Exception lastError = null;
            for (var attempt = 0; attempt < OpenAttempts; attempt++) {
                try {
                    IDataObject dataObject;
                    var result = OleGetClipboard(out dataObject);
                    if (result >= 0 && dataObject != null) return new OleSnapshot(dataObject, false);
                    if (IsEmpty()) return new OleSnapshot(null, true);
                } catch (Exception ex) {
                    lastError = ex;
                }
                Thread.Sleep(RetryDelayMs);
            }
            Logging.Log("OLE clipboard snapshot unavailable after bounded retries" +
                        (lastError == null ? "." : ": " + lastError.Message), 2);
            return null;
        }
''',
    "OLE capture method",
)

path.write_text(text, encoding="utf-8-sig", newline="\r\n")
Path(__file__).unlink()
print("Round 7 bounded OLE clipboard retries applied.")
