namespace s_oil.Forms
{
    partial class HomePage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HomePage));
            ProductBuy = new s_oil.Utils.ImageButton();
            productstatus = new s_oil.Utils.ImageButton();
            admin_button = new s_oil.Utils.ImageButton();
            SuspendLayout();
            // 
            // ProductBuy
            // 
            ProductBuy.BackColor = Color.Transparent;
            ProductBuy.BackgroundImage = (Image)resources.GetObject("ProductBuy.BackgroundImage");
            ProductBuy.BackgroundImageLayout = ImageLayout.Stretch;
            ProductBuy.FlatAppearance.BorderSize = 0;
            ProductBuy.FlatAppearance.MouseDownBackColor = Color.Transparent;
            ProductBuy.FlatAppearance.MouseOverBackColor = Color.Transparent;
            ProductBuy.FlatStyle = FlatStyle.Flat;
            ProductBuy.Location = new Point(130, 908);
            ProductBuy.Margin = new Padding(2);
            ProductBuy.MouseDownBackColor = Color.Transparent;
            ProductBuy.MouseOverBackColor = Color.Transparent;
            ProductBuy.Name = "ProductBuy";
            ProductBuy.Size = new Size(850, 284);
            ProductBuy.TabIndex = 0;
            ProductBuy.TabStop = false;
            ProductBuy.UseVisualStyleBackColor = false;
            ProductBuy.Click += button1_Click;
            // 
            // productstatus
            // 
            productstatus.BackColor = Color.Transparent;
            productstatus.BackgroundImage = (Image)resources.GetObject("productstatus.BackgroundImage");
            productstatus.BackgroundImageLayout = ImageLayout.Stretch;
            productstatus.FlatAppearance.BorderSize = 0;
            productstatus.FlatAppearance.MouseDownBackColor = Color.Transparent;
            productstatus.FlatAppearance.MouseOverBackColor = Color.Transparent;
            productstatus.FlatStyle = FlatStyle.Flat;
            productstatus.Location = new Point(323, 1605);
            productstatus.Margin = new Padding(2);
            productstatus.MouseDownBackColor = Color.Transparent;
            productstatus.MouseOverBackColor = Color.Transparent;
            productstatus.Name = "productstatus";
            productstatus.Size = new Size(456, 157);
            productstatus.TabIndex = 1;
            productstatus.TabStop = false;
            productstatus.UseVisualStyleBackColor = false;
            productstatus.Click += productstatus_Click;
            // 
            // admin_button
            // 
            admin_button.Anchor = AnchorStyles.None;
            admin_button.BackColor = Color.Transparent;
            admin_button.BackgroundImage = (Image)resources.GetObject("admin_button.BackgroundImage");
            admin_button.BackgroundImageLayout = ImageLayout.Stretch;
            admin_button.FlatAppearance.BorderSize = 0;
            admin_button.FlatAppearance.MouseDownBackColor = Color.Transparent;
            admin_button.FlatAppearance.MouseOverBackColor = Color.Transparent;
            admin_button.FlatStyle = FlatStyle.Flat;
            admin_button.Location = new Point(674, 31);
            admin_button.Margin = new Padding(2);
            admin_button.MouseDownBackColor = Color.Transparent;
            admin_button.MouseOverBackColor = Color.Transparent;
            admin_button.Name = "admin_button";
            admin_button.Size = new Size(372, 133);
            admin_button.TabIndex = 2;
            admin_button.TabStop = false;
            admin_button.UseVisualStyleBackColor = false;
            admin_button.Click += admin_button_Click;
            // 
            // HomePage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(1080, 1920);
            Controls.Add(admin_button);
            Controls.Add(productstatus);
            Controls.Add(ProductBuy);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(2);
            Name = "HomePage";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HomePage";
            Load += HomePage_Load;
            ResumeLayout(false);
        }

        #endregion

        private Utils.ImageButton ProductBuy;
        private Utils.ImageButton productstatus;
        private Utils.ImageButton admin_button;
    }
}