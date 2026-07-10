using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mahou
{
    public partial class MoreConfigs
    {
        private Button layoutLearningButton;
        private bool safetyEventsWired;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DpiAccessibility.Apply(this, "MIXANIZM Mahou advanced settings", "btnOK", "btnNO");
            EnsureLayoutLearningButton();
            WireSafetyEvents();
            ApplyDisabledUpdatePolicy();
        }

        private void WireSafetyEvents()
        {
            if (safetyEventsWired)
                return;

            safetyEventsWired = true;
            VisibleChanged += RestoreSettingsWhenShown;
            btnNO.Click += RestoreSettingsAfterCancel;
        }

        private void RestoreSettingsWhenShown(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            load();
            tmpRestore();
            ApplyDisabledUpdatePolicy();
            DisEna();
        }

        private void RestoreSettingsAfterCancel(object sender, EventArgs e)
        {
            load();
            tmpRestore();
            ApplyDisabledUpdatePolicy();
            DisEna();
        }

        private void ApplyDisabledUpdatePolicy()
        {
            cbCheckForUPD.Checked = false;
            cbCheckForUPD.Enabled = false;
            cbCheckForUPD.AccessibleName = "Automatic update checks";
            cbCheckForUPD.AccessibleDescription = "Automatic updates are disabled until signed packages and rollback are implemented.";
        }

        private void EnsureLayoutLearningButton()
        {
            if (layoutLearningButton != null)
                return;

            bool russian = String.Equals(MMain.MyConfs.Read("Locales", "LANGUAGE"), "RU", StringComparison.OrdinalIgnoreCase);
            layoutLearningButton = new Button
            {
                Name = "btnLayoutLearning",
                Location = new Point(10, 122),
                Size = new Size(258, 32),
                Text = russian ? "Адаптивное исправление раскладки…" : "Adaptive layout correction…",
                AccessibleName = russian ? "Настройки адаптивного исправления раскладки" : "Adaptive layout correction settings",
                UseVisualStyleBackColor = true,
                TabIndex = 10
            };
            layoutLearningButton.Click += LayoutLearningButton_Click;
            pEExtra.Controls.Add(layoutLearningButton);
            layoutLearningButton.BringToFront();
        }

        private void LayoutLearningButton_Click(object sender, EventArgs e)
        {
            using (var form = new LayoutLearningSettingsForm())
                form.ShowDialog(this);
        }
    }
}
