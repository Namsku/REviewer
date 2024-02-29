
using System.Diagnostics;
using REviewer.Modules.Utils;
using REviewer.Modules.RE;
using System.Configuration;
using System;
using REviewer.Modules.RE.Common;

namespace REviewer.Modules.Forms
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private readonly List<string> gameNames = new List<string>
        {
            "Bio",
            // Other games will be incremented here based on the position in the combobox
        };

        private RootObject _residentEvilGame;
        private Process _process;
        private MonitorVariables _MVariables;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBoxSelectGame = new GroupBox();
            buttonCheck = new Button();
            comboBoxSelectGame = new ComboBox();
            groupBoxDebug = new GroupBox();
            labelRebirthDll = new Label();
            labelGameStatus = new Label();
            labelRebirthDllTitle = new Label();
            labelGameStatusTitle = new Label();
            buttonQuitProgram = new Button();
            buttonRace = new Button();
            menuStrip1 = new MenuStrip();
            optionToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            labelSavePath = new Label();
            label2 = new Label();
            groupBoxSelectGame.SuspendLayout();
            groupBoxDebug.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxSelectGame
            // 
            groupBoxSelectGame.Controls.Add(buttonCheck);
            groupBoxSelectGame.Controls.Add(comboBoxSelectGame);
            groupBoxSelectGame.Location = new Point(12, 44);
            groupBoxSelectGame.Name = "groupBoxSelectGame";
            groupBoxSelectGame.Size = new Size(490, 76);
            groupBoxSelectGame.TabIndex = 1;
            groupBoxSelectGame.TabStop = false;
            groupBoxSelectGame.Text = "Select Game";
            // 
            // buttonCheck
            // 
            buttonCheck.Location = new Point(362, 28);
            buttonCheck.Name = "buttonCheck";
            buttonCheck.Size = new Size(112, 34);
            buttonCheck.TabIndex = 2;
            buttonCheck.Text = "Check";
            buttonCheck.UseVisualStyleBackColor = true;
            buttonCheck.Click += buttonCheck_Click;
            // 
            // comboBoxSelectGame
            // 
            comboBoxSelectGame.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSelectGame.FormattingEnabled = true;
            comboBoxSelectGame.Items.AddRange(new object[] { " Resident Evil (1996) - MediaKit" });
            comboBoxSelectGame.Location = new Point(6, 30);
            comboBoxSelectGame.Name = "comboBoxSelectGame";
            comboBoxSelectGame.Size = new Size(341, 33);
            comboBoxSelectGame.TabIndex = 0;
            // 
            // groupBoxDebug
            // 
            groupBoxDebug.Controls.Add(labelSavePath);
            groupBoxDebug.Controls.Add(label2);
            groupBoxDebug.Controls.Add(labelRebirthDll);
            groupBoxDebug.Controls.Add(labelGameStatus);
            groupBoxDebug.Controls.Add(labelRebirthDllTitle);
            groupBoxDebug.Controls.Add(labelGameStatusTitle);
            groupBoxDebug.Location = new Point(18, 126);
            groupBoxDebug.Name = "groupBoxDebug";
            groupBoxDebug.Size = new Size(484, 118);
            groupBoxDebug.TabIndex = 2;
            groupBoxDebug.TabStop = false;
            groupBoxDebug.Text = "Debug";
            // 
            // labelRebirthDll
            // 
            labelRebirthDll.AutoSize = true;
            labelRebirthDll.Location = new Point(96, 52);
            labelRebirthDll.Name = "labelRebirthDll";
            labelRebirthDll.Size = new Size(19, 25);
            labelRebirthDll.TabIndex = 3;
            labelRebirthDll.Text = "-";
            // 
            // labelGameStatus
            // 
            labelGameStatus.AutoSize = true;
            labelGameStatus.Location = new Point(96, 27);
            labelGameStatus.Name = "labelGameStatus";
            labelGameStatus.Size = new Size(19, 25);
            labelGameStatus.TabIndex = 2;
            labelGameStatus.Text = "-";
            // 
            // labelRebirthDllTitle
            // 
            labelRebirthDllTitle.AutoSize = true;
            labelRebirthDllTitle.Location = new Point(6, 52);
            labelRebirthDllTitle.Name = "labelRebirthDllTitle";
            labelRebirthDllTitle.Size = new Size(68, 25);
            labelRebirthDllTitle.TabIndex = 1;
            labelRebirthDllTitle.Text = "Rebirth";
            // 
            // labelGameStatusTitle
            // 
            labelGameStatusTitle.AutoSize = true;
            labelGameStatusTitle.Location = new Point(6, 27);
            labelGameStatusTitle.Name = "labelGameStatusTitle";
            labelGameStatusTitle.Size = new Size(58, 25);
            labelGameStatusTitle.TabIndex = 0;
            labelGameStatusTitle.Text = "Game";
            // 
            // buttonQuitProgram
            // 
            buttonQuitProgram.Location = new Point(390, 284);
            buttonQuitProgram.Name = "buttonQuitProgram";
            buttonQuitProgram.Size = new Size(112, 34);
            buttonQuitProgram.TabIndex = 3;
            buttonQuitProgram.Text = "Quit";
            buttonQuitProgram.UseVisualStyleBackColor = true;
            buttonQuitProgram.Click += buttonQuitProgram_Click;
            // 
            // buttonRace
            // 
            buttonRace.Enabled = false;
            buttonRace.Location = new Point(12, 284);
            buttonRace.Name = "buttonRace";
            buttonRace.Size = new Size(112, 34);
            buttonRace.TabIndex = 5;
            buttonRace.Text = "Race";
            buttonRace.UseVisualStyleBackColor = true;
            buttonRace.Click += buttonRace_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(32, 32);
            menuStrip1.Items.AddRange(new ToolStripItem[] { optionToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(5, 2, 0, 2);
            menuStrip1.Size = new Size(512, 33);
            menuStrip1.TabIndex = 6;
            menuStrip1.Text = "menuStrip1";
            // 
            // optionToolStripMenuItem
            // 
            optionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { settingsToolStripMenuItem });
            optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            optionToolStripMenuItem.Size = new Size(58, 29);
            optionToolStripMenuItem.Text = "Edit";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(178, 34);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(65, 29);
            helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(164, 34);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // labelSavePath
            // 
            labelSavePath.AutoSize = true;
            labelSavePath.Location = new Point(96, 77);
            labelSavePath.Name = "labelSavePath";
            labelSavePath.Size = new Size(19, 25);
            labelSavePath.TabIndex = 5;
            labelSavePath.Text = "-";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 77);
            label2.Name = "label2";
            label2.Size = new Size(88, 25);
            label2.TabIndex = 4;
            label2.Text = "Save Path";
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(512, 330);
            Controls.Add(buttonRace);
            Controls.Add(buttonQuitProgram);
            Controls.Add(groupBoxDebug);
            Controls.Add(groupBoxSelectGame);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MainMenuStrip = menuStrip1;
            Name = "MainWindow";
            Text = "REviewer";
            FormClosed += MainWindow_FormClosed;
            groupBoxSelectGame.ResumeLayout(false);
            groupBoxDebug.ResumeLayout(false);
            groupBoxDebug.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            try
            {
                CleanupObjects();

                int index = comboBoxSelectGame.SelectedIndex;

                if (index == -1)
                {
                    ShowError("Please select a game", labelGameStatus);
                    return;
                }

                string processName = gameNames[index];
                _process = Library.GetProcessByName(processName);

                if (_process == null)
                {
                    ShowError("The process was not found", labelGameStatus);
                    return;
                }

                _process.EnableRaisingEvents = true;
                _process.Exited += ProcessExited;

                if (!Library.IsDdrawLoaded(_process))
                {
                    ShowError("Rebirth DLL was not found", labelRebirthDll);
                    return;
                }

                if (!Library.IsSavePathPresent(index))
                {
                    ShowError("Save path was not found", labelSavePath);
                    return;
                }

                UpdateLabel(labelGameStatus, "Found", Color.Green);
                UpdateLabel(labelRebirthDll, "Found", Color.Green);
                UpdateLabel(labelSavePath, "Found", Color.Green);

                GameData gameData = new GameData(_process.ProcessName);
                _residentEvilGame = gameData.GenerateGameData();
                _MVariables = new MonitorVariables(_process.Handle, _process.ProcessName);
                _MVariables.Start(_residentEvilGame);

                if (Application.OpenForms["Race"] == null)
                {
                    _raceForm = new(_residentEvilGame, gameNames[comboBoxSelectGame.SelectedIndex]);
                    _raceForm.SetMonitorVariables(_MVariables);
                    _raceForm.FormClosed += Updated_RaceForm;
                    buttonCheck.Enabled = false;
                    _raceForm.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here
                ShowError("An error occurred: " + ex.Message + "\n" + "Details: " + ex.StackTrace);
            }
        }

        private void CleanupObjects()
        {
            Logger.Instance.Info("Cleaning up objects (_process and _MVariables)");
            if (_process != null)
            {
                _process.Exited -= ProcessExited;
                _process.Dispose();
                _process = null;
            }

            if (_MVariables != null)
            {
                _MVariables.Stop();
                _MVariables = null;
            }
        }

        private void ShowError(string message, Label label = null)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (label != null)
            {
                UpdateLabel(label, "Not found", Color.Red);
            }
        }

        private void UpdateLabel(Label label, string text, Color color)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => UpdateLabel(label, text, color)));
            }
            else
            {
                label.Text = text;
                label.ForeColor = color;

                // if font not bold , make it bold
                if (label.Font.Style != FontStyle.Bold)
                {
                    label.Font = new Font(label.Font, FontStyle.Bold);
                }
            }
        }

        private System.Threading.Timer _searchProcessTimer;
        private bool _isProcessFound;

        private void ProcessExited(object sender, EventArgs e)
        {
            UpdateLabel(labelGameStatus, "Offline", Color.Red);
            UpdateLabel(labelRebirthDll, "Offline", Color.Red);
            UpdateButton(buttonRace, false);

            _MVariables.Stop();
            
            _process.Exited -= ProcessExited;
            _process.Dispose();
            _process = null;

            Logger.Instance.Info("Process exited - Trying to find a new process");

            // Start the timer to search for the process
            _searchProcessTimer = new System.Threading.Timer(CheckProcessStatus, null, 0, 100);
        }

        private void InvokeUI(Action action)
        {
            if (!IsDisposed)
            {
                if (InvokeRequired)
                {
                        try
                        {
                            Invoke(action);
                        }
                        catch (ObjectDisposedException)
                        {
                            // Handle the ObjectDisposedException gracefully
                            Logger.Instance.Error("ObjectDisposedException in InvokeUI");
                        }
                }
                else
                {
                    action();
                }
            }
        }

        private void CheckProcessStatus(object state) => InvokeUI(() =>
        {
            if (_isProcessFound)
            {
                // Process is already found, stop the timer
                _searchProcessTimer.Dispose();
                return;
            }

            int index = comboBoxSelectGame.SelectedIndex;
            string processName = gameNames[index];
            _process = Library.GetProcessByName(processName);

            if (_process != null)
            {
                _process.Exited += ProcessExited;
                Logger.Instance.Info("Process found!");
                
                // Process is found, stop the timer
                _searchProcessTimer.Dispose();
                _isProcessFound = true;

                if (_MVariables == null)
                {
                    _MVariables = new MonitorVariables(_process.Handle, _process.ProcessName);
                }

                // Perform any necessary actions when the process is found
                // TODO
                _MVariables.UpdateProcessHandle(_process.Handle);

                // Update labels and buttons
                UpdateLabel(labelGameStatus, "Online", Color.Green);
                UpdateLabel(labelRebirthDll, "Online", Color.Green);
                UpdateButton(buttonRace, true);
            }
        });

        private void UpdateButton(Button button, bool enabled)
        {
            button.Invoke((Action)(() => button.Enabled = enabled));
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["About"] == null)
            {
                About about = new About();
                about.Location = new Point(this.Location.X + 12, this.Location.Y + 12);
                about.FormClosed += (s, args) => about.Dispose();
                about.Show();
            }
        }

#endregion
        private GroupBox groupBoxSelectGame;
        private Button buttonCheck;
        private ComboBox comboBoxSelectGame;
        private GroupBox groupBoxDebug;
        private Label labelRebirthDll;
        private Label labelGameStatus;
        private Label labelRebirthDllTitle;
        private Label labelGameStatusTitle;
        private Button buttonQuitProgram;
        private Button buttonRace;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem optionToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private Label labelSavePath;
        private Label label2;
    }
}