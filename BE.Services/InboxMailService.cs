using System;
using BE.Services.Exceptions;

namespace BE.Services
{
    public class InboxMailService
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IInboxMailDbManager _inboxMailDbManager;
        private readonly IMailManager _mailManager;

        public InboxMailService(IConfigurationManager configurationManager, 
            IInboxMailDbManager inboxMailDbManager,
            IMailManager mailManager)
        {
            if(configurationManager==null)
                throw new ArgumentNullException("configurationManager");
            if (inboxMailDbManager == null)
                throw new ArgumentNullException("inboxMailDbManager");
            if(mailManager==null)
                throw new ArgumentNullException("mailManager");

            _configurationManager = configurationManager;
            _inboxMailDbManager = inboxMailDbManager;
            _mailManager = mailManager;
        }
        public void RefreshInbox()
        {
            //configured to refresh the mailbox
            var refreshMail = _configurationManager.Read<bool>("InboxRefresh");

            if (!refreshMail)
                return;

            //read mail settings
            var mailBox = _configurationManager.Read<string>("EmailInbox");
            var mailPassword = _configurationManager.Read<string>("EmailPassword");
            var mailServer = _configurationManager.Read<string>("EmailServer");

            if (string.IsNullOrWhiteSpace(mailBox) || 
                string.IsNullOrWhiteSpace(mailPassword) ||
                string.IsNullOrWhiteSpace(mailServer))
                throw new InboxMailServiceException("Unable to connect to Email Server.  Please check your connection settings.");

            //delete and create existing tblEmailInbox 
            _inboxMailDbManager.DeleteAndCreateInboxTable();

            var connected = _mailManager.Connect(server: mailServer, port: 110, userName: mailBox,
                password: mailPassword);

            if(!connected)
                throw new InboxMailServiceException("Unable to connect to Email Server.  Please check and verify connection settings.");

            var mailList = _mailManager.GetMails();

            foreach (IMailEntry mailEntry in mailList)
            {
                var mail = _mailManager.GetMessage(mailEntry.Id);
            }

        }

    }
}
