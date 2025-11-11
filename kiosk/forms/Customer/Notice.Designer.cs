namespace s_oil.Forms.Customer
{
    partial class Notice
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Notice));
            confirm_button = new Button();
            notice_content = new TextBox();
            SuspendLayout();
            // 
            // confirm_button
            // 
            confirm_button.BackColor = Color.Transparent;
            confirm_button.BackgroundImage = (Image)resources.GetObject("confirm_button.BackgroundImage");
            confirm_button.FlatAppearance.BorderSize = 0;
            confirm_button.FlatStyle = FlatStyle.Flat;
            confirm_button.Location = new Point(282, 566);
            confirm_button.Name = "confirm_button";
            confirm_button.Size = new Size(264, 94);
            confirm_button.TabIndex = 0;
            confirm_button.UseVisualStyleBackColor = false;
            // 
            // notice_content
            // 
            notice_content.BackColor = Color.White;
            notice_content.BorderStyle = BorderStyle.None;
            notice_content.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold);
            notice_content.ForeColor = Color.FromArgb(60, 60, 60);
            notice_content.Location = new Point(90, 280);
            notice_content.Multiline = true;
            notice_content.Name = "notice_content";
            notice_content.ReadOnly = true;
            notice_content.Size = new Size(690, 280);
            notice_content.TabIndex = 1;
            notice_content.TextAlign = HorizontalAlignment.Center;
            // 
            // Notice
            // 
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 255, 248);
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new Size(867, 717);
            Controls.Add(notice_content);
            Controls.Add(confirm_button);
            Font = new Font("Pretendard Variable", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Notice";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Notice";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button confirm_button;
        private TextBox notice_content;
    }
}