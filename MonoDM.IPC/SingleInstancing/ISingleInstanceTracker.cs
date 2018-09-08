using System;
using System.Runtime.Serialization;
using Json = Newtonsoft.Json;
namespace MonoDM.IPC.SingleInstancing
{
    /// <summary>
    /// Represents an object used to check for a previous instance of an application, and sending messages to it.
    /// </summary>
    public class SingleInstanceTracker<TIPC> where TIPC : BaseIPCConnection
    {
        public BaseIPCConnection IPC;
        public SingleInstanceTracker()
        {
            try
            {
                IPC = (BaseIPCConnection)typeof(TIPC).GetConstructor(new Type[0]).Invoke(new object[0]);
                IPC.MessageReceived += Ipc_MessageReceived;
                if (!IsFirstInstance)
                {
                    IPC.SendMessage("singleinstancing", new[] { "new_instance" });
                }
            }catch(Exception ex){
                throw new SingleInstancingException("Couldn't initialize single-instancing tracker.", ex);
            }
        }

        void Ipc_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if(e.Message.RequestedMethod == "singleinstancing")
            {
                if (e.Message.Parameters.Length == 1 && e.Message.Parameters[0] == "new_instance")
                {
                    OnNewInstanceCreated(sender, new EventArgs());
                }
                else
                {
                    OnMessageReceived(sender, e);
                }
            }
        }

        public void SendMessageToFirstInstance(object message)
        {
            string[] parameters = { Json.JsonConvert.SerializeObject(message) };
            if(IsFirstInstance)
            {
                OnMessageReceived(IPC, new MessageReceivedEventArgs{Message = new Message{ RequestedMethod = "singleinstancing", Parameters = parameters } });
            }
            else if(!IPC.SendMessage("singleinstancing", parameters))
            {
                throw new SingleInstancingException("Couldn't send message to first instance.");
            }
        }

        /// <summary>
        /// Handles messages received from a new instance of the application.
        /// </summary>
        /// <param name="e">The EventArgs object holding information about the event.</param>
        protected virtual void OnMessageReceived(object sender, MessageReceivedEventArgs e){
            MessageReceived(sender, e);
        }
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        /// <summary>
        /// Handles a creation of a new instance of the application.
        /// </summary>
        /// <param name="e">The EventArgs object holding information about the event.</param>
        protected virtual void OnNewInstanceCreated(object sender, EventArgs e){
            NewInstanceCreated(sender, e);
        }
        public event EventHandler NewInstanceCreated;
        /// <summary>
        /// Gets a value indicating whether this instance of the application is the first instance.
        /// </summary>
        public bool IsFirstInstance
        {
            get
            {
                return IPC.IsServer;
            }
        }
    }
    [Serializable]
    public class SingleInstancingException : Exception
    {
        /// <summary>
        /// Instantiates a new SingleInstancingException object.
        /// </summary>
        public SingleInstancingException() { }

        /// <summary>
        /// Instantiates a new SingleInstancingException object.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SingleInstancingException(string message)
            : base(message) { }

        /// <summary>
        /// Instantiates a new SingleInstancingException object.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SingleInstancingException(string message, Exception inner)
            : base(message, inner) { }

        /// <summary>
        /// Instantiates a new SingleInstancingException object with serialized data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected SingleInstancingException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}