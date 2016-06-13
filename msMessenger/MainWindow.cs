/*
 * 
 * msMessenger main winform GUI and methods
 * Copyright (C) 2016  Clinton Jarvis
 * WWW: jarvis.im   Email: clinton@jarvis.im
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using msMessenger.Resources;
using System.Xml;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;

namespace msMessenger
{

    public partial class msMessenger : MaterialForm
    {
        #region "Intialize, Load, and Close"

        private readonly MaterialSkinManager materialSkinManager;
        
        public List<TextMessage> messageData = new List<TextMessage>();
        public Dictionary<string, string> contactData = new Dictionary<string, string>();
        public SettingsCollection activeSettings = new SettingsCollection();
        bool setup = true;
        public WebServer ws;
        
        public msMessenger()
        {
            InitializeComponent();
            // Initialize MaterialSkinManager
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Red800, Primary.Red900, Primary.Red500, Accent.Red200, TextShade.WHITE);
        }


        public string SendResponse(HttpListenerRequest request)
        {
            Console.WriteLine("Received callback! Checking for messages...");
            if (activeSettings.ActiveDID != "")
            {
                object output = sms.getSMS(activeSettings.Username, activeSettings.Password, activeSettings.ActiveDID, activeSettings.LastServerUpdate.Substring(0, 10));
                if (output is XmlElement)
                {
                    XmlElement smsData = (XmlElement)output;
                    updateMessageData(smsData);
                    refreshMainMessagesList();
                    activeSettings.LastServerUpdate = utilities.getNewDateStr();
                    serverUpdate_label.Text = "Messages Updated: " + activeSettings.LastServerUpdate;
                    Properties.Settings.Default.LastServerUpdate = activeSettings.LastServerUpdate;
                }
                else
                {
                    Console.WriteLine("Failed: getSMS: " + (string)output);
                }
            }
            return "{ \"status\": \"success\" }";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ActiveDID == "")
            {
                Setup settingsForm = new Setup();
                DialogResult result = settingsForm.ShowDialog();
                if (result == DialogResult.Abort)
                {
                    setup = false;
                    Application.Exit();
                }
            }
            ws = new WebServer(SendResponse, "http://*:11010/");
            ws.Run();
            
            MainMessagePanel.HorizontalScroll.Enabled = false;
            MainMessagePanel.VerticalScroll.Enabled = true;
            
            systemUID_text.Text = FingerPrint.Value();
            
                voipUsername_input.Text = Properties.Settings.Default.Username;
                activeSettings.Username = Properties.Settings.Default.Username;

                // Decrypt
                byte[] data = Convert.FromBase64String(Properties.Settings.Default.Password);
                byte[] user = Convert.FromBase64String(Properties.Settings.Default.userKey);
                byte[] machine = Convert.FromBase64String(Properties.Settings.Default.machineKey);
                byte[] decdata = Encryption.AESThenHMAC.SimpleDecrypt(data, machine, user);
                voipPassword_input.Text = Encoding.UTF8.GetString(decdata);
                activeSettings.Password = voipPassword_input.Text;
                rememberCredentials_check.Checked = true;
            activeSettings.ActiveDID = Properties.Settings.Default.ActiveDID;
            activeDID_text.Text = Properties.Settings.Default.ActiveDID;
            activeSettings.LastServerUpdate = Properties.Settings.Default.LastServerUpdate;
            serverUpdate_label.Text = "Messages Updated: " + activeSettings.LastServerUpdate;
            //Pull existing data
            getSavedMessageData();
            refreshContacts(false);
            refreshMainMessagesList(false);

            // icon stuff
            NotifyIcon.Icon = Properties.Resources.Messenger_icon_blank_r_32;
            this.Icon = Properties.Resources.Messenger_icon_blank_r_32;

            // If connection is available, pull texts since last update
            if (activeSettings.ActiveDID != "")
            {
                MessageCheckTimer.Enabled = true;
                //object output = null; 
                //object output = sms.getSMS(activeSettings.Username, activeSettings.Password, activeSettings.ActiveDID, activeSettings.LastServerUpdate.Substring(0, 10));
                object output = sms.getSMS(activeSettings.Username, activeSettings.Password, activeSettings.ActiveDID, activeSettings.LastServerUpdate.Substring(0, 10));
                if (output is XmlElement)
                {
                    XmlElement smsData = (XmlElement)output;
                    updateMessageData(smsData);
                    refreshMainMessagesList();
                    activeSettings.LastServerUpdate = utilities.getNewDateStr();
                    serverUpdate_label.Text = "Messages Updated: " + activeSettings.LastServerUpdate;
                    Properties.Settings.Default.LastServerUpdate = activeSettings.LastServerUpdate;
                } else
                {
                    Console.WriteLine("Failed on load: getSMS: " + (string)output);
                }
            }
            
        }

        private void msMessenger_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (setup == true)
            {
                ws.Stop();
                NotifyIcon.Visible = false;
                // SAVE THINGS
                Properties.Settings.Default.Save();
                string saveLocation = AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\data.json";

                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    writer.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

                    writer.WriteStartObject();
                    writer.WritePropertyName("DatabaseVersion");
                    writer.WriteValue("1.0");
                    foreach (KeyValuePair<string, string> item in contactData)
                    {
                        writer.WritePropertyName("CONTACT");
                        writer.WriteStartObject();
                        writer.WritePropertyName("Name");
                        writer.WriteValue(item.Value);
                        writer.WritePropertyName("Number");
                        writer.WriteValue(item.Key);
                        writer.WriteEndObject();
                    }


                    foreach (TextMessage item in messageData)
                    {
                        writer.WritePropertyName("SMS");
                        writer.WriteStartObject();
                        writer.WritePropertyName("ID");
                        writer.WriteValue(item.ID);
                        writer.WritePropertyName("Timestamp");
                        writer.WriteValue(item.Timestamp);
                        writer.WritePropertyName("Type");
                        writer.WriteValue(item.Type);
                        writer.WritePropertyName("DID");
                        writer.WriteValue(item.DID);
                        writer.WritePropertyName("ContactNumber");
                        writer.WriteValue(item.ContactNumber);
                        writer.WritePropertyName("Unread");
                        writer.WriteValue(item.Unread);
                        writer.WritePropertyName("Message");
                        writer.WriteValue(item.Message);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                }

                File.WriteAllText(saveLocation, sb.ToString());
            }
        }

        #endregion

        #region "GUI Updates"

        /*
         * Material Theme
         */
        private void materialTheme_btn_Click(object sender, EventArgs e)
        {
            materialSkinManager.Theme = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Themes.LIGHT : MaterialSkinManager.Themes.DARK;

            //Change message page too since its broken
            //   Primary.Red800, Primary.Red900, Primary.Red500, Accent.Red200, TextShade.WHITE);
            if (materialSkinManager.Theme == MaterialSkinManager.Themes.DARK)
            {
                /*
                MainMessages_list.BackColor = Color.FromArgb(50, 50, 50);
                message_tab.BackColor = Color.FromArgb(50, 50, 50);
                ComposeMessage_img.BackColor = Color.FromArgb(50, 50, 50);

                for (int i = 0; i <= MainMessages_list.Items.Count - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        MainMessages_list.Items[i].BackColor = Color.FromArgb(35, 35, 35);
                    }
                    MainMessages_list.Items[i].SubItems[0].ForeColor = Color.White;
                    MainMessages_list.Items[i].SubItems[1].ForeColor = Color.White;
                    MainMessages_list.Items[i].SubItems[2].ForeColor = Color.White;
                }
                */

            }
            else
            {
                /*
                for (int i = 0; i <= MainMessages_list.Items.Count - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        MainMessages_list.Items[i].BackColor = Color.FromArgb(240, 240, 240);
                    }

                    MainMessages_list.Items[i].SubItems[0].ForeColor = Color.FromArgb(40, 40, 40);
                    MainMessages_list.Items[i].SubItems[1].ForeColor = Color.FromArgb(65, 65, 65);
                    MainMessages_list.Items[i].SubItems[2].ForeColor = Color.FromArgb(110, 110, 110);
                }


                MainMessages_list.BackColor = Color.White;
                message_tab.BackColor = Color.White;
                ComposeMessage_img.BackColor = Color.White;
                */
            }
        }

        /*
         * Compose and Conversion
         */
        private void ComposeMessage_img_Click(object sender, EventArgs e)
        {
            newMessageContent_text.Text = "";
            Conversation_list.Controls.Clear();
            toAddress_text.Enabled = true;
            toAddress_text.Text = "";
            toAddress_text.Focus();
            materialTabControl1.Visible = false;
            materialTabSelector1.Visible = false;
        }

        private void newMessageContent_text_KeyUp(object sender, KeyEventArgs e)
        {
            characterCount_label.Text = newMessageContent_text.Text.Length.ToString() + " / 160";
            if (newMessageContent_text.Text.Length > 0)
            {
                newMessage_Send.Enabled = true;
                if (e.Shift == true && e.KeyCode == Keys.Enter)
                {

                    newMessageContent_text.Enabled = false;
                    newMessage_Send.Enabled = false;
                    toAddress_text.Enabled = false;
                    string destination = parseContactToNumber(toAddress_text.Text);
                    object output = sms.sendSMS(activeSettings.Username, activeSettings.Password, activeSettings.ActiveDID, destination, newMessageContent_text.Text);
                    if (output is List<TextMessage>) // only success will be a List<> at the moment failures return strings.
                    {
                        updateMessageData(null, (List<TextMessage>)output); //update the sms database
                        populateConversationList(destination); //repopulate the conversation list to insert the new messages.
                    }
                    newMessageContent_text.Text = "";
                    newMessage_Send.Enabled = true;
                    newMessageContent_text.Enabled = true;
                    characterCount_label.Text = "0 / 160";
                    newMessageContent_text.Focus();

                }
            }
            else
            {
                newMessage_Send.Enabled = false;
            }
        }

        private void newMessage_Send_Click(object sender, EventArgs e)
        {
            newMessageContent_text.Enabled = false;
            newMessage_Send.Enabled = false;
            toAddress_text.Enabled = false;
            string destination = parseContactToNumber(toAddress_text.Text);
            object output = sms.sendSMS(activeSettings.Username, activeSettings.Password, activeSettings.ActiveDID, destination, newMessageContent_text.Text);

            if (output is List<TextMessage>) // only success will be a List<> at the moment failures return strings.
            {
                updateMessageData(null, (List<TextMessage>)output); //update the sms database
                populateConversationList(destination); //repopulate the conversation list to insert the new messages.
            }


            newMessageContent_text.Text = "";
            newMessage_Send.Enabled = true;
            newMessageContent_text.Enabled = true;
            characterCount_label.Text = "0 / 160";
            newMessageContent_text.Focus();
        }

        private void backToMessages_list_Click(object sender, EventArgs e)
        {
            materialTabControl1.Visible = true;
            materialTabSelector1.Visible = true;
            toAddress_text.Text = "";
            Conversation_list.Controls.Clear();
        }

        /*
         * Options and testing
         */
        private void materialCheckbox1_CheckedChanged(object sender, EventArgs e)
        {
            if (rememberCredentials_check.Checked)
            {
                Properties.Settings.Default.Username = voipUsername_input.Text;
                Properties.Settings.Default.Password = voipPassword_input.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Username = "";
                Properties.Settings.Default.Password = "";
                Properties.Settings.Default.Save();

            }
        }

        private void voipTestConnect_btn_Click(object sender, EventArgs e)
        {
            voipTestConnect_btn.Text = "Testing...";

            voipUsername_input.Enabled = false;
            voipPassword_input.Enabled = false;
            voipTestConnect_btn.Enabled = false;

            ms.voip.VoIPms_Service soap = new ms.voip.VoIPms_Service();
            ms.voip.getDIDsInfoInput input = new ms.voip.getDIDsInfoInput();
            XmlNode[] output;

            //XML Elements
            XmlElement result_status = null;
            XmlElement result_dids = null;
            XmlElement dids = null;
            XmlElement did = null;

            //String Vars
            string status = null;
            string str_dids = null;

            //Fill Input Object
            input.api_username = voipUsername_input.Text;
            input.api_password = voipPassword_input.Text;
            input.client = ""; // Set to nothing so we can get all the account DID's
            input.did = ""; // Same thing. need all the DID's

            //Request Info
            output = (XmlNode[])soap.getDIDsInfo(input);
            result_status = (XmlElement)output.GetValue(1);

            //if (output.GetType().ToString() == "System.Xml.XmlNode[]")
            //{
            //Get status
            status = result_status.ChildNodes[1].InnerText;
            //}
            voipTestConnect_btn.Text = status;

            if (status == "success")
            {
                result_dids = (XmlElement)output.GetValue(2);
                dids = (XmlElement)result_dids.ChildNodes[1];

                for (int i = 0; i <= dids.ChildNodes.Count - 1; i++)
                {
                    //Get DID
                    did = (XmlElement)dids.ChildNodes[i];

                    if (did.ChildNodes[21].ChildNodes[1].InnerText == "1")
                    {
                        activeDID_text.Text = did.ChildNodes[0].ChildNodes[1].InnerText;
                        Properties.Settings.Default.ActiveDID = did.ChildNodes[0].ChildNodes[1].InnerText;
                        Properties.Settings.Default.Save();
                    }

                    callbackUrl_text.Text = did.ChildNodes[26].ChildNodes[1].InnerText;
                    for (int j = 0; j <= did.ChildNodes.Count - 1; j++)
                    {

                        //Get Data from DID
                        str_dids += j.ToString() + ">>" + did.ChildNodes[j].ChildNodes[0].InnerText + " => ";
                        str_dids += did.ChildNodes[j].ChildNodes[1].InnerText + "\n";
                    }

                    str_dids += "\n";
                }
                MessageBox.Show(str_dids, "a bunch of data");
            }

            voipUsername_input.Enabled = true;
            voipPassword_input.Enabled = true;
            voipTestConnect_btn.Enabled = true;
        }

        private void callbackset_button_Click(object sender, EventArgs e)
        {
            callbackset_button.Text = "Setting Callback...";
            XmlElement result_status = null;
            XmlNode[] output = null;
            ms.voip.VoIPms_Service soap = new ms.voip.VoIPms_Service();
            ms.voip.setSMSInput input = new ms.voip.setSMSInput();

            input.api_password = activeSettings.Password;
            input.api_username = activeSettings.Username;
            input.did = activeSettings.ActiveDID;
            input.enable = "1";
            input.url_callback_enable = "1";
            input.url_callback = callbackUrl_text.Text;
            input.url_callback_retry = "0";

            output = (XmlNode[])soap.setSMS(input);
            result_status = (XmlElement)output.GetValue(1);


            if (result_status.ChildNodes[1].InnerText == "success")
            {
                //result_id = (XmlElement)output.GetValue(2);
                callbackset_button.Text = "Success!";
            }
            else
            {
                callbackset_button.Text = result_status.ChildNodes[1].InnerText;
            }
        }

        /*
         * Contacts
         */
        private void AddContact_img_Click(object sender, EventArgs e)
        {
            ContactList.Enabled = false;
            contactAction_label.Text = "New Contact";
            AddContact_img.Hide();
            ContactList.Height = 297;

        }

        private void contactActionCancel_button_Click(object sender, EventArgs e)
        {
            contactName_text.BackColor = Color.White;
            contactNumber_text.BackColor = Color.White;
            ContactList.Enabled = true;
            ContactList.Height = 471;
            AddContact_img.Show();
            contactName_text.Text = "";
            contactNumber_text.Text = "";
        }

        private void contactActionSave_button_Click(object sender, EventArgs e)
        {
            if (contactData.ContainsKey(contactNumber_text.Text) || contactNumber_text.Text == "")
            {
                contactNumber_text.BackColor = Color.LightCoral;
            }
            else
            {
                contactNumber_text.BackColor = Color.White;
            }

            if (contactData.ContainsValue(contactName_text.Text) || contactName_text.Text == "")
            {
                contactName_text.BackColor = Color.LightCoral;

            }
            else
            {
                contactName_text.BackColor = Color.White;
            }

            if (contactName_text.BackColor != Color.LightCoral && contactNumber_text.BackColor != Color.LightCoral)
            {
                contactData.Add(contactNumber_text.Text, contactName_text.Text);

                ListViewItem litem = new ListViewItem(new string[] { contactName_text.Text, contactNumber_text.Text });
                ContactList.Items.Add(litem);
                refreshMainMessagesList();
                ContactList.Enabled = true;
                ContactList.Height = 471;
                AddContact_img.Show();
                contactName_text.Text = "";
                contactNumber_text.Text = "";
            }
        }

        private void ContactList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                ContactList.Enabled = false;
                contactAction_label.Text = "Edit Contact";
                AddContact_img.Hide();
                contactNumber_text.Text = ContactList.SelectedItems[0].SubItems[1].Text;
                contactName_text.Text = ContactList.SelectedItems[0].SubItems[0].Text;
                ContactList.Height = 297;
            }
            if (e.Button == MouseButtons.Right)
            {

                contactData.Remove(ContactList.SelectedItems[0].SubItems[1].Text);
                ContactList.SelectedItems[0].Remove();
            }
        }

        /*
         * Main message list
         */
        private void MainMessages_list_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // So much hate
                /*
                populateConversationList(MainMessages_list.SelectedItems[0].Text);
                toAddress_text.Text = MainMessages_list.SelectedItems[0].Text;
                toAddress_text.Enabled = false;
                for (int i = 0; i <= Conversation_list.Items.Count - 1; i++)
                {
                    if (Conversation_list.Items[i].Text != "Me")
                    {
                        Conversation_list.Items[i].Text = MainMessages_list.SelectedItems[0].Text;
                    }
                }
                */
                newMessageContent_text.Focus();
                materialTabControl1.Visible = false;
                materialTabSelector1.Visible = false;
            }
        }

        #endregion

        #region "Data Management Methods"

        /*
         * Composing and conversations
         */
        private void addConverstationItem()
        {

        }

        private string parseContactToNumber(string data)
        {
            long n;
            bool isNumeric = long.TryParse(data, out n);
            if (!isNumeric)
            {
                return contactData.FirstOrDefault(x => x.Value == data).Key;
            }
            else
            {
                return data;
            }
        }

        private void populateConversationList(string contact)
        {
            Conversation_list.Controls.Clear();
            string contactLookup = contact;
            contactLookup = parseContactToNumber(contact);


            foreach (TextMessage item in messageData)
            {
                if (item.ContactNumber == contactLookup)
                {
                    string who = contact;
                    if (item.Type == "0")
                    {
                        who = "Me";
                    }
                    ListViewItem text = new ListViewItem(new string[] { who, item.Message, item.Timestamp }, 0);
                    text.UseItemStyleForSubItems = false;
                    text.SubItems[0].ForeColor = Color.FromArgb(40, 40, 40);
                    text.SubItems[1].ForeColor = Color.FromArgb(65, 65, 65);
                    text.SubItems[2].ForeColor = Color.FromArgb(110, 110, 110);
                    if (who == "Me")
                    {
                        text.BackColor = Color.FromArgb(240, 240, 240);

                    }
                    //Conversation_list.Items.Add(text);
                }
            }

        }

        /*
         * Main message list
         */
        private void refreshMainMessagesList(bool refresh = true)
        {
            if (refresh)
            {
                MainMessagePanel.Controls.Clear();
            }
            List<string> existCheckList = new List<string>();
            string contactName = "";
            foreach (TextMessage item in messageData)
            {
                if (!existCheckList.Contains(item.ContactNumber))
                {
                    contactName = item.ContactNumber;
                    if (contactData.ContainsKey(item.ContactNumber))
                    {
                        contactName = contactData[item.ContactNumber];
                    }
                    Message.message m = new Message.message();
                    m.messageName = contactName;
                    m.messageText = item.Message;
                    m.messageTimestamp = item.Timestamp;
                    MainMessagePanel.Controls.Add(m);


                    /*
                    ListViewItem text = new ListViewItem(new string[] { contactName, item.Message, item.Timestamp }, 0);
                    text.UseItemStyleForSubItems = false;
                    text.SubItems[0].ForeColor = Color.FromArgb(40, 40, 40);

                    text.SubItems[1].ForeColor = Color.FromArgb(65, 65, 65);
                    text.SubItems[2].ForeColor = Color.FromArgb(110, 110, 110);

                    MainMessages_list.Items.Add(text);
                    */
                    existCheckList.Add(item.ContactNumber);
                }
            }
        }

        /*
         * global data management
         */
        public void populateMessages(XmlElement messages)
        {
            //
            XmlElement message = null;

            for (int i = 0; i <= messages.ChildNodes.Count - 1; i++)
            {
                // Get individual SMS message data
                message = (XmlElement)messages.ChildNodes[i];

                //build a real TextMessage
                TextMessage testData = new TextMessage();

                testData.ID = message.ChildNodes[0].ChildNodes[1].InnerText;
                testData.Timestamp = message.ChildNodes[1].ChildNodes[1].InnerText;
                testData.Type = message.ChildNodes[2].ChildNodes[1].InnerText;
                testData.DID = message.ChildNodes[3].ChildNodes[1].InnerText;
                testData.ContactNumber = message.ChildNodes[4].ChildNodes[1].InnerText;
                testData.Message = message.ChildNodes[5].ChildNodes[1].InnerText;

                // If it doesn't exist in the database, add it.
                if (!messageData.Contains(testData))
                {
                    messageData.Add(testData);
                }
            }
        }

        

        public bool updateMessageData(XmlElement xmlData = null, List<TextMessage> textData = null)
        {
            List<TextMessage> newMessages = new List<TextMessage>();

            if (xmlData != null)
            {
                int count = 0;
                bool canAdd = true;
                for (int i = 0; i <= xmlData.ChildNodes.Count - 1; i++)
                {
                    XmlElement message = (XmlElement)xmlData.ChildNodes[i];
                    TextMessage testData = new TextMessage();
                    testData.ID = message.ChildNodes[0].ChildNodes[1].InnerText;
                    testData.Timestamp = message.ChildNodes[1].ChildNodes[1].InnerText;
                    testData.Type = message.ChildNodes[2].ChildNodes[1].InnerText;
                    testData.DID = message.ChildNodes[3].ChildNodes[1].InnerText;
                    testData.ContactNumber = message.ChildNodes[4].ChildNodes[1].InnerText;
                    testData.Message = message.ChildNodes[5].ChildNodes[1].InnerText;
                    testData.Unread = true;
                    for (int j = 0; j <= messageData.Count - 1; j++)
                    {
                        if (messageData[j].ID == testData.ID)
                        {
                            canAdd = false;
                        }
                    }
                    if (canAdd == true)
                    {
                        count++;
                        Console.WriteLine("Adding new text...");
                        newMessages.Add(testData);
                    }
                }
                Console.WriteLine("Total texts parsed: " + xmlData.ChildNodes.Count);
                Console.WriteLine("Total texts added: " + count);
                if (count > 0)
                {
                    // Notification sound and tray icon
                    NotifyIcon.BalloonTipText = count + " New Message";
                    if (count > 1)
                        NotifyIcon.BalloonTipText += "s";
                    NotifyIcon.BalloonTipTitle = "New Messages";
                    NotifyIcon.ShowBalloonTip(7000);
                    
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                    player.Stream = Properties.Resources.notification;
                    player.Play();
                }
            }

            if (textData != null)
            {
                newMessages = textData;
                //newMessages.Add(textData);
            }

            for (int j = 0; j <= messageData.Count - 1; j++)
            {
                newMessages.Add(messageData[j]);
            }

            messageData.Clear();
            messageData = newMessages;
            return true;
        }

        /* private void updateMessageData(TextMessage newMessage)
        {
            List<TextMessage> newMessages = new List<TextMessage>();
            newMessages.Add(newMessage);
            for (int j = 0; j <= messageData.Count - 1; j++)
            {
                newMessages.Add(messageData[j]);
            }
            messageData.Clear();
            messageData = newMessages;
        }*/

        private void refreshContacts(bool refresh = true)
        {
            if (refresh)
            {
                ContactList.Items.Clear();
            }
            foreach (KeyValuePair<string, string> item in contactData)
            {
                ContactList.Items.Add(new ListViewItem(new string[] { item.Value, item.Key }));
            }
        }

        private bool getSavedMessageData(string saveLocation = null)
        {
            if (saveLocation == null)
            {
                saveLocation = AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\data.json";
            }

            if (File.Exists(saveLocation))
            {
                string savedData = File.ReadAllText(saveLocation);
                string valuetemp = "";
                int c = 0;
                int m = 0;
                TextMessage item = new TextMessage();
                JsonTextReader reader = new JsonTextReader(new StringReader(savedData));
                string phone_name = "";
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        if (valuetemp == "SMS")
                        {
                            item = new TextMessage();
                            valuetemp = "";
                        }
                        if (valuetemp == "CONTACT")
                        {
                            phone_name = "";
                            valuetemp = "";
                        }
                        if (valuetemp == "Name")
                        {
                            phone_name = reader.Value.ToString();
                            valuetemp = "";
                        }

                        if (valuetemp == "ID")
                        {
                            item.ID = reader.Value.ToString();
                            valuetemp = "";
                        }
                        if (valuetemp == "Timestamp")
                        {
                            item.Timestamp = reader.Value.ToString();
                            valuetemp = "";
                        }
                        if (valuetemp == "Type")
                        {
                            item.Type = reader.Value.ToString();
                            valuetemp = "";
                        }
                        if (valuetemp == "DID")
                        {
                            item.DID = reader.Value.ToString();
                            valuetemp = "";
                        }
                        if (valuetemp == "ContactNumber")
                        {
                            item.ContactNumber = reader.Value.ToString();
                            valuetemp = "";
                        }
                        if (valuetemp == "Unread")
                        {
                            item.Unread = (bool)item.Unread;

                            valuetemp = "";
                        }
                        if (valuetemp == "Message")
                        {
                            item.Message = reader.Value.ToString();
                            messageData.Add(item);
                            m++;
                            valuetemp = "";
                        }

                        if (valuetemp == "Number")
                        {
                            contactData.Add(reader.Value.ToString(), phone_name);
                            c++;
                            valuetemp = "";
                        }


                        if (reader.TokenType.ToString() == "PropertyName")
                        {
                            valuetemp = reader.Value.ToString();
                        }
                    }
                }
                Console.WriteLine("Loaded " + m + " message(s) and " + c + " contacts");
                return true;
            }
            return false;
        }

        


        #endregion

        #region "Timer management"

        private void MessageCheckTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Tick!...");
            object output = sms.getSMS(activeSettings.Username, activeSettings.Password, activeSettings.ActiveDID, activeSettings.LastServerUpdate.Substring(0, 10));
            if (output is XmlElement)
            {
                XmlElement getSMSData = (XmlElement)output;
                bool returned = updateMessageData(getSMSData);
                refreshMainMessagesList();

                if (toAddress_text.Text != "" && Conversation_list.Controls.Count > 0)
                {
                    populateConversationList(toAddress_text.Text);
                }

                activeSettings.LastServerUpdate = utilities.getNewDateStr();
                Properties.Settings.Default.LastServerUpdate = activeSettings.LastServerUpdate;
                serverUpdate_label.Text = "Messages Updated: " + activeSettings.LastServerUpdate;
            } else {
                Console.WriteLine("Failed: getSMS: " + (string)output);
                }
                Console.WriteLine("...Tock!");
        }

        #endregion

        #region "Data Classes"

        

        public class SettingsCollection
        {

            public string Username { get; set; }
            public string Password { get; set; }
            public string ActiveDID { get; set; }
            public string LastServerUpdate { get; set; }

        }

        #endregion

        #region "TEST CODE AND OTHER USELESS STUFF"

        private string convertEmojiToText(string unicodeData)
        {
            



            return "";
        }


        #endregion

        private void MainMessagePanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            populateConversationList("4033995293");
            toAddress_text.Text = "Mom";
            toAddress_text.Enabled = false;
            newMessageContent_text.Focus();
            materialTabControl1.Visible = false;
            materialTabSelector1.Visible = false;
        }

        private void MainMessagePanel_MouseEnter(object sender, EventArgs e)
        {
            MainMessagePanel.Focus();
        }

        private void ComposeMessage_img_MouseHover(object sender, EventArgs e)
        {
         while (ComposeMessage_img.Left > 283)
            {
                
                ComposeMessage_img.Left = ComposeMessage_img.Left - 10;
                ComposeMessage_img.Refresh();
                System.Threading.Thread.Sleep(30);
            }   
        }

        private void ComposeMessage_img_MouseLeave(object sender, EventArgs e)
        {
            while (ComposeMessage_img.Left < 333)
            {
                ComposeMessage_img.Left = ComposeMessage_img.Left + 10;
                ComposeMessage_img.Refresh();
                System.Threading.Thread.Sleep(30);
            }
        }
    }
}
