using System;
using System.Drawing;
using System.Windows.Forms;

namespace REviewer.Modules.Forms
{
    public partial class SeedChecker : Form
    {
        public SeedChecker(string seed)
        {
            InitializeComponent();
            textBoxYourSeed.Text = seed;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxYourSeed.Text);
        }

        private void CheckSeed(object sender, EventArgs e)
        {
            bool isValid = textBoxSeed.Text == textBoxYourSeed.Text;
            labelCheck.Text = isValid ? "valid" : "invalid";
            labelCheck.ForeColor = isValid ? Color.Green : Color.Red;
        }
    }
}
