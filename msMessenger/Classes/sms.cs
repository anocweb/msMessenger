using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace msMessenger
{
    class sms
    {

        public static object getSMS(string username, string password, string did, string dateBegin, string dateEnd = "", string type = "", string contact = "", string limit = "2000", string timezone = "")
        {
            //minimum required information otherwise fail
            if (username == "" || password == "" || did == "" || dateBegin == "")
            {
                Console.WriteLine("getSMS Failed. Cannot have empty values.");
                return "failed_emptyvalues";
            }

            //Check date ranges
            if (dateEnd.Length > 0) {
                double days = (DateTime.Parse(dateEnd + " 23:59:59") - DateTime.Parse(dateBegin + " 00:00:00")).TotalDays;
                if (days > 90 | days <= 0)
                {
                    Console.WriteLine("getSMS Failed. Date range must be with 1-90 days");
                    return "failed_baddate";
                }
            }
            
            if (!utilities.isNumber(did))
               
            {
                Console.WriteLine("getSMS Failed. DID must be numeric value");
                return "failed_didnotnumeric";
            }

            Console.WriteLine("Updating Messages from " + dateBegin);
            Console.WriteLine("Current time: " + DateTime.Now.ToShortTimeString());

            ms.voip.VoIPms_Service soap = new ms.voip.VoIPms_Service();
            ms.voip.getSMSInput input = new ms.voip.getSMSInput();
            string status = null;

            //Fill Input Object
            input.api_username = username;
            input.api_password = password;
            input.did = did;
            input.from = dateBegin;
            input.to = dateEnd;
            input.type = type;
            input.contact = contact;
            input.limit = limit;
            input.timezone = timezone;

            //Request Info
            XmlNode[] output = (XmlNode[])soap.getSMS(input);
            XmlElement result_status = (XmlElement)output.GetValue(1);
            status = result_status.ChildNodes[1].InnerText;
            if (status == "success")
            {
                XmlElement messages = (XmlElement)output.GetValue(2);
                Console.WriteLine("getSMS Successful. SMS Count: " + messages.ChildNodes[1].ChildNodes.Count.ToString());
                return (XmlElement)messages.ChildNodes[1];
            }
                return status;
        }

        public static object sendSMS(string username, string password, string did, string destination, string smsContent)
        {
            //minimum required information otherwise fail
            if (username == "" || password == "" || did == "" || destination == "" || smsContent == "")
            {
                Console.WriteLine("sendSMS Failed. Cannot have empty values.");
                return "failed_emptyvalues";
            }

            if (!utilities.isNumber(did) || !utilities.isNumber(destination))

            {
                Console.WriteLine("sendSMS Failed. DID or Destination must be numeric values");
                return "failed_notnumeric";
            }

            ms.voip.VoIPms_Service soap = new ms.voip.VoIPms_Service();
            ms.voip.sendSMSInput input = new ms.voip.sendSMSInput();

            input.api_username = username;
            input.api_password = password;
            input.did = did;
            input.dst = destination;

            XmlNode[] output = null;
            XmlElement result_status = null;
            XmlElement result_id = null;

            //used to update messageData
            List<TextMessage> smsUpdateList = new List<TextMessage>();
            TextMessage insertMessage = new TextMessage();
            insertMessage.Type = "0"; // 0 Sent / 1 Received
            insertMessage.ContactNumber = destination;
            insertMessage.DID = did;
            insertMessage.Unread = false;

            string[] smsDataArray = utilities.str_SplitChunk(smsContent, 160); // text messages must be split into 160 character chunks to be valid. voip.ms will return error if sent without splitting
            
            foreach (string smsString in smsDataArray)
            {
                input.message = smsString;
                insertMessage.Message = smsString;

                //try and send the message
                output = (XmlNode[])soap.sendSMS(input);
                result_status = (XmlElement)output.GetValue(1);

                if (result_status.ChildNodes[1].InnerText == "success")
                {
                    // voip.ms generated sms ID
                    result_id = (XmlElement)output.GetValue(2);
                    insertMessage.ID = result_id.ChildNodes[1].InnerText; // Set voip.ms ID
                    insertMessage.Timestamp = utilities.getNewDateStr(); // Get the
                    
                }
                else
                {
                    //failed messages need to still be added. They need to be either resent later or deleted.
                    insertMessage.ID = result_id.ChildNodes[1].InnerText; // Set to failure message right now
                    insertMessage.Timestamp = result_id.ChildNodes[1].InnerText; // Set to failure message right now
                }
                smsUpdateList.Add(insertMessage);

            }
            smsUpdateList.Reverse(); // Messages need to be last sent at the top
            return smsUpdateList;
        }

    }
}
