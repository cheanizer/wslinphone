using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Telephony
{
    public class Laputa : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            WebSocketKu.cobaconsole(e.Data);
            WebSocketKu.translate(e.Data);
        }
    }

    public class Action
    {

    }
    class WebSocketKu
    {
        WebSocketServer ws;
        public static string address = "ws://localhost:3487";
        public delegate void OnMessageRecived (string message);
        public delegate void OnSocketOpened();
        public delegate void OnTransated(Action action, string[] param);
        public delegate void OnErrorTranslated(string message);
        public delegate void OnRegister(string hosts, string ext, string password);
        public delegate void DoCall(string extension);
        public delegate void OnTerminate();
        public delegate void OnRecieve();
        

        public static event OnMessageRecived MessageRecived;
        public static event OnSocketOpened SocketOpened;
        public static event OnTransated Translated;
        public static event OnErrorTranslated ErrorTranslated;
        public static event OnRegister Register;
        public static event DoCall Call;
        public static event OnTerminate Terminate;
        public static event OnRecieve Recieve;
        

        public enum Action {
            Empty,
            Register,
            Call,
            Hangup,
            Answer
        };
       
        public void start()
        {
            ws = new WebSocketServer ("ws://localhost:3487");
            ws.AddWebSocketService<Laputa> ("/Laputa");
            ws.Start ();
            
        }

        public void stop()
        {
            ws.Stop();
        }

        public static void cobaconsole(string message)
        {
            Console.WriteLine("message " + message );
            MessageRecived(message);
        }

        public void send(string message)
        {
            ws.WebSocketServices["/Laputa"].Sessions.Broadcast(message);
        }

        public static void translate(string message)
        {
            message = message.Trim('"',' ');
            string[] split = message.Split('|');
            if (string.IsNullOrEmpty(split[0]))
            {
                ErrorTranslated("Error Empty Mode");
            }
            else
            {
                switch (split[0])
                {
                    case "register":
                        if (string.IsNullOrEmpty(split[1]) || string.IsNullOrEmpty(split[2]) || string.IsNullOrEmpty(split[3]))
                        {
                            ErrorTranslated("Empty parameter");
                        }
                        else
                        {
                            Register(split[1], split[2], split[3]);
                        }
                        break;
                    case "call":
                        if (string.IsNullOrEmpty(split[1]))
                            ErrorTranslated("Empty parameter");
                        else
                            Call(split[1]);
                        break;
                    case "recieve":
                            Recieve();
                        break;
                    case "terminate":
                        Terminate();
                        break;
                }
            }
        }

        

        public static void opened()
        {
            SocketOpened();
        }
    }
}
