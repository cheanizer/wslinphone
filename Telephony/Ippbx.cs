using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telephony
{
    class Ippbx
    {
        public Ippbx()
        {
        }
        public string hosts
        {
            get;set;
        }

        public string extension
        { get; set; }

        public string password { get; set; }
        public string callerid { get; set; }

    }
}
