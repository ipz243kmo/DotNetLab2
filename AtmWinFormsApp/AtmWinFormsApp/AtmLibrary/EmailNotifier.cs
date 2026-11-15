using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace AtmLibrary
{
    public class EmailNotifier
    {
        private readonly string _senderEmail;
        private readonly string _password;
        private readonly string _recipientEmail;

        public EmailNotifier(string senderEmail, string password, string recipientEmail)
        {
            _senderEmail = senderEmail;
            _password = password;
            _recipientEmail = recipientEmail;
        }

        public void SendEmailNotification(object sender, AtmEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_senderEmail) || string.IsNullOrWhiteSpace(_password))
                return;

            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_senderEmail, _password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                smtpClient.Send(
                    _senderEmail,
                    _recipientEmail,
                    "Сповіщення від банкомату",
                    string.Format("Час операції: {0:dd.MM.yyyy HH:mm:ss}\nДеталі: {1}", e.Timestamp, e.Message)
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Помилка відправки Email: " + ex.Message);
            }
        }
    }
}