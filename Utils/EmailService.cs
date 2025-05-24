using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EMS.Utils
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUsername = "isuruijs@gmail.com"; // Replace with your email
        private readonly string _smtpPassword = "vahglsggdpidutpb"; // Use App Password
        private readonly string _jwtSecretKey = "my_secret_for_jwt_token_101010RandomKey1100";
        private readonly int _jwtExpirationMinutes = 3600; // Token valid for 60 minutes

        public async Task SendEmailAsync(string toEmail, string toName)
        {

            string verificationToken = GenerateVerificationToken(toEmail);
            string verificationLink = $"http://localhost:5263/admins/verify-email?emailToken={verificationToken}";

            string bodyData = $@"
                <h3>Hello {toName},</h3>
                <p>Thank you for registering! Please verify your email by clicking the link below:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>If you didn’t request this, you can safely ignore this email.</p>
                <br>
                <p>Best Regards,</p>
                <p>Employee Management System</p>";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EMS", _smtpUsername));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "Email Verificatoin";
            message.Body = new TextPart("html") { Text = bodyData };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        private string GenerateVerificationToken(string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }
}
