using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msMessenger
{
    public class TextMessage
    {

        public string ID { get; set; }
        public string Type { get; set; }
        public string Timestamp { get; set; }
        public string DID { get; set; }
        public string ContactNumber { get; set; }
        public string Message { get; set; }
        public bool Unread { get; set; }

    }
}
