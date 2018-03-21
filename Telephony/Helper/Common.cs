using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Telephony.Helper
{
    public class Common
    {
        public static string getExtensionNumberFromCall(string call)
        {
            Regex r = new Regex(@":(.+?)\@");
            Match mc = r.Match(call);
            return mc.Value.ToString().Replace("@", "").Replace(":",""); 
        }
        public static int getEpoch()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int res = (int)t.TotalSeconds;
            return res;
        }

        public static string passwordToAsterixs(string password)
        {
            string result = "*";
            if (password.Length > 1)
            {
                for (int x = 0; x < password.Length; x++)
                {
                    result += "*";
                }
            }

            return result;
        }
    }
}
