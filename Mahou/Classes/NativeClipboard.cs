using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace Mahou {
    /// <summary>
    /// Bounded native clipboard operations and a non-materializing OLE snapshot.
    /// The snapshot holds the original IDataObject instead of enumerating and
    /// re-serializing formats such as HTML, RTF, Excel, images and file drops.
    /// </summary>
    public static class NativeClipboard {
        const int OpenAttempts = 25;
        const int RetryDelayMs = 4;
        const uint GMEM_MOVEABLE = 0x0002;
        const uint GMEM_ZEROINIT = 0x0040;

        [DllImport("ole32.dll")]
        static extern int OleGetClipboard([MarshalAs(UnmanagedType.Interface)] out IDataObject dataObject);

        [DllImport("ole32.dll")]
        static extern int OleSetClipboard([MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

        static bool OpenWithRetry() {
            for (var i = 0; i < OpenAttempts; i++) {
                if (WinAPI.OpenClipboard(IntPtr.Zero)) return true;
                Thread.Sleep(RetryDelayMs);
            }
            return false;
        }

        public static bool Clear() {
            if (!OpenWithRetry()) return false;
            try { return WinAPI.EmptyClipboard(); }
            finally { WinAPI.CloseClipboard(); }
        }

        public static bool IsEmpty() {
            if (!OpenWithRetry()) return false;
            try {
                return WinAPI.EnumClipboardFormats(0) == 0;
            } finally {
                WinAPI.CloseClipboard();
            }
        }

        public static string GetText(uint format = WinAPI.CF_UNICODETEXT, bool wide = true) {
            if (!WinAPI.IsClipboardFormatAvailable(format) || !OpenWithRetry()) return null;
            try {
                var handle = WinAPI.GetClipboardData(format);
                if (handle == IntPtr.Zero) return null;
                var pointer = WinAPI.GlobalLock(handle);
                if (pointer == IntPtr.Zero) return null;
                try { return wide ? Marshal.PtrToStringUni(pointer) : Marshal.PtrToStringAnsi(pointer); }
                finally { WinAPI.GlobalUnlock(handle); }
            } finally {
                WinAPI.CloseClipboard();
            }
        }

        public static bool SetText(string text) {
            text = text ?? String.Empty;
            if (!OpenWithRetry()) return false;
            IntPtr memory = IntPtr.Zero;
            try {
                if (!WinAPI.EmptyClipboard()) return false;
                var bytes = checked((text.Length + 1) * 2);
                memory = WinAPI.GlobalAlloc(GMEM_MOVEABLE | GMEM_ZEROINIT, new UIntPtr((uint)bytes));
                if (memory == IntPtr.Zero) return false;
                var pointer = WinAPI.GlobalLock(memory);
                if (pointer == IntPtr.Zero) return false;
                try {
                    Marshal.Copy(text.ToCharArray(), 0, pointer, text.Length);
                    Marshal.WriteInt16(pointer, text.Length * 2, 0);
                } finally {
                    WinAPI.GlobalUnlock(memory);
                }
                if (WinAPI.SetClipboardData(WinAPI.CF_UNICODETEXT, memory) == IntPtr.Zero) return false;
                memory = IntPtr.Zero;
                return true;
            } finally {
                if (memory != IntPtr.Zero) WinAPI.GlobalFree(memory);
                WinAPI.CloseClipboard();
            }
        }

        public sealed class OleSnapshot : IDisposable {
            IDataObject dataObject;
            readonly bool wasEmpty;
            bool restored;

            internal OleSnapshot(IDataObject dataObject, bool wasEmpty) {
                this.dataObject = dataObject;
                this.wasEmpty = wasEmpty;
            }

            public bool Restore() {
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

            public void Dispose() {
                if (dataObject != null && Marshal.IsComObject(dataObject)) {
                    try { Marshal.ReleaseComObject(dataObject); } catch { }
                }
                dataObject = null;
            }
        }

        public static OleSnapshot CaptureOleSnapshot() {
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
    }
}
