using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mahou
{
    public partial class MoreConfigs
    {
        private Button layoutLearningButton;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            EnsureLayoutLearningButton();
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
