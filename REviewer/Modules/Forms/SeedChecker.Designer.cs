namespace REviewer.Modules.Forms
{
    partial class SeedChecker
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
            button1 = new Button();
            textBoxYourSeed = new TextBox();
            textBoxSeed = new TextBox();
            button2 = new Button();
            labelCheck = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(206, 108);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 0;
            button1.Text = "Quit";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBoxYourSeed
            // 
            textBoxYourSeed.BorderStyle = BorderStyle.FixedSingle;
            textBoxYourSeed.Enabled = false;
            textBoxYourSeed.Location = new Point(21, 22);
            textBoxYourSeed.Name = "textBoxYourSeed";
            textBoxYourSeed.Size = new Size(374, 31);
            textBoxYourSeed.TabIndex = 1;
            // 
            // textBoxSeed
            // 
            textBoxSeed.BorderStyle = BorderStyle.FixedSingle;
            textBoxSeed.Location = new Point(21, 59);
            textBoxSeed.Name = "textBoxSeed";
            textBoxSeed.Size = new Size(374, 31);
            textBoxSeed.TabIndex = 2;
            textBoxSeed.TextChanged += CheckSeed;
            // 
            // button2
            // 
            button2.Location = new Point(411, 22);
            button2.Name = "button2";
            button2.Size = new Size(112, 34);
            button2.TabIndex = 3;
            button2.Text = "Copy";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // labelCheck
            // 
            labelCheck.AutoSize = true;
            labelCheck.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelCheck.Location = new Point(421, 58);
            labelCheck.Name = "labelCheck";
            labelCheck.Size = new Size(0, 32);
            labelCheck.TabIndex = 4;
            // 
            // SeedChecker
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(533, 154);
            Controls.Add(labelCheck);
            Controls.Add(button2);
            Controls.Add(textBoxSeed);
            Controls.Add(textBoxYourSeed);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "SeedChecker";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Check your seed";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBoxYourSeed;
        private TextBox textBoxSeed;
        private Button button2;
        private Label labelCheck;
    }
}