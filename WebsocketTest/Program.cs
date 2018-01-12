using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebsocketTest
{

    public class Laputa : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = e.Data == "BALUS"
                      ? "I've been balused already..."
                      : "I'm not available now.";
            Console.WriteLine(e.Data);
            Send(msg);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var wssv = new WebSocketServer ("ws://localhost:8444");
              wssv.AddWebSocketService<Laputa> ("/Laputa");
              wssv.Start ();
              Console.ReadKey (true);
              wssv.Stop ();
        }
    }
}
