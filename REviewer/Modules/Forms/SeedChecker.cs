using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxYourSeed.Text);
        }

        private void CheckSeed(object sender, EventArgs e)
        {
            labelCheck.Text = textBoxSeed.Text == textBoxYourSeed.Text ? "valid" : "invalid";
            labelCheck.ForeColor = textBoxSeed.Text == textBoxYourSeed.Text ? Color.Green : Color.Red;
        }

    }
}
