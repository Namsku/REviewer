﻿using System.Configuration;
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

        private void buttonRace_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["Race"] == null)
            {
                _raceForm = new(_residentEvilGame, gameNames[comboBoxSelectGame.SelectedIndex]);
                _raceForm.FormClosed += (s, args) => _raceForm.Dispose();
                _raceForm.Show();
            }

            // Close the main window
            Dispose();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["About"] == null)
            {
                About about = new();
                about.StartPosition = FormStartPosition.CenterScreen; // Set the StartPosition to CenterScreen
                about.FormClosed += (s, args) => about.Dispose();
                about.Show();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["Settings"] == null)
            {
                Settings settings = new();
                settings.StartPosition = FormStartPosition.CenterScreen; // Set the StartPosition to CenterScreen
                settings.FormClosed += (s, args) => settings.Dispose();
                settings.Show();
            }
        }
    }
}
