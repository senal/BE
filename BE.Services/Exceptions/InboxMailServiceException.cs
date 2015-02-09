using System;

namespace BE.Services.Exceptions
{
    public class InboxMailServiceException:Exception
    {
        public InboxMailServiceException(string message)
            :base(message)
        {
            
        }
    }
}
