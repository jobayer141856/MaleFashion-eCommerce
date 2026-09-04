using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class TwoFactorAuthenticationModel
    {
       
        public bool HasAuthenticator { get; set; }
      
        public int RecoveryCodesLeft { get; set; }

        public bool Is2faEnabled { get; set; }

        public bool IsMachineRemembered { get; set; }

        public string? StatusMessage { get; set; }
    }
}

