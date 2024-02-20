using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace REviewer.Modules.Forms
{
    public partial class Settings : Form
    {
        private Dictionary<string, string> _re1_json;

        public Settings()
        {
            InitializeComponent();
            InitSavePaths();
        }

        private void InitSavePaths()
        {
            var configPath = ConfigurationManager.AppSettings["Config"];
            if (configPath != null && File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                _re1_json = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                if (_re1_json.ContainsKey("RE1"))
                {
                    textBox1.Text = _re1_json["RE1"];
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(configPath));
            }
        }

        private void buttonSelectRE1Folder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string path = fbd.SelectedPath;
                    textBox1.Text = path;

                    var configPath = ConfigurationManager.AppSettings["Config"];
                    if (configPath != null && File.Exists(configPath))
                    {
                        var json = File.ReadAllText(configPath);
                        _re1_json = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        _re1_json["RE1"] = path;

                        json = JsonConvert.SerializeObject(_re1_json, Formatting.Indented);
                        File.WriteAllText(configPath, json);
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(configPath));
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}