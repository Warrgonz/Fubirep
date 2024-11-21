namespace fubi_api.Utils.Smtp
{
    public interface IMessage
    {
        void SendEmail(string subject, string body, string to);
    }
}
