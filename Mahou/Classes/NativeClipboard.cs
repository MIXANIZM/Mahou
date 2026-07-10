using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mahou
{
    public static class NativeClipboard
    {
        #region DLL Imports/Constants
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetClipboardData(uint uFormat);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EmptyClipboard();
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool CloseClipboard();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsClipboardFormatAvailable(uint format);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GlobalFree(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UIntPtr GlobalSize(IntPtr hMem);
        [DllImport("kernel32.dll")]
        static extern uint EnumClipboardFormats(uint format);
        public const uint GMEM_DDESHARE = 0x2000;
        public const uint GMEM_MOVEABLE = 0x2;
        private const int ClipboardOpenAttempts = 50;
        private const int ClipboardOpenDelayMs = 10;
        public enum uFormat
        {
            CF_TEXT = 1,
            CF_BITMAP = 2,
            CF_SYLK = 4,
            CF_DIF = 5,
            CF_TIFF = 6,
            CF_OEMTEXT = 7,
            CF_DIB = 8,
            CF_PALETTE = 9,
            CF_PENDATA = 10,
            CF_RIFF = 11,
            CF_WAVE = 12,
            CF_UNICODETEXT = 13
        }
        #endregion

        public static void Clear() // Clears Clipboard
        {
            if (!TryOpenClipboard())
                return;

            try
            {
                EmptyClipboard();
            }
            finally
            {
                CloseClipboard();
            }
        }

        public static string GetText() // Gets text data from clipboard
        {
            if (!IsClipboardFormatAvailable((uint)uFormat.CF_UNICODETEXT))
                return null;

            if (!TryOpenClipboard())
                return null;

            IntPtr hGlobal = IntPtr.Zero;
            IntPtr locked = IntPtr.Zero;
            try
            {
                hGlobal = GetClipboardData((uint)uFormat.CF_UNICODETEXT);
                if (hGlobal == IntPtr.Zero)
                    return null;

                locked = GlobalLock(hGlobal);
                if (locked == IntPtr.Zero)
                    return null;

                return Marshal.PtrToStringUni(locked);
            }
            finally
            {
                if (locked != IntPtr.Zero && hGlobal != IntPtr.Zero)
                    GlobalUnlock(hGlobal);
                CloseClipboard();
            }
        }

        public static ClipboardData GetClipboardDatas() // Gets supported clipboard data.
        {
            var cd = new ClipboardData()
            {
                data = new List<byte[]>(),
                format = new List<uint>()
            };

            if (!TryOpenClipboard())
                return cd;

            try
            {
                foreach (var fmt in (uint[])Enum.GetValues(typeof(uFormat)))
                {
                    IntPtr pos = GetClipboardData(fmt);
                    if (pos == IntPtr.Zero)
                        continue;

                    UIntPtr length = GlobalSize(pos);
                    var byteCount = length.ToUInt32();
                    if (byteCount == 0 || byteCount > Int32.MaxValue)
                        continue;

                    IntPtr gLock = GlobalLock(pos);
                    if (gLock == IntPtr.Zero)
                        continue;

                    try
                    {
                        byte[] data = new byte[(int)byteCount];
                        Marshal.Copy(gLock, data, 0, data.Length);
                        cd.data.Add(data);
                        cd.format.Add(fmt);
                    }
                    finally
                    {
                        GlobalUnlock(pos);
                    }
                }
            }
            finally
            {
                CloseClipboard();
            }

            return cd;
        }

        public static void RestoreData(ClipboardData datas) // Places supported data back to clipboard.
        {
            if (datas.data == null || datas.format == null || datas.data.Count != datas.format.Count)
                return;

            if (!TryOpenClipboard())
                return;

            try
            {
                if (!EmptyClipboard())
                    return;

                for (int i = 0; i != datas.data.Count; i++)
                {
                    var data = datas.data[i];
                    if (data == null || data.Length == 0)
                        continue;

                    IntPtr alloc = GlobalAlloc(GMEM_MOVEABLE | GMEM_DDESHARE, new UIntPtr(Convert.ToUInt32(data.Length)));
                    if (alloc == IntPtr.Zero)
                        continue;

                    bool ownershipTransferred = false;
                    try
                    {
                        var glock = GlobalLock(alloc);
                        if (glock == IntPtr.Zero)
                            continue;

                        try
                        {
                            Marshal.Copy(data, 0, glock, data.Length);
                        }
                        finally
                        {
                            GlobalUnlock(alloc);
                        }

                        var fmt = datas.format[i];
                        ownershipTransferred = SetClipboardData(fmt, alloc) != IntPtr.Zero;
                    }
                    finally
                    {
                        if (!ownershipTransferred)
                            GlobalFree(alloc);
                    }
                }
            }
            finally
            {
                CloseClipboard();
            }
        }

        private static bool TryOpenClipboard()
        {
            for (int attempt = 0; attempt < ClipboardOpenAttempts; attempt++)
            {
                if (OpenClipboard(IntPtr.Zero))
                    return true;

                Thread.Sleep(ClipboardOpenDelayMs);
            }
            return false;
        }

        public struct ClipboardData // Struct of List of byte[](data) and uint(data format)
        {
            public List<byte[]> data;
            public List<uint> format;
        }
    }
}
