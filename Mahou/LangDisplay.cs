using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mahou
{
    public partial class LangDisplay : Form
    {
        private bool transparentBack;

        public LangDisplay()
        {
            InitializeComponent();
            SetVisInvis();
        }

        public void ChangeLD(string to)
        {
            lbLang.Text = to;
            if (transparentBack)
                Invalidate();
        }

        public void SetVisInvis()
        {
            transparentBack = ReadTransparentBack();
            lbLang.Visible = !transparentBack;
            Invalidate();
        }

        public void RefreshLang()
        {
            try
            {
                var clangname = new System.Globalization.CultureInfo((int)Locales.GetCurrentLocale());
                ChangeLD(clangname.ThreeLetterISOLanguageName.Substring(0, 1).ToUpper() + clangname.ThreeLetterISOLanguageName.Substring(1));
            }
            catch
            {
            }
        }

        public void ChangeColors(Color fore, Color back)
        {
            lbLang.ForeColor = fore;
            lbLang.BackColor = back;
            Invalidate();
        }

        public void ChangeSizes(Font fnt, int height, int width)
        {
            lbLang.Font = fnt;
            lbLang.Height = height;
            lbLang.Width = width;
            Invalidate();
        }

        public void ShowInactiveTopmost()
        {
            ShowWindow(Handle, SW_SHOWNOACTIVATE);
            SetWindowPos(Handle.ToInt32(), HWND_TOPMOST, Left, Top, Width, Height, SWP_NOACTIVATE);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.ExStyle |= 0x80;
                return parameters;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (transparentBack)
            {
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                using (var brush = new SolidBrush(lbLang.ForeColor))
                    e.Graphics.DrawString(lbLang.Text, lbLang.Font, brush, 0, 0);
            }
            base.OnPaint(e);
        }

        public void HideWnd()
        {
            ShowWindow(Handle, 0);
        }

        private static bool ReadTransparentBack()
        {
            bool value;
            return Boolean.TryParse(MMain.MyConfs.Read("TTipUI", "TransparentBack"), out value) && value;
        }

        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int command);
    }
}
