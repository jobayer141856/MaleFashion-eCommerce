using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Contracts.Services
{
    public class RecaptchaVerifyResponse
    {
        public bool Success { get; set; }
        public decimal Score { get; set; }
        public string? Action { get; set; }
        public DateTime? ChallengeTs { get; set; }
        public string? Hostname { get; set; }
        public IReadOnlyList<string>? ErrorCodes { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IRecaptchaService
    {
        Task<RecaptchaVerifyResponse> VerifyTokenAsync(
            string? token, 
            string? expectedAction = null, 
            CancellationToken cancellationToken = default);
    }
}

