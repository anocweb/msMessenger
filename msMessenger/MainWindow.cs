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

namespace msMessenger
{

    public partial class msMessenger : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("And now, instead of mounting barded steeds");
        public List<TextMessage> messageData = new List<TextMessage>();
        public Dictionary<string, string> contactData = new Dictionary<string, string>();
        public SettingsCollection globalSettings = new SettingsCollection();

        public msMessenger()
        {
            InitializeComponent();
            // Initialize MaterialSkinManager
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Red800, Primary.Red900, Primary.Red500, Accent.Red200, TextShade.WHITE);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Load Settings
            if (Properties.Settings.Default.Password != "" && Properties.Settings.Default.Username != "")
            {
                voipPassword_input.Text = Properties.Settings.Default.Password; // kinda skipping the encryption atm.....
                voipUsername_input.Text = Properties.Settings.Default.Username; // kinda skipping the encryption atm.....
                globalSettings.Username = Properties.Settings.Default.Username;
                globalSettings.Password = Properties.Settings.Default.Password;
                rememberCredentials_check.Checked = true;
            }
            globalSettings.ActiveDID = Properties.Settings.Default.ActiveDID;
            activeDID_text.Text = Properties.Settings.Default.ActiveDID;
            globalSettings.LastServerUpdate = Properties.Settings.Default.LastServerUpdate;
            serverUpdate_label.Text = globalSettings.LastServerUpdate;
            //Pull existing data
            getSavedMessageData(@"C:\Users\Clinton\Desktop\SavedData.json");
            refreshContacts(false);
            refreshMainMessagesList(false);
            // If connection is available, pull texts since last update
            if (globalSettings.ActiveDID != "")
            {
                MessageCheckTimer.Enabled = true;
                XmlElement returnedData = getSMS(activeDID_text.Text, globalSettings.LastServerUpdate, "", "", "", "", ""); //NOPE Using fake date for testing
                if (returnedData != null)
                {
                    updateMessageData(returnedData);
                    refreshMainMessagesList();
                    globalSettings.LastServerUpdate = getNewTime();
                    serverUpdate_label.Text = globalSettings.LastServerUpdate;
                    Properties.Settings.Default.LastServerUpdate = globalSettings.LastServerUpdate;
                } else
                {
                    Console.WriteLine("Failed to connect!");
                }
            }
        }

        private void updateMessageData(XmlElement messages)
        {
            List<TextMessage> newMessages = new List<TextMessage>();
            int count = 0;
            bool canAdd = true;
            for (int i = 0; i <= messages.ChildNodes.Count - 1; i++)
            {
                XmlElement message = (XmlElement)messages.ChildNodes[i];
                TextMessage testData = new TextMessage();
                testData.ID = message.ChildNodes[0].ChildNodes[1].InnerText;
                testData.Timestamp = message.ChildNodes[1].ChildNodes[1].InnerText;
                testData.Type = message.ChildNodes[2].ChildNodes[1].InnerText;
                testData.DID = message.ChildNodes[3].ChildNodes[1].InnerText;
                testData.ContactNumber = message.ChildNodes[4].ChildNodes[1].InnerText;
                testData.ContactName = message.ChildNodes[4].ChildNodes[1].InnerText;
                testData.Text = message.ChildNodes[5].ChildNodes[1].InnerText;
                for (int j = 0; j <= messageData.Count -1; j++)
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
            for (int j = 0; j <= messageData.Count - 1; j++)
            {
                newMessages.Add(messageData[j]);
            }
            messageData.Clear();
            messageData = newMessages;
            Console.WriteLine("Total texts parsed: " + messages.ChildNodes.Count);
            Console.WriteLine("Total texts added: " + count);

        }

        private void updateMessageData(TextMessage newMessage)
        {
            List<TextMessage> newMessages = new List<TextMessage>();
            newMessages.Add(newMessage);
            for (int j = 0; j <= messageData.Count - 1; j++)
            {
                newMessages.Add(messageData[j]);
            }
            messageData.Clear();
            messageData = newMessages;
        }

        private void materialTheme_btn_Click(object sender, EventArgs e)
        {
            materialSkinManager.Theme = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Themes.LIGHT : MaterialSkinManager.Themes.DARK;

            //Change message page too since its broken
            //   Primary.Red800, Primary.Red900, Primary.Red500, Accent.Red200, TextShade.WHITE);
            if (materialSkinManager.Theme == MaterialSkinManager.Themes.DARK)
            {
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

            }
            else
            {

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
            }
        }



        private void ComposeMessage_img_Click(object sender, EventArgs e)
        {
            newMessageContent_text.Text = "";
            Conversation_list.Items.Clear();
            toAddress_text.Text = "";
            toAddress_text.Focus();
            materialTabControl1.Visible = false;
            materialTabSelector1.Visible = false;
        }

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

        private bool sendTextMessage(TextMessage newMessage)
        {
            XmlElement result_status = null;
            XmlNode[] output = null;
            ms.voip.VoIPms_Service soap = new ms.voip.VoIPms_Service();
            ms.voip.sendSMSInput input = new ms.voip.sendSMSInput();
            decimal charcount = newMessage.Text.Count();
            decimal usedChar = 0;
            input.api_username = globalSettings.Username;
            input.api_password = globalSettings.Password;
            input.did = globalSettings.ActiveDID;
            input.dst = newMessage.ContactNumber;
            int mcount = (int)Math.Ceiling(charcount / 160);
            while (mcount != 0) {
                if (mcount > 1)
                {
                    input.message = newMessage.Text.Substring((int)usedChar, 160);
                    usedChar += 160;
                } else {
                    input.message = newMessage.Text.Substring((int)usedChar, ((int)charcount- (int)usedChar));
                }

                output = (XmlNode[])soap.sendSMS(input);
                result_status = (XmlElement)output.GetValue(1);
                MessageBox.Show(result_status.ChildNodes[1].InnerText);
                if (result_status.ChildNodes[1].InnerText != "success")
                {
                    mcount = 0;
                }
                else
                {
                    mcount--;
                }
                MessageCheckTimer.Interval = 15000;
                BoostCheck.Enabled = true;
                return false;

             }




            



            return false;
        }

        private void newMessageContent_text_KeyUp(object sender, KeyEventArgs e)
        {
            characterCount_label.Text = newMessageContent_text.Text.Length.ToString() + " / 160";
            if (newMessageContent_text.Text.Length > 0)
            {
                newMessage_Send.Enabled = true;
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
            string asd = toAddress_text.Text;
            TextMessage newmessage = new TextMessage();
            newmessage.ContactName = asd;
            newmessage.ContactNumber = parseContactToNumber(asd);
            newmessage.Text = newMessageContent_text.Text;
            object result = sendTextMessage(newmessage);

            if (result is bool)
            {
                // going to add this soon... updateMessageData()
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
        }

        private void MainMessages_list_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // So much hate
                populateConversationList(MainMessages_list.SelectedItems[0].Text);
                toAddress_text.Text = MainMessages_list.SelectedItems[0].Text;
                for (int i = 0; i <= Conversation_list.Items.Count - 1; i++)
                {
                    if (Conversation_list.Items[i].Text != "Me")
                    {
                        Conversation_list.Items[i].Text = MainMessages_list.SelectedItems[0].Text;
                    }
                }
                
                newMessageContent_text.Focus();
                materialTabControl1.Visible = false;
                materialTabSelector1.Visible = false;
            }
        }
        
        public XmlElement getSMS(string did, string from = "", string to = "", string type = "", string contact = "", string limit = "", string timezone = "")
        {
            Console.WriteLine("Updating Messages from " + from);
            ms.voip.VoIPms_Service soap = new ms.voip.VoIPms_Service();
            ms.voip.getSMSInput input = new ms.voip.getSMSInput();
            XmlNode[] output;

            //XML Elements
            System.Xml.XmlElement result_status = null;
            XmlElement messages = null;

            //String Vars
            string status = null;

            //Fill Input Object
            input.api_username = globalSettings.Username;
            input.api_password = globalSettings.Password;
            input.did = did;
            input.from = from;
            input.to = to;
            input.type = type;
            input.contact = contact;
            input.limit = "2000";
            input.timezone = timezone;

            //Request Info
            output = (XmlNode[])soap.getSMS(input);

            result_status = (XmlElement)output.GetValue(1);

            status = result_status.ChildNodes[1].InnerText;

            if (status == "success")
            {
                messages = (XmlElement)output.GetValue(2);
                Console.WriteLine("# Retreived: " + messages.ChildNodes[1].ChildNodes.Count.ToString());
                return (XmlElement)messages.ChildNodes[1];
            }
            else
            {
                return null;
            }

        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            /*materialRaisedButton1.Text = "Getting SMS Data...";

            XmlElement returnedData = getSMS(globalSettings.ActiveDID, globalSettings.LastServerUpdate, "", "", "", "", "");
            materialRaisedButton1.Text = "Data Collected...";

            if (returnedData != null)
            {
                materialRaisedButton1.Text = "Done!";
            }
            else
            {
                materialRaisedButton1.Text = "Failed!";
            }*/

            MessageCheckTimer.Interval = 30000;
            BoostCheck.Enabled = true;


        }

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
            } else
            {
                return data;
            }
        }

        private void populateConversationList(string contact)
        {
            Conversation_list.Items.Clear();
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
                    ListViewItem text = new ListViewItem(new string[] { who, item.Text, item.Timestamp }, 0);
                    text.UseItemStyleForSubItems = false;

                    // Font for contact
                    text.SubItems[0].Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold, GraphicsUnit.Point);
                    text.SubItems[0].ForeColor = Color.FromArgb(40, 40, 40);

                    // Font for message data
                    text.SubItems[1].Font = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);
                    text.SubItems[1].ForeColor = Color.FromArgb(65, 65, 65);

                    // Font for timestamp
                    text.SubItems[2].Font = new Font("Segoe UI", 8, FontStyle.Regular, GraphicsUnit.Point);
                    text.SubItems[2].ForeColor = Color.FromArgb(110, 110, 110);
                    if (who == "Me")
                    {
                        text.BackColor = Color.FromArgb(240, 240, 240);
                    }
                    Conversation_list.Items.Add(text);
                }
            }

        }

        private void refreshMainMessagesList(bool refresh = true)
        {
            if (refresh)
            {
                MainMessages_list.Clear();
            }
            List<string> existCheckList = new List<string>();
            string contactFill = "";
            foreach (TextMessage text in messageData)
            {
                if (!existCheckList.Contains(text.ContactNumber))
                {
                    if (contactData.ContainsKey(text.ContactNumber))
                    {
                        contactFill = contactData[text.ContactNumber];
                    } else 
                    {
                        contactFill = text.ContactNumber;
                    }
                    ListViewItem item = new ListViewItem(new string[] { contactFill, text.Text, text.Timestamp }, 0);

                    item.UseItemStyleForSubItems = false;
                    item.SubItems[0].Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold, GraphicsUnit.Point);
                    item.SubItems[0].ForeColor = Color.FromArgb(40, 40, 40);
                    item.SubItems[1].Font = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);
                    item.SubItems[1].ForeColor = Color.FromArgb(65, 65, 65);
                    item.SubItems[2].Font = new Font("Segoe UI", 8, FontStyle.Regular, GraphicsUnit.Point);
                    item.SubItems[2].ForeColor = Color.FromArgb(110, 110, 110);
                    MainMessages_list.Items.Add(item);
                    existCheckList.Add(text.ContactNumber);
                }
            }
        }
        
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
                testData.Text = message.ChildNodes[5].ChildNodes[1].InnerText;

                // If it doesn't exist in the database, add it.
                if (!messageData.Contains(testData))
                {
                    messageData.Add(testData);
                }
            }
        }



        public string getNewTime()
        {
            return DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + " " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00");
        }

        private void MessageCheckTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Tick!...");

            XmlElement returnedData = getSMS(globalSettings.ActiveDID, "", globalSettings.LastServerUpdate, "", "", "", "");
            updateMessageData(returnedData);
            refreshMainMessagesList();
            globalSettings.LastServerUpdate = getNewTime();
            Properties.Settings.Default.LastServerUpdate = globalSettings.LastServerUpdate;
            serverUpdate_label.Text = globalSettings.LastServerUpdate;
            Console.WriteLine("...Tock!");
        }

        private void msMessenger_FormClosing(object sender, FormClosingEventArgs e)
        {
            // SAVE THINGS
            Properties.Settings.Default.Save();
            string saveLocation = @"C:\Users\Clinton\Desktop\SavedData.json";

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
                    writer.WritePropertyName("ContactName");
                    writer.WriteValue(item.ContactName);
                    writer.WritePropertyName("Text");
                    writer.WriteValue(item.Text);
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }

            File.WriteAllText(saveLocation, sb.ToString());

        }

        // data types

        public class TextMessage
        {

            public string ID { get; set; }
            public string Type { get; set; }
            public string Timestamp { get; set; }
            public string DID { get; set; }
            public string ContactNumber { get; set; }
            public string ContactName { get; set; }
            public string Text { get; set; }

        }


        public class SettingsCollection
        {

            public string Username { get; set; }
            public string Password { get; set; }
            public string ActiveDID { get; set; }
            public string LastServerUpdate { get; set; }

        }

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
            if (contactData.ContainsKey(contactNumber_text.Text) || contactNumber_text.Text == "") {
                contactNumber_text.BackColor = Color.LightCoral;
            } else
            {
                contactNumber_text.BackColor = Color.White;
            }

            if (contactData.ContainsValue(contactName_text.Text) || contactName_text.Text == "") {
                contactName_text.BackColor = Color.LightCoral;

            } else
            {
                contactName_text.BackColor = Color.White;
            }

            if (contactName_text.BackColor != Color.LightCoral && contactNumber_text.BackColor != Color.LightCoral) {
                contactData.Add(contactNumber_text.Text, contactName_text.Text);

                ListViewItem litem = new ListViewItem(new string[] { contactName_text.Text, contactNumber_text.Text });
                ContactList.Items.Add(litem);

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

        private void refreshContacts(bool refresh = true)
        {
            if (refresh)
            {
                ContactList.Items.Clear();
            }
                foreach (KeyValuePair<string,string> item in contactData)
            {
                ContactList.Items.Add(new ListViewItem(new string[] {item.Value,item.Key}));
            }
        }


        private bool getSavedMessageData(string saveLocation = null)
        {
            if (saveLocation == null) {
            saveLocation = AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\data.json";
            }
    
            if (File.Exists(saveLocation))
            {
                string savedData = File.ReadAllText(saveLocation);
                string valuetemp = "";
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
                        if (valuetemp == "ContactName")
                        {
                            item.ContactName = null;
                            if (reader.Value != null && reader.Value.ToString() != "Text")
                            {
                                item.ContactName = reader.Value.ToString();
                            }
                            valuetemp = "";
                        }
                        if (valuetemp == "Text")
                        {
                            item.Text = reader.Value.ToString();
                            messageData.Add(item);
                            valuetemp = "";
                        }

                        if (valuetemp == "Number")
                        {
                            contactData.Add(reader.Value.ToString(), phone_name);
                            valuetemp = "";
                        }


                        if (reader.TokenType.ToString() == "PropertyName")
                        {
                            valuetemp = reader.Value.ToString();
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private void BoostCheck_Tick(object sender, EventArgs e)
        {
            MessageCheckTimer.Interval = 900000;
            BoostCheck.Enabled = false;
        }
        
    }
}
