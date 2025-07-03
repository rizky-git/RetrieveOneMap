namespace RetrieveOneMapForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            rtbToken = new RichTextBox();
            tbEmail = new TextBox();
            tbPassword = new TextBox();
            tbStart = new TextBox();
            tbTo = new TextBox();
            label5 = new Label();
            btnRun = new Button();
            lbStatus = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(28, 31);
            label1.Name = "label1";
            label1.Size = new Size(48, 20);
            label1.TabIndex = 0;
            label1.Text = "Token";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(28, 174);
            label2.Name = "label2";
            label2.Size = new Size(46, 20);
            label2.TabIndex = 1;
            label2.Text = "Email";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(28, 219);
            label3.Name = "label3";
            label3.Size = new Size(70, 20);
            label3.TabIndex = 2;
            label3.Text = "Password";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(28, 265);
            label4.Name = "label4";
            label4.Size = new Size(125, 20);
            label4.TabIndex = 3;
            label4.Text = "Postal Code From";
            // 
            // rtbToken
            // 
            rtbToken.Location = new Point(165, 31);
            rtbToken.Name = "rtbToken";
            rtbToken.Size = new Size(363, 120);
            rtbToken.TabIndex = 4;
            rtbToken.Text = "";
            // 
            // tbEmail
            // 
            tbEmail.Location = new Point(165, 167);
            tbEmail.Name = "tbEmail";
            tbEmail.Size = new Size(363, 27);
            tbEmail.TabIndex = 5;
            // 
            // tbPassword
            // 
            tbPassword.Location = new Point(165, 212);
            tbPassword.Name = "tbPassword";
            tbPassword.Size = new Size(363, 27);
            tbPassword.TabIndex = 6;
            tbPassword.UseSystemPasswordChar = true;
            // 
            // tbStart
            // 
            tbStart.Location = new Point(165, 258);
            tbStart.Name = "tbStart";
            tbStart.Size = new Size(150, 27);
            tbStart.TabIndex = 7;
            tbStart.Text = "0";
            // 
            // tbTo
            // 
            tbTo.Location = new Point(378, 258);
            tbTo.Name = "tbTo";
            tbTo.Size = new Size(150, 27);
            tbTo.TabIndex = 8;
            tbTo.Text = "999999";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(333, 261);
            label5.Name = "label5";
            label5.Size = new Size(25, 20);
            label5.TabIndex = 9;
            label5.Text = "To";
            // 
            // btnRun
            // 
            btnRun.Location = new Point(28, 309);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(500, 66);
            btnRun.TabIndex = 10;
            btnRun.Text = "EXTRACT";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += button1_Click;
            // 
            // lbStatus
            // 
            lbStatus.Location = new Point(28, 392);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new Size(500, 40);
            lbStatus.TabIndex = 11;
            lbStatus.Text = "Status";
            lbStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(559, 450);
            Controls.Add(lbStatus);
            Controls.Add(btnRun);
            Controls.Add(label5);
            Controls.Add(tbTo);
            Controls.Add(tbStart);
            Controls.Add(tbPassword);
            Controls.Add(tbEmail);
            Controls.Add(rtbToken);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "RetrieveOneMapForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private RichTextBox rtbToken;
        private TextBox tbEmail;
        private TextBox tbPassword;
        private TextBox tbStart;
        private TextBox tbTo;
        private Label label5;
        private Button btnRun;
        private Label lbStatus;
    }
}
