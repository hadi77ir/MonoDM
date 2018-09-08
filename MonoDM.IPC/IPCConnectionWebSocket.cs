using System;
using System.Collections.Generic;
using System.Linq;
using Json = Newtonsoft.Json;
using WS = WebSocketSharp;
using WSS = WebSocketSharp.Server;
namespace MonoDM.IPC
{
    /// <summary>
    /// Cross-platform IPC implementation using WebSockets.
    /// Usable between app instances, browsers, Node.js apps and more.
    /// </summary>
    public class IPCConnectionWebSocket : BaseIPCConnection
    {
        public const int ListeningPort = 32703;
        protected WSS.WebSocketServer Server;
        protected WS.WebSocket Client;
        public override bool HasAnswerCapability => IsServer;

        public IPCConnectionWebSocket()
        {
            try
            {
                Server = new WSS.WebSocketServer(ListeningPort, false);
                Server.AddWebSocketService<IPCWSBehavior>("/", () => new IPCWSBehavior(this));
                Server.Start();
            }catch(Exception e){
                IsServer = false;
                Client = new WS.WebSocket("ws://127.0.0.1:" + ListeningPort + "/");
                Client.OnMessage += ClientReceivedMsg;
                Client.Connect();
            }finally{
                IsServer = true;
            }
        }
        ~IPCConnectionWebSocket(){
            Dispose();
        }
        public override void Dispose(){
            if (IsServer)
            {
                Server.Stop();
            }
            else
            {
                Client.Close();
            }
        }
        protected virtual void ClientReceivedMsg(object sender, WS.MessageEventArgs args)
        {
            if(args.IsText)
            {
                var obj = Json.Linq.JObject.Parse(args.Data)["message"];
                Message msg = obj.Type == Json.Linq.JTokenType.Object ?
                         Json.JsonConvert.DeserializeObject<Message>(obj.ToString()) :
                         new Message { RequestedMethod = "message", Parameters = obj.Values<string>().ToArray() };

                OnMessageReceived(this, new MessageReceivedEventArgs { Message = msg });
            }
        }
        public class MessageContainer{
            public object message { get; set; }
        }
        public class IPCWSBehavior : WSS.WebSocketBehavior, IIPCBehavior
        {
            private IPCConnectionWebSocket _cn;
            public bool HasAnswerCapability => true;
            public IPCWSBehavior(IPCConnectionWebSocket cn)
            {
                _cn = cn;
            }
            protected override void OnMessage(WS.MessageEventArgs e)
            {
                if(e.IsText)
                {
                    var obj = Json.Linq.JObject.Parse(e.Data)["message"];
                    Message msg = obj.Type == Json.Linq.JTokenType.Object ? 
                             Json.JsonConvert.DeserializeObject<Message>(obj.ToString()) : 
                             new Message { RequestedMethod = "message", Parameters = obj.Values<string>().ToArray() };
                    _cn.OnMessageReceived(this, new MessageReceivedEventArgs { Message = msg });
                }
            }

            public bool Answer(object msg)
            {
                try { Send(Json.JsonConvert.SerializeObject(new MessageContainer{ message = msg })); }
                catch { return false; }

                return true;
            }
            public bool Answer(string msg)
            {
                try { Send(Json.JsonConvert.SerializeObject(new MessageContainer{ message = msg })); }
                catch { return false; }

                return true;
            }
        }

        public override bool SendMessage(string method, string[] parameters)
        {
            return SendMessage(new Message
            {
                Parameters = parameters,
                RequestedMethod = method
            });
        }

        public override bool SendMessage(Message msg)
        {
            string json = Json.JsonConvert.SerializeObject(new MessageContainer{ message = msg });
            if(IsServer){
                try{
                    Server.WebSocketServices.Broadcast(json);
                }
                catch{
                    return false;
                }
                return true;
            }
            else{
                try{
                    Client.Send(json);
                }catch{
                    return false;
                }
                return true;
            }
        }

        protected override void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            base.OnMessageReceived(sender, e);
        }
    }
}
