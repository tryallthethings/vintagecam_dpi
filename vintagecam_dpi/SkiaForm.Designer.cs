namespace vintagecam_dpi
{
    partial class SkiaForm
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
            buttonprint = new Button();
            panel = new Panel();
            button_zoomoutcenter = new Button();
            button_savepdf = new Button();
            SuspendLayout();
            // 
            // buttonprint
            // 
            buttonprint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonprint.Location = new Point(810, 8);
            buttonprint.Name = "buttonprint";
            buttonprint.Size = new Size(94, 29);
            buttonprint.TabIndex = 0;
            buttonprint.Text = "Print";
            buttonprint.UseVisualStyleBackColor = true;
            buttonprint.Click += buttonprint_Click;
            // 
            // panel
            // 
            panel.Location = new Point(12, 12);
            panel.Name = "panel";
            panel.Size = new Size(779, 1086);
            panel.TabIndex = 2;
            // 
            // button_zoomoutcenter
            // 
            button_zoomoutcenter.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button_zoomoutcenter.Location = new Point(810, 78);
            button_zoomoutcenter.Name = "button_zoomoutcenter";
            button_zoomoutcenter.Size = new Size(206, 29);
            button_zoomoutcenter.TabIndex = 3;
            button_zoomoutcenter.Text = "Zoom Out and Center";
            button_zoomoutcenter.UseVisualStyleBackColor = true;
            button_zoomoutcenter.Click += button_zoomoutcenter_Click;
            // 
            // button_savepdf
            // 
            button_savepdf.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button_savepdf.Location = new Point(810, 43);
            button_savepdf.Name = "button_savepdf";
            button_savepdf.Size = new Size(94, 29);
            button_savepdf.TabIndex = 5;
            button_savepdf.Text = "Save PDF";
            button_savepdf.UseVisualStyleBackColor = true;
            button_savepdf.Click += button_savepdf_Click;
            // 
            // SkiaForm
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1027, 1182);
            Controls.Add(button_savepdf);
            Controls.Add(button_zoomoutcenter);
            Controls.Add(panel);
            Controls.Add(buttonprint);
            MaximumSize = new Size(1200, 1233);
            Name = "SkiaForm";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button buttonprint;
        private Panel panel;
        private Button button_zoomoutcenter;
        private Button button_savepdf;
    }
}
