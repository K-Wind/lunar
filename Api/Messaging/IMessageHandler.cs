namespace Api
{
    public interface IMessageHandler
    {
        public void Send(string message, IBasicProperties? props = null);
        public Task<string> SendAndReceive(string message);
    }
}
