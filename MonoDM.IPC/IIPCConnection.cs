using System;
namespace MonoDM.IPC
{
    public abstract class BaseIPCConnection : IIPCSide, IDisposable
    {
        public virtual bool IsServer { get; set; }
        public virtual bool HasAnswerCapability { get; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        /// <summary>
        /// Raised when a message comes to app.
        /// </summary>
        /// <param name="sender">Sender, inherited from IIPCSide.</param>
        /// <param name="e">E.</param>
        protected virtual void OnMessageReceived(object sender, MessageReceivedEventArgs e){
            MessageReceived(sender, e);
        }
        public abstract bool SendMessage(string method, string[] parameters);
        public abstract bool SendMessage(Message msg);
        public virtual void Dispose() { }
    }
    public interface IIPCSide
    {
        bool HasAnswerCapability { get; }
    }
    public interface IIPCBehavior : IIPCSide
    {
        bool Answer(object msg);
        bool Answer(string msg);
    }
    [Serializable]
    public class Message
    {
        public string RequestedMethod { get; set; }
        public string[] Parameters { get; set; }
    }
    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}
