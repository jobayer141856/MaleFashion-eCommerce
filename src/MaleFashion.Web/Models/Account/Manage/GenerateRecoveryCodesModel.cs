using Microsoft.AspNetCore.Mvc;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class GenerateRecoveryCodesModel
    {
        public string[]? RecoveryCodes { get; set; }

        public string? StatusMessage { get; set; }
    }
}
