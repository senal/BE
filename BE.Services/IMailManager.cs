using System.Collections.Generic;

namespace BE.Services
{
    public interface IMailManager
    {
        bool Connect(string server, long port, string userName, string password);
        IList<IMailEntry> GetMails();
        IMail GetMessage(long id);
    }
}
