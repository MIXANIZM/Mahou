using System;
using System.Text;
using System.Windows.Forms;
using NLog;

namespace Mahou
{
    internal static class DpiAccessibility
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static void Apply(Form form, string accessibleName, string acceptControlName, string cancelControlName)
        {
            if (form == null)
                return;

            form.AutoScroll = true;
            if (String.IsNullOrWhiteSpace(form.AccessibleName))
                form.AccessibleName = String.IsNullOrWhiteSpace(accessibleName) ? CleanText(form.Text) : accessibleName;

            ApplyRecursive(form);
            SetDialogButtons(form, acceptControlName, cancelControlName);
            form.SystemColorsChanged -= Form_SystemColorsChanged;
            form.SystemColorsChanged += Form_SystemColorsChanged;

            try
            {
                Log.Info("UI form {0} opened at {1} DPI; high contrast={2}", form.Name, form.DeviceDpi, SystemInformation.HighContrast);
            }
            catch
            {
            }
        }

        private static void ApplyRecursive(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                var label = control as Label;
                if (label != null)
                {
                    label.TabStop = false;
                    if (!label.AutoSize)
                        label.AutoEllipsis = true;
                }

                if (String.IsNullOrWhiteSpace(control.AccessibleName))
                {
                    string candidate = CleanText(control.Text);
                    if (String.IsNullOrWhiteSpace(candidate))
                        candidate = HumanizeName(control.Name);
                    if (!String.IsNullOrWhiteSpace(candidate))
                        control.AccessibleName = candidate;
                }

                if (control.HasChildren)
                    ApplyRecursive(control);
            }
        }

        private static void SetDialogButtons(Form form, string acceptControlName, string cancelControlName)
        {
            var accept = FindControl<Button>(form, acceptControlName);
            if (accept != null)
                form.AcceptButton = accept;

            var cancel = FindControl<Button>(form, cancelControlName);
            if (cancel != null)
            {
                cancel.DialogResult = DialogResult.Cancel;
                form.CancelButton = cancel;
            }
        }

        private static T FindControl<T>(Control root, string name) where T : Control
        {
            if (root == null || String.IsNullOrWhiteSpace(name))
                return null;

            Control[] found = root.Controls.Find(name, true);
            if (found.Length == 0)
                return null;
            return found[0] as T;
        }

        private static string CleanText(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return String.Empty;
            return value.Replace("&&", "\0").Replace("&", String.Empty).Replace("\0", "&").Trim();
        }

        private static string HumanizeName(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return String.Empty;

            string source = value;
            string[] prefixes = { "btn", "cb", "tb", "lb", "lbl", "nud", "gb", "pE" };
            foreach (string prefix in prefixes)
            {
                if (source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && source.Length > prefix.Length)
                {
                    source = source.Substring(prefix.Length);
                    break;
                }
            }

            var result = new StringBuilder(source.Length + 8);
            for (int i = 0; i < source.Length; i++)
            {
                char current = source[i];
                if (i > 0 && Char.IsUpper(current) && !Char.IsWhiteSpace(source[i - 1]))
                    result.Append(' ');
                result.Append(current);
            }
            return result.ToString().Trim();
        }

        private static void Form_SystemColorsChanged(object sender, EventArgs e)
        {
            var form = sender as Form;
            if (form != null)
                form.Invalidate(true);
        }
    }
}
