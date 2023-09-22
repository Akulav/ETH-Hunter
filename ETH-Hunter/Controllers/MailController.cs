using System.Net;
using System.Net.Mail;

namespace ETH_Generator.Controllers
{
    public static class MailController
    {
        public static void sendMail(string body, string id)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, // Port number for the SMTP server (587 for TLS, 465 for SSL, 25 for non-secure)
                Credentials = new NetworkCredential("eth.hunter.miner@gmail.com", "nbowhnrfngcdwhmv"),
                EnableSsl = true, // Use SSL/TLS to secure the connection (true for most modern SMTP servers)
            };

            // Create a new email message
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress("eth.hunter.miner@gmail.com"),
                Subject = "Health Status ID: " + id,
                Body = body,
            };

            // Add recipients (you can add multiple recipients)
            mailMessage.To.Add("catalin0505229@gmail.com");
            mailMessage.To.Add("mpetrusenco@gmail.com");

            try
            {
                // Send the email
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}
