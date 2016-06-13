using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace msMessenger
{
    public partial class Setup : Form
    {
        bool setupComplete = false;
        public Setup()
        {
            InitializeComponent();
        }

        private void TestConnection_Click(object sender, EventArgs e)
        {
            TestConnection.Text = "Testing...";
            username_text.Enabled = false;
            password_text.Enabled = false;
            TestConnection.Enabled = false;
            ms.voip.VoIPms_Service soap = new ms.voip.VoIPms_Service();
            ms.voip.getDIDsInfoInput input = new ms.voip.getDIDsInfoInput();
            XmlNode[] output;
            XmlElement result_status = null;
            string status = null;
            input.api_username = username_text.Text;
            input.api_password = password_text.Text;
            input.client = ""; // Set to nothing so we can get all the account DID's
            input.did = ""; // Same thing. need all the DID's
            output = (XmlNode[])soap.getDIDsInfo(input);
            result_status = (XmlElement)output.GetValue(1);
            status = result_status.ChildNodes[1].InnerText;
            TestConnection.Text = status;
            if (status == "success")
            {
                XmlElement result_dids = null;
                XmlElement dids = null;
                XmlElement did = null;
                result_dids = (XmlElement)output.GetValue(2);
                dids = (XmlElement)result_dids.ChildNodes[1];
                for (int i = 0; i <= dids.ChildNodes.Count - 1; i++)
                {
                    did = (XmlElement)dids.ChildNodes[i];
                    if (did.ChildNodes[21].ChildNodes[1].InnerText == "1")
                    {
                        comboBox1.Items.Add(did.ChildNodes[0].ChildNodes[1].InnerText);
                    }
                    //callbackUrl_text.Text = did.ChildNodes[26].ChildNodes[1].InnerText;
                }
                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;
                    comboBox1.Enabled = true;
                    dateTimePicker1.Enabled = true;
                    saveOpen_button.Enabled = true;
                }
            }
            else
            {
                comboBox1.Items.Clear();
                comboBox1.Enabled = false;
                dateTimePicker1.Enabled = false;
                saveOpen_button.Enabled = false;
            }
            username_text.Enabled = true;
            password_text.Enabled = true;
            TestConnection.Enabled = true;
        }

        private void saveOpen_button_Click(object sender, EventArgs e)
        {
            byte[] userKey = Encoding.UTF8.GetBytes(label7.Text);
            byte[] machineKey = Encryption.AESThenHMAC.NewKey();
            byte[] bytesPass = Encoding.UTF8.GetBytes(password_text.Text);
            byte[] byteUser = Encoding.UTF8.GetBytes(username_text.Text);
            byte[] encPass = Encryption.AESThenHMAC.SimpleEncrypt(bytesPass,machineKey,userKey);

            Properties.Settings.Default.userKey = Convert.ToBase64String(userKey);
            Properties.Settings.Default.machineKey = Convert.ToBase64String(machineKey);
            Properties.Settings.Default.Username = username_text.Text;
            Properties.Settings.Default.Password = Convert.ToBase64String(encPass);
            Properties.Settings.Default.ActiveDID = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            Properties.Settings.Default.LastServerUpdate = dateTimePicker1.Value.ToString("yyyy-MM-dd") + " 00:00:00";
            Properties.Settings.Default.AvailableDIDs = comboBox1.Items[0].ToString();
            if (comboBox1.Items.Count > 1)
            {
                for (int i = 1; i <= comboBox1.Items.Count - 1; i++)
                {
                    Properties.Settings.Default.AvailableDIDs += "|" + comboBox1.Items[i].ToString();
                }
            }


            Properties.Settings.Default.Save();
            setupComplete = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Setup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (setupComplete == false)
            {
                this.DialogResult = DialogResult.Abort;
            }
        }

        private void Setup_Load(object sender, EventArgs e)
        {
            label7.Text = "";
        }
        
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            label7.Text = Cursor.Position.X.ToString() + Cursor.Position.Y.ToString() + label7.Text;
            if (Encoding.UTF8.GetByteCount(label7.Text) > 32)
            {   
                var newArray = new byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(label7.Text), newArray, 32);
                label7.Text = Encoding.UTF8.GetString(newArray);
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
