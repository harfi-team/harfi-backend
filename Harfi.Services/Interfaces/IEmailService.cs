using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Services.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string toEmail, string userName, string code);
}