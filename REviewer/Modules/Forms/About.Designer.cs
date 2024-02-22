namespace REviewer.Modules.Forms
{
    partial class About
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
            buttonAboutQuit = new Button();
            linkLabelDiscord = new LinkLabel();
            linkLabelGithub = new LinkLabel();
            labelAuthor = new Label();
            labelProgramTitle = new Label();
            labelProgramVersion = new Label();
            LabelImageTitle = new Label();
            labelSeam = new Label();
            labelRE = new Label();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // buttonAboutQuit
            // 
            buttonAboutQuit.Location = new Point(189, 179);
            buttonAboutQuit.Name = "buttonAboutQuit";
            buttonAboutQuit.Size = new Size(112, 34);
            buttonAboutQuit.TabIndex = 0;
            buttonAboutQuit.Text = "Quit";
            buttonAboutQuit.UseVisualStyleBackColor = true;
            buttonAboutQuit.Click += ButtonAboutQuit_Click;
            // 
            // linkLabelDiscord
            // 
            linkLabelDiscord.AutoSize = true;
            linkLabelDiscord.Location = new Point(406, 9);
            linkLabelDiscord.Name = "linkLabelDiscord";
            linkLabelDiscord.Size = new Size(73, 25);
            linkLabelDiscord.TabIndex = 3;
            linkLabelDiscord.TabStop = true;
            linkLabelDiscord.Text = "Discord";
            linkLabelDiscord.LinkClicked += LinkLabelDiscord_LinkClicked;
            // 
            // linkLabelGithub
            // 
            linkLabelGithub.AutoSize = true;
            linkLabelGithub.Location = new Point(414, 34);
            linkLabelGithub.Name = "linkLabelGithub";
            linkLabelGithub.Size = new Size(65, 25);
            linkLabelGithub.TabIndex = 4;
            linkLabelGithub.TabStop = true;
            linkLabelGithub.Text = "Github";
            linkLabelGithub.LinkClicked += LinkLabel1_LinkClicked;
            // 
            // labelAuthor
            // 
            labelAuthor.AutoSize = true;
            labelAuthor.Location = new Point(12, 9);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(149, 25);
            labelAuthor.TabIndex = 5;
            labelAuthor.Text = "Author - Namsku";
            // 
            // labelProgramTitle
            // 
            labelProgramTitle.AutoSize = true;
            labelProgramTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelProgramTitle.Location = new Point(211, 51);
            labelProgramTitle.Name = "labelProgramTitle";
            labelProgramTitle.Size = new Size(90, 25);
            labelProgramTitle.TabIndex = 6;
            labelProgramTitle.Text = "REviewer";
            // 
            // labelProgramVersion
            // 
            labelProgramVersion.AutoSize = true;
            labelProgramVersion.Location = new Point(226, 76);
            labelProgramVersion.Name = "labelProgramVersion";
            labelProgramVersion.Size = new Size(50, 25);
            labelProgramVersion.TabIndex = 7;
            labelProgramVersion.Text = "0.0.0";
            labelProgramVersion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LabelImageTitle
            // 
            LabelImageTitle.AutoSize = true;
            LabelImageTitle.Location = new Point(12, 76);
            LabelImageTitle.Name = "LabelImageTitle";
            LabelImageTitle.Size = new Size(70, 25);
            LabelImageTitle.TabIndex = 8;
            LabelImageTitle.Text = "Images";
            // 
            // labelSeam
            // 
            labelSeam.AutoSize = true;
            labelSeam.Location = new Point(12, 126);
            labelSeam.Name = "labelSeam";
            labelSeam.Size = new Size(175, 25);
            labelSeam.TabIndex = 9;
            labelSeam.Text = "Seamless Project HD";
            // 
            // labelRE
            // 
            labelRE.AutoSize = true;
            labelRE.Location = new Point(12, 101);
            labelRE.Name = "labelRE";
            labelRE.Size = new Size(114, 25);
            labelRE.TabIndex = 10;
            labelRE.Text = "Evil Resource";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(385, 101);
            label1.Name = "label1";
            label1.Size = new Size(94, 25);
            label1.TabIndex = 11;
            label1.Text = "Thank you";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(299, 126);
            label2.Name = "label2";
            label2.Size = new Size(180, 25);
            label2.TabIndex = 12;
            label2.Text = "For using my tool <3";
            // 
            // About
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(491, 225);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(labelRE);
            Controls.Add(labelSeam);
            Controls.Add(LabelImageTitle);
            Controls.Add(labelProgramVersion);
            Controls.Add(labelProgramTitle);
            Controls.Add(labelAuthor);
            Controls.Add(linkLabelGithub);
            Controls.Add(linkLabelDiscord);
            Controls.Add(buttonAboutQuit);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "About";
            Text = "About";
            Load += About_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonAboutQuit;
        private LinkLabel linkLabelDiscord;
        private LinkLabel linkLabelGithub;
        private Label labelAuthor;
        private Label labelProgramTitle;
        private Label labelProgramVersion;
        private Label LabelImageTitle;
        private Label labelSeam;
        private Label labelRE;
        private Label label1;
        private Label label2;
    }
}