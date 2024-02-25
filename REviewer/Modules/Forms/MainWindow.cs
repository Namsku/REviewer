using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using REviewer.Modules.RE;
using REviewer.Modules.Utils;

namespace REviewer.Modules.Forms
{
    public partial class MainWindow : Form
    {
        private Race _raceForm;

        public MainWindow()
        {
            InitializeComponent();
            this.Text = $"REviewer - {ConfigurationManager.AppSettings["Version"]}";
            comboBoxSelectGame.SelectedIndex = 0;
        }

        private void buttonQuitProgram_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void buttonRace_Click(object sender, EventArgs e) => InvokeUI(() =>
        {
            if (Application.OpenForms["Race"] == null)
            {
                _raceForm = new(_residentEvilGame, gameNames[comboBoxSelectGame.SelectedIndex]);
                _raceForm.FormClosed += Updated_RaceForm;
                buttonCheck.Enabled = false;
                _raceForm.Show();
            }

            // Close the main window
            Dispose();
        });

        private void Updated_RaceForm(object sender, FormClosedEventArgs e) => InvokeUI(() =>
        {
            buttonCheck.Enabled = true;
            _raceForm.Dispose();
        });

        private void Race_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["About"] == null)
            {
                About about = new()
                {
                    StartPosition = FormStartPosition.CenterScreen // Set the StartPosition to CenterScreen
                };
                about.FormClosed += (s, args) => about.Dispose();
                about.Show();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["Settings"] == null)
            {
                Settings settings = new()
                {
                    StartPosition = FormStartPosition.CenterScreen // Set the StartPosition to CenterScreen
                };
                settings.FormClosed += (s, args) => settings.Dispose();
                settings.Show();
            }
        }
    }
}
