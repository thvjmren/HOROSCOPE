﻿namespace HoroScope.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(string emailTo, string subject, string body, bool isHtml = true);
    }
}
