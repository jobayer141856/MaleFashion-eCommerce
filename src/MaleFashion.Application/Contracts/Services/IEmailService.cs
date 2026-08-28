using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Contracts.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string receiverName, string reciverEmail, string subject, string body);
    }
}
