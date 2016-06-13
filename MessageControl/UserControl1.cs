using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Message
{
    public partial class message: UserControl
    {
        public bool alignRight = false;
        public bool showPicture = true;
        public string messageName = "Bacon";
        public string messageText = "And now, instead barded steeds.";
        public string messageTimestamp = "2016-06-05 22:38:16";




        public message()
        {
            InitializeComponent();
        }

        private void message_Load(object sender, EventArgs e)
        {
            name.Text = messageName;
            timestamp.Text = messageTimestamp;
            text.Text = messageText;

            updateDirection();
        }

        private void updateDirection()
        {
            
            if (alignRight)
            {
                picture.Left = (this.Right - 3) - picture.Width;
                messageData.Left = 13 + (messageData.Width - picture.Left);
                messageData.RightToLeft = RightToLeft.Yes;
                name.RightToLeft = RightToLeft.No;
                text.RightToLeft = RightToLeft.No;
                timestamp.RightToLeft = RightToLeft.No;
                messageData.BackColor = Color.WhiteSmoke;
                name.Text = "Me";
            } else
            {
                picture.Left = this.Left + 3;
                messageData.Left = 5 + picture.Right;
                messageData.RightToLeft = RightToLeft.No;
                name.RightToLeft = RightToLeft.No;
                text.RightToLeft = RightToLeft.No;
                timestamp.RightToLeft = RightToLeft.No;
                messageData.BackColor = Color.MistyRose;
                name.Text = messageName;
            }
        }

        private void picture_Click(object sender, EventArgs e)
        {
            alignRight = !alignRight;
            updateDirection();
        }

        private void message_Click(object sender, EventArgs e)
        {

        }

        private void Control_DoubleClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("User control clicked");
            OnDoubleClick(e);
        }

        private void message_ControlAdded(object sender, ControlEventArgs e)
        {
            //e.Control.DoubleClick += Control_DoubleClick;
            base.OnControlAdded(e);
        }

        private void messageData_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
