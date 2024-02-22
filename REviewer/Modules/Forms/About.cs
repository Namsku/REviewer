﻿using System.Configuration;
using System.Diagnostics;

namespace REviewer.Modules.Forms
{
    public partial class About : Form
    {
        private static readonly string DiscordLink = "https://discord.gg/xxqtuubayy";
        private static readonly string GithubLink = "https://github.com/namsku";
        private static readonly string version = ConfigurationManager.AppSettings["Version"].ToString();    
        public About()
        {
            InitializeComponent();

            // Set the version label to the current version
            labelProgramVersion.Text = version;

            // Adjust the controls to the center of the position they are in
            CenterControl(labelProgramVersion);
            CenterControl(labelProgramTitle);
            CenterControl(buttonAboutQuit);
        }

        private void ButtonAboutQuit_Click(object sender, EventArgs e)
        {
            // Quit the form
            this.Close();
        }

        private void LinkLabelDiscord_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Mark the link as visited
            linkLabelDiscord.LinkVisited = true;

            // Navigate to the Discord link
            OpenUrl(DiscordLink);
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Mark the link as visited
            linkLabelGithub.LinkVisited = true;

            // Navigate to the GitHub link
            OpenUrl(GithubLink);
        }

        private static void OpenUrl(string url)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to open the link: {ex.Message}");
            }
        }

        private void CenterControl(Control control)
        {
            control.Left = (this.ClientSize.Width - control.Width) / 2;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Dispose();
        }

        private void About_Load(object sender, EventArgs e)
        {

        }
    }
}
