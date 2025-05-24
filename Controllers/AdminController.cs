using EMS.Entities.DTO;
using EMS.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using System.Web;
using EMS.Utils;
using Amazon;
using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;




namespace EMS.Controllers
{
    [Route("admins/")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly string _jwtSecretKey = "my_secret_for_jwt_token_101010RandomKey1100";
        private readonly int _jwtExpirationMinutes = 60; // 60 minutes

        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Admin>> AddAdmin(AdminDto adminDto)
        {
            try
            {
                var existingAdmin = await _context.Admins.FirstOrDefaultAsync(a => a.email == adminDto.email);
                if (existingAdmin != null)
                {
                    // 409 Conflict if the email already exists
                    return Conflict("An account with this email already exists.");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminDto.password);
                var adminEntity = new Admin()
                {
                    name = adminDto.name,
                    email = adminDto.email,
                    password = hashedPassword,
                    mobileNumber = adminDto.mobileNumber
                };

                await _context.Admins.AddAsync(adminEntity);
                await _context.SaveChangesAsync();

                // Send confirmation email
                var emailService = new EmailService();
                await emailService.SendEmailAsync(adminDto.email, adminDto.name);

                return Created("", new { adminEntity.name });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }



        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {

            try
            {
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.email == loginDto.email);

                if (admin == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                if (admin.isEmailVerified is false)
                {
                    return StatusCode(403, new { message = "Email not verified. Please verify your email before logging in." });

                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.password, admin.password);

                if (!isPasswordValid)
                {
                    return Unauthorized("Invalid email or password.");
                }

                var token = GenerateJwtToken(admin);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(24)
                };

                Response.Cookies.Append("authToken", token, cookieOptions);
                Response.Cookies.Append("name", admin.name, cookieOptions);
                Response.Cookies.Append("type", "login", cookieOptions);

                return Ok(new { name = admin.name });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }

        }

        private string GenerateJwtToken(Admin admin)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.id.ToString()),
                new Claim(ClaimTypes.Name, admin.name),
                new Claim(ClaimTypes.Email,admin.email)
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



        [HttpGet("auth/verify-token")]
        public IActionResult VerifyToken([FromHeader(Name = "Authorization")] string authHeader)
        {
            try
            {
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Access Denied. No token provided." });
                }

                var token = authHeader.Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;


                return Ok(new
                {
                    message = "Token is valid"
                });
            }
            catch (Exception)
            {
                return StatusCode(403, new { message = "Invalid or expired token." });
            }
        }


        [HttpPost]
        [Route("/auth/google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            Request.EnableBuffering();

            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
            }

            // Extract the credential value
            var parsedQuery = HttpUtility.ParseQueryString(body);
            string credential = parsedQuery["credential"];


            if (string.IsNullOrEmpty(credential))
            {
                return BadRequest(new { error = "Missing credential parameter." });
            }

            try
            {
                // Validate the Google ID token using with provided client ID
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    credential,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new List<string> { "770208398633-q171sv78ao9e8ubf7pj2m5lemii3kvfo.apps.googleusercontent.com" }
                    }
                );

                string email = payload.Email;
                string name = payload.Name;

                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.email == email);
                if (admin == null)
                {
                    var adminEntity = new Admin()
                    {
                        name = name,
                        email = email,
                        password = email,
                        isEmailVerified = true,
                        mobileNumber = "0712345678"

                    };

                    await _context.Admins.AddAsync(adminEntity);
                    await _context.SaveChangesAsync();
                    admin = adminEntity;
                }

                var token = GenerateJwtToken(admin);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(24)
                };

                Response.Cookies.Append("authToken", token, cookieOptions);
                Response.Cookies.Append("name", admin.name, cookieOptions);
                Response.Cookies.Append("type", "sso", cookieOptions);

                return Redirect("http://localhost:4200/users");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Authentication failed", details = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string emailToken)
        {
            try
            {
                if (emailToken == null)
                {
                    return Unauthorized(new { message = "Access Denied. No email token provided." });
                }


                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(emailToken, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                Console.WriteLine(jwtToken);

                if (Email == null)
                {
                    return BadRequest(new { message = "Invalid token" });
                }

                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.email == Email);

                if (admin == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                admin.isEmailVerified = true;
                await _context.SaveChangesAsync();

                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "<html><head><title>Email Verified</title></head><body><h2>Email verified successfully!</h2><p>You can now <a href='http://localhost:4200/login'>log in</a>.</p></body></html>",
                    StatusCode = 200
                };

            }
            catch (SecurityTokenException)
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "<html><head><title>Unauthorized</title></head><body><h2>Invalid or Expired Token</h2><p>Please request a new verification link.</p></body></html>",
                    StatusCode = 401
                };
            }
            catch (Exception ex)
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = $"<html><head><title>Error</title></head><body><h2>An error occurred</h2><p>{ex.Message}</p></body></html>",
                    StatusCode = 500
                };
            }
        }


        [HttpPost("send-otp")]
        public async Task<ActionResult> SendOtpToPhone(int id, string phoneNumber)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);

                if (admin is null)
                {
                    return NotFound("Admin Not Found");
                }


                if (string.IsNullOrEmpty(phoneNumber))
                {
                    return BadRequest(new { message = "Phone number is required." });
                }


                string accountSid = _configuration["Twilio:AccountSid"]!;
                string authToken = _configuration["Twilio:AuthToken"]!;
                string twilioNumber = _configuration["Twilio:TwilioNumber"]!;

                TwilioClient.Init(accountSid, authToken);

                var otp = new Random().Next(100000, 999999).ToString();

                var messageBody = $"Hello From Employee Management System. Your OTP code is: {otp}.";

                var message = await MessageResource.CreateAsync(
                    body: messageBody,
                    from: new PhoneNumber(twilioNumber),
                    to: new PhoneNumber(phoneNumber)
                );

                admin.otp = otp;

                await _context.SaveChangesAsync();

                return Ok(new { message = "OTP sent successfully", otp = otp, messageSid = message.Sid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "    ", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SendEmployeeDto>> GetEmployee(int id)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(e => e.id == id);

            if (admin is null)
            {
                return NotFound("Admin Not Found");
            }

            var adminDto = new SendAdminDto
            {
                id = admin.id,
                name = admin.name,
                email = admin.email,
                mobileNumber = admin.mobileNumber,
                isMobileVerified = admin.isMobileVerified

            };


            return Ok(adminDto);
        }


        [HttpPut("verify-otp")]
        public async Task<IActionResult> VerifyOtp(int id, string otp)
        {
            if (string.IsNullOrEmpty(otp))
            {
                return BadRequest(new { message = "OTP is required." });
            }

            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound(new { message = "Admin not found." });
            }

            if (admin.otp != otp)
            {
                return BadRequest(new { message = "OTP does not match." });
            }

            admin.otp = null;
            admin.isMobileVerified = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "OTP verified successfully." });
        }



    }
}
