namespace s_oil
{
    partial class MainForm
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
            SuspendLayout();
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.None; // ? AutoScaleMode를 None으로 변경
            BackColor = SystemColors.Control;
            ClientSize = new Size(1080, 1920); // ? 1080x1920으로 수정
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None; // ? 테두리 완전 제거
            Margin = new Padding(0); // ? 여백 제거
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.Manual; // ? 수동 위치 설정
            Text = "S-Oil Kiosk";
            WindowState = FormWindowState.Normal; // ? 일반 창 상태
            ResumeLayout(false);
        }

        #endregion
    }
}
