using System.Configuration;
using Newtonsoft.Json;
using REviewer.Modules.Utils;

namespace REviewer.Modules.Forms
{
    public partial class Settings : Form
    {
        private Dictionary<string, string> _re1_json;

        public Settings()
        {
            InitializeComponent();

            _re1_json = [];
            InitSavePaths();
        }

        private void InitSavePaths()
        {
            var configPath = ConfigurationManager.AppSettings["Config"];
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new ArgumentNullException(nameof(configPath));
            }

            var json = File.ReadAllText(configPath);
            _re1_json = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (_re1_json.TryGetValue("RE1", out string? value))
            {
                if (value != null)
                {
                    textBox1.Text = value;
                }
            }
        }

        private void ButtonSelectRE1Folder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string path = fbd.SelectedPath;
                    textBox1.Text = path;

                    var configPath = ConfigurationManager.AppSettings["Config"];
                    if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                    {
                        Logger.Instance.Error("Config path is null or file does not exist");
                    }

                    _re1_json["RE1"] = path;

                    var json = JsonConvert.SerializeObject(_re1_json, Formatting.Indented);
                    File.WriteAllText(configPath, json);
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}