using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msMessenger
{
    class utilities
    {
        public static bool isNumber(string data) {
            long n;
            if (long.TryParse(data, out n))
            {
                return true;
            }
            return false;
        }

        public static string[] str_SplitChunk(string str, int chunkSize)
        {
            if (str.Length == 0 || chunkSize == 0 || chunkSize >= str.Length)
            {
                return new string[] { str }; // just return the string for now
            }

            string[] chunks = new string[(int)Math.Ceiling((decimal)str.Length / (decimal)chunkSize)];
            for (int i = 0; i <= chunks.Count(); i++)
            {
                chunks[i] = str.Substring((i - 1) * chunkSize, chunkSize);
            }

            return chunks;
        }

        public static string getNewDateStr(bool withTime = true)
        {
            if (withTime)
            {
                return DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + " " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00");
            }
            else
            {
                return DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00");
            }
        }


    }
}
