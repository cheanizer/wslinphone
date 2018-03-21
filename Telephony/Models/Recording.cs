using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telephony.Models
{
    class Recording
    {
        public enum RecType {Input,Output,All}
        public enum RecStatUpload { Unuploaded, Uploaded, Failed}
        public enum RecDirection { Inbound, Outbound}

        public int Id { get; set; }
        public string Filename { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime StopAt { get; set; }
        public double Duration { get; set; }
        public string Location { get; set; }
        public RecType Type { get; set; }
        public RecStatUpload Status { get; set; }
        public RecDirection Direction { get; set; }
        public string CallerId { get; set; }
    }
}
