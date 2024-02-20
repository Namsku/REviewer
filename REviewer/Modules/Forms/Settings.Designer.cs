namespace REviewer.Modules.Forms
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            buttonSelectRE1Folder = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            button2 = new Button();
            folderBrowserDialog1 = new FolderBrowserDialog();
            SuspendLayout();
            // 
            // buttonSelectRE1Folder
            // 
            buttonSelectRE1Folder.Location = new Point(748, 40);
            buttonSelectRE1Folder.Name = "buttonSelectRE1Folder";
            buttonSelectRE1Folder.Size = new Size(43, 34);
            buttonSelectRE1Folder.TabIndex = 0;
            buttonSelectRE1Folder.Text = "...";
            buttonSelectRE1Folder.UseVisualStyleBackColor = true;
            buttonSelectRE1Folder.Click += buttonSelectRE1Folder_Click;
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Enabled = false;
            textBox1.Location = new Point(152, 42);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(582, 31);
            textBox1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 42);
            label1.Name = "label1";
            label1.Size = new Size(139, 25);
            label1.TabIndex = 2;
            label1.Text = "RE1 Save Folder";
            // 
            // button2
            // 
            button2.Location = new Point(679, 96);
            button2.Name = "button2";
            button2.Size = new Size(112, 34);
            button2.TabIndex = 4;
            button2.Text = "Quit";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 142);
            Controls.Add(button2);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(buttonSelectRE1Folder);
            Name = "Settings";
            Text = "Settings";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonSelectRE1Folder;
        private TextBox textBox1;
        private Label label1;
        private Button button2;
        private FolderBrowserDialog folderBrowserDialog1;
    }
}