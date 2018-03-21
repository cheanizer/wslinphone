using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telephony.Models
{
    class CallerIdentity
    {
        public string Name { get; set; }
        public string Destination { get;  set; }
        public string Number { get; set; }
        public string Extension { get; set; }

        public CallerIdentity(string destination)
        {
            this.Destination = destination;
            getNumberExtension();
        }

        public void getNumberExtension()
        {
            if (this.Destination != null && this.Destination.Equals("") == false)
            {
                string d = this.Destination;
                string[] des = d.Split('.');
                if (des.Length > 1)
                {
                    this.Number = des[0];
                    this.Extension = des[1];
                }
                else
                {
                    this.Number = this.Destination;
                }
            }
        }
    }
}
