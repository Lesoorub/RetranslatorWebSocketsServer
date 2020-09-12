using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json.Linq;

namespace RetranslatorWebSocketsServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var wss = new WebSocketServer(2020);
            wss.AddWebSocketService<General>("/");
            wss.Start();
            while(wss.IsListening)
                Console.Read();
        }
    }
    class General : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Console.WriteLine("[O] " + ID);
            Send("open", ID);
        }
        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("[C] " + ID);
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.IsText)
            {
                Console.WriteLine("> " + e.Data);
                JObject json = JObject.Parse(e.Data);
                string type = "BR";
                if (json["type"] != null)
                    type = json["type"].ToString();
                switch (type)
                {
                    case "TO":
                        if (json["to"].ToString() != null)
                        {
                            string to = json["to"].ToString();
                            Sessions.SendToAsync(e.Data, to, null);
                        }
                        break;
                    case "ALL":
                        Sessions.BroadcastAsync(e.Data, null);
                        break;
                    default:
                        foreach (var s in Sessions.Sessions)
                            if (s.ID != ID)
                                Sessions.SendToAsync(e.Data, s.ID, null);
                        break;
                }
            }
            if (e.IsBinary)
            {
                foreach (var s in Sessions.Sessions)
                    if (s.ID != ID)
                        Sessions.SendToAsync(e.RawData, s.ID, null);
            }
        }


        public void Send(string @event, JToken data) =>
            Send(@event, data.ToString(Newtonsoft.Json.Formatting.None));
        public void Send(string @event, string data)
        {
            SendAsync($"{{\"event\":\"{@event}\", \"data\":{data}}}", null);
        }
    }
}
