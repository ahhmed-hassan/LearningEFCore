using DependencyInjection.Application.Intrefaces;
using DependencyInjection.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.Application;

internal class RegisterService (SmtpClient _smtpClient)

{
    private readonly SmtpClient _smtpClient = _smtpClient;

    public async void RegisterUser(
        string username,
        string email,
        Domain.SubScriptionTier subscriptionTier, 
        string? referralCode)
    {

        if (!string.IsNullOrEmpty(referralCode))
        {
            var referrer = await _context.Users
                .FirstOrDefaultAsync(u => u.ReferralCode == referralCode);

            if (referrer != null)
            {
                referrer.ReferralCredits += subscriptionTier == Domain.SubScriptionTier.Premium ? 20 : 10;

                // Can't test this without sending real emails!
                var smtpClient = new SmtpClient("smtp.gmail.com");

                smtpClient.Send(
                    "our_System@example.com",
                    referrer.Email,
                    "You earned credits!",
                    $"Your friend {username} just signed up using your code!"
                );
            }

            
        }

        if (subscriptionTier == Domain.SubScriptionTier.Premium)
        {
            _smtpClient.Send(
                from: "our_system@example.com",
                recipients: email,
                subject: "Welcome to Premium!",
                body: $"Hi {username}, enjoy your premium features!");
        }
        else
        {
            _smtpClient.Send(
                from: "our_system@example.com",
                recipients: email,
                subject: "Welcome!",
                body: $"Hi {username}, thanks for joining!");
        }
        // Usually would save user to DB here
    }
}
