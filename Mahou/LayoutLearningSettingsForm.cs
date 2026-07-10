using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Mahou
{
    internal sealed class LayoutLearningSettingsForm : Form
    {
        private readonly CheckBox enabled = new CheckBox();
        private readonly CheckBox autoConvert = new CheckBox();
        private readonly CheckBox perAppRules = new CheckBox();
        private readonly NumericUpDown confirmations = new NumericUpDown();
        private readonly NumericUpDown minWordLength = new NumericUpDown();
        private readonly Label rulesInfo = new Label();
        private readonly bool russian;

        public LayoutLearningSettingsForm()
        {
            russian = String.Equals(MMain.MyConfs.Read("Locales", "LANGUAGE"), "RU", StringComparison.OrdinalIgnoreCase);
            InitializeUi();
            LoadSettings();
        }

        private void InitializeUi()
        {
            Text = russian ? "Адаптивное исправление раскладки" : "Adaptive layout correction";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ClientSize = new Size(470, 330);
            AutoScaleMode = AutoScaleMode.Font;

            var title = new Label
            {
                AutoSize = false,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(16, 14),
                Size = new Size(438, 38),
                Text = russian
                    ? "Mahou запоминает только ваши ручные исправления последнего слова."
                    : "Mahou learns only your explicit last-word corrections."
            };

            enabled.AutoSize = true;
            enabled.Location = new Point(18, 59);
            enabled.Text = russian ? "Включить обучение" : "Enable learning";
            enabled.CheckedChanged += delegate { RefreshEnabledState(); };

            autoConvert.AutoSize = true;
            autoConvert.Location = new Point(18, 87);
            autoConvert.Text = russian
                ? "Автоматически исправлять на пробеле или Enter"
                : "Auto-correct on Space or Enter";

            perAppRules.AutoSize = true;
            perAppRules.Location = new Point(18, 115);
            perAppRules.Text = russian
                ? "Отдельные правила для каждой программы"
                : "Use separate rules for each application";

            var confirmationsLabel = new Label
            {
                AutoSize = true,
                Location = new Point(18, 151),
                Text = russian ? "Ручных исправлений до активации правила:" : "Manual corrections before a rule activates:"
            };
            confirmations.Location = new Point(356, 147);
            confirmations.Size = new Size(80, 22);
            confirmations.Minimum = 1;
            confirmations.Maximum = 20;

            var minLengthLabel = new Label
            {
                AutoSize = true,
                Location = new Point(18, 184),
                Text = russian ? "Минимальная длина слова:" : "Minimum word length:"
            };
            minWordLength.Location = new Point(356, 180);
            minWordLength.Size = new Size(80, 22);
            minWordLength.Minimum = 1;
            minWordLength.Maximum = 64;

            rulesInfo.AutoSize = false;
            rulesInfo.Location = new Point(18, 217);
            rulesInfo.Size = new Size(418, 22);

            var clear = new Button
            {
                Location = new Point(18, 248),
                Size = new Size(150, 28),
                Text = russian ? "Очистить память" : "Clear learned rules"
            };
            clear.Click += ClearRules_Click;

            var openFolder = new Button
            {
                Location = new Point(176, 248),
                Size = new Size(135, 28),
                Text = russian ? "Открыть папку" : "Open data folder"
            };
            openFolder.Click += OpenFolder_Click;

            var save = new Button
            {
                Location = new Point(298, 291),
                Size = new Size(75, 27),
                Text = russian ? "Сохранить" : "Save",
                DialogResult = DialogResult.None
            };
            save.Click += Save_Click;

            var cancel = new Button
            {
                Location = new Point(379, 291),
                Size = new Size(75, 27),
                Text = russian ? "Отмена" : "Cancel",
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(title);
            Controls.Add(enabled);
            Controls.Add(autoConvert);
            Controls.Add(perAppRules);
            Controls.Add(confirmationsLabel);
            Controls.Add(confirmations);
            Controls.Add(minLengthLabel);
            Controls.Add(minWordLength);
            Controls.Add(rulesInfo);
            Controls.Add(clear);
            Controls.Add(openFolder);
            Controls.Add(save);
            Controls.Add(cancel);

            AcceptButton = save;
            CancelButton = cancel;
        }

        private void LoadSettings()
        {
            enabled.Checked = ReadBool("Enabled", true);
            autoConvert.Checked = ReadBool("AutoConvertOnSpace", false);
            perAppRules.Checked = ReadBool("PerAppRules", false);
            confirmations.Value = Clamp(ReadInt("ConfirmationsToEnable", 2), confirmations.Minimum, confirmations.Maximum);
            minWordLength.Value = Clamp(ReadInt("MinWordLength", 4), minWordLength.Minimum, minWordLength.Maximum);
            RefreshEnabledState();
            RefreshRulesInfo();
        }

        private void RefreshEnabledState()
        {
            autoConvert.Enabled = enabled.Checked;
            perAppRules.Enabled = enabled.Checked;
            confirmations.Enabled = enabled.Checked;
            minWordLength.Enabled = enabled.Checked;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            MMain.MyConfs.Write("LayoutLearning", "Enabled", enabled.Checked.ToString());
            MMain.MyConfs.Write("LayoutLearning", "AutoConvertOnSpace", autoConvert.Checked.ToString());
            MMain.MyConfs.Write("LayoutLearning", "PerAppRules", perAppRules.Checked.ToString());
            MMain.MyConfs.Write("LayoutLearning", "ConfirmationsToEnable", confirmations.Value.ToString());
            MMain.MyConfs.Write("LayoutLearning", "MinWordLength", minWordLength.Value.ToString());

            AdaptiveLayoutLearning.Stop();
            AdaptiveLayoutLearning.Start();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ClearRules_Click(object sender, EventArgs e)
        {
            var answer = MessageBox.Show(
                russian ? "Удалить все выученные правила?" : "Delete all learned rules?",
                Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
                return;

            AdaptiveLayoutLearning.ClearRules();
            RefreshRulesInfo();
        }

        private void OpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Configs.dataPath);
                Process.Start("explorer.exe", Configs.dataPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshRulesInfo()
        {
            int count = 0;
            var file = Path.Combine(Configs.dataPath, "layout-learning.tsv");
            try
            {
                if (File.Exists(file))
                {
                    foreach (var line in File.ReadLines(file))
                    {
                        if (!String.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                            count++;
                    }
                }
            }
            catch
            {
            }

            rulesInfo.Text = russian
                ? "Сохранено правил: " + count + ". Менеджеры паролей исключены автоматически."
                : "Saved rules: " + count + ". Password managers are excluded automatically.";
        }

        private static bool ReadBool(string key, bool defaultValue)
        {
            bool value;
            return Boolean.TryParse(MMain.MyConfs.Read("LayoutLearning", key), out value) ? value : defaultValue;
        }

        private static int ReadInt(string key, int defaultValue)
        {
            int value;
            return Int32.TryParse(MMain.MyConfs.Read("LayoutLearning", key), out value) ? value : defaultValue;
        }

        private static decimal Clamp(int value, decimal minimum, decimal maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }
    }
}
