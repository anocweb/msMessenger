namespace Message
{
    partial class message
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(message));
            this.picture = new System.Windows.Forms.PictureBox();
            this.messageData = new System.Windows.Forms.FlowLayoutPanel();
            this.name = new System.Windows.Forms.Label();
            this.text = new System.Windows.Forms.Label();
            this.timestamp = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picture)).BeginInit();
            this.messageData.SuspendLayout();
            this.SuspendLayout();
            // 
            // picture
            // 
            this.picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picture.Image = ((System.Drawing.Image)(resources.GetObject("picture.Image")));
            this.picture.Location = new System.Drawing.Point(3, 3);
            this.picture.Name = "picture";
            this.picture.Size = new System.Drawing.Size(50, 50);
            this.picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picture.TabIndex = 11;
            this.picture.TabStop = false;
            this.picture.Click += new System.EventHandler(this.picture_Click);
            // 
            // messageData
            // 
            this.messageData.AutoSize = true;
            this.messageData.BackColor = System.Drawing.Color.WhiteSmoke;
            this.messageData.Controls.Add(this.name);
            this.messageData.Controls.Add(this.text);
            this.messageData.Controls.Add(this.timestamp);
            this.messageData.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.messageData.Location = new System.Drawing.Point(57, 3);
            this.messageData.MaximumSize = new System.Drawing.Size(265, 1000);
            this.messageData.Name = "messageData";
            this.messageData.Padding = new System.Windows.Forms.Padding(5);
            this.messageData.Size = new System.Drawing.Size(258, 69);
            this.messageData.TabIndex = 10;
            this.messageData.Paint += new System.Windows.Forms.PaintEventHandler(this.messageData_Paint);
            // 
            // name
            // 
            this.name.AutoSize = true;
            this.name.Font = new System.Drawing.Font("Noto Sans", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.name.Location = new System.Drawing.Point(8, 5);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(146, 20);
            this.name.TabIndex = 3;
            this.name.Text = "Bacon Bacon Bacon";
            // 
            // text
            // 
            this.text.AutoSize = true;
            this.text.Font = new System.Drawing.Font("Noto Sans", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text.Location = new System.Drawing.Point(8, 25);
            this.text.Name = "text";
            this.text.Size = new System.Drawing.Size(208, 18);
            this.text.TabIndex = 4;
            this.text.Text = "at responding to text messages :/";
            // 
            // timestamp
            // 
            this.timestamp.AutoSize = true;
            this.timestamp.Font = new System.Drawing.Font("Noto Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timestamp.ForeColor = System.Drawing.Color.DimGray;
            this.timestamp.Location = new System.Drawing.Point(8, 43);
            this.timestamp.Name = "timestamp";
            this.timestamp.Size = new System.Drawing.Size(108, 15);
            this.timestamp.TabIndex = 5;
            this.timestamp.Text = "2016-06-05 22:38:16";
            // 
            // message
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.picture);
            this.Controls.Add(this.messageData);
            this.Name = "message";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Size = new System.Drawing.Size(318, 75);
            this.Load += new System.EventHandler(this.message_Load);
            this.Click += new System.EventHandler(this.message_Click);
            this.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.message_ControlAdded);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Control_DoubleClick);
            ((System.ComponentModel.ISupportInitialize)(this.picture)).EndInit();
            this.messageData.ResumeLayout(false);
            this.messageData.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picture;
        private System.Windows.Forms.FlowLayoutPanel messageData;
        private System.Windows.Forms.Label name;
        private System.Windows.Forms.Label text;
        private System.Windows.Forms.Label timestamp;
    }
}
