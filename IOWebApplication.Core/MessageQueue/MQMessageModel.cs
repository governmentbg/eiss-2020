namespace IOWebApplication.Core.MessageQueue
{
    public class MQMessageModel
    {
        public string ClientId { get; set; }

        public string Method { get; set; }

        public string Params { get; set; }
    }
}
