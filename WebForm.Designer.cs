namespace PA.DesktopWebApp
{
    partial class WebForm
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
            this.mainWebControl = new PA.DesktopWebApp.WebControl();
            this.SuspendLayout();
            // 
            // mainWebControl
            // 
            this.mainWebControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainWebControl.Location = new System.Drawing.Point(0, 0);
            this.mainWebControl.Name = "mainWebControl";
            this.mainWebControl.Size = new System.Drawing.Size(589, 385);
            this.mainWebControl.StartPage = "index.html";
            this.mainWebControl.TabIndex = 0;
            this.mainWebControl.ScriptCall += new PA.DesktopWebApp.ScriptCallEventHandler(this.mainWebControl_ScriptCall);
            this.mainWebControl.DocumentLoaded += new System.EventHandler(this.mainWebControl_DocumentLoaded);
            // 
            // WebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 385);
            this.Controls.Add(this.mainWebControl);
            this.Name = "WebForm";
            this.ResumeLayout(false);

        }

        #endregion

        private PA.DesktopWebApp.WebControl mainWebControl;

    }
}

