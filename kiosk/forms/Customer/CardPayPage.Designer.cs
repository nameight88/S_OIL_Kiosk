namespace s_oil.Forms.Customer
{
    partial class CardPayPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CardPayPage));
            price_amount = new TextBox();
            SuspendLayout();
            // 
            // price_amount
            // 
            price_amount.Anchor = AnchorStyles.Right;
            price_amount.BackColor = Color.White;
            price_amount.BorderStyle = BorderStyle.None;
            price_amount.Cursor = Cursors.No;
            price_amount.Font = new Font("Pretendard Variable", 72F, FontStyle.Bold, GraphicsUnit.Point, 129);
            price_amount.ForeColor = Color.FromArgb(206, 14, 14);
            price_amount.Location = new Point(362, 1433);
            price_amount.Multiline = true;
            price_amount.Name = "price_amount";
            price_amount.ReadOnly = true;
            price_amount.Size = new Size(502, 126);
            price_amount.TabIndex = 0;
            price_amount.TabStop = false;
            price_amount.TextAlign = HorizontalAlignment.Center;
            // 
            // CardPayPage
            // 
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new Size(1064, 1881);
            Controls.Add(price_amount);
            Name = "CardPayPage";
            Shown += FormProducts_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox price_amount;
    }
}