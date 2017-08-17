using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using tracktor.app.Models;

namespace tracktor.app
{
    [Produces("application/json")]
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IAntiforgery _antiForgery;
        private readonly IEmailSender _emailSender;
        private readonly ITracktorService _client;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory,
            IAntiforgery antiForgery,
            IConfiguration config,
            IEmailSender emailSender,
            ITracktorService client)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _antiForgery = antiForgery;
            _config = config;
            _emailSender = emailSender;
            _client = client;
        }

        private async Task<LoginDTO> CreateResponse(ApplicationUser user)
        {
            HttpContext.User = user != null ? await _signInManager.CreateUserPrincipalAsync(user) : null;
            var roles = user != null ? await _userManager.GetRolesAsync(user) : null;
            var afTokenSet = _antiForgery.GetAndStoreTokens(Request.HttpContext);
            return new LoginDTO
            {
                id = user?.Id,
                afToken = afTokenSet.RequestToken,
                afHeader = afTokenSet.HeaderName,
                email = user?.Email,
                roles = roles,
                timeZone = user?.TimeZone
            };
        }

        private static bool IsPopulated(string v)
        {
            return !string.IsNullOrWhiteSpace(v);
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="dto">Required fields: email, password</param>
        /// <returns>LoginDTO</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody]AccountDTO dto)
        {
            if(!IsPopulated(dto?.email) || !IsPopulated(dto?.password))
            {
                return BadRequest();
            }
            if(!EmailHelpers.Validate(dto.Username))
            {
                return BadRequest();
            }

            if(!Boolean.Parse(_config["Tracktor:RegistrationEnabled"]))
            {
                return BadRequest("@RegistrationDisabled");
            }

            if (_config["Tracktor:RegistrationCode"] != dto.code)
            {
                return BadRequest("@BadCode");
            }

            var user = new ApplicationUser { UserName = dto.Username, Email = dto.Username };
            var result = await _userManager.CreateAsync(user, dto.password);
            if (result.Succeeded)
            {
                user = await _userManager.FindByEmailAsync(dto.Username);
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: true);
                _logger.LogInformation(3, $"User {dto.Username} created a new account with a password");

                // create user in tracktor
                user.TUserID = await _client.CreateUserAsync(user.Id);
                user.TimeZone = dto.timezone;
                await _userManager.UpdateAsync(user);

                return Ok(await CreateResponse(user));
            }
            else if(result.Errors != null && result.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                return BadRequest("@UsernameTaken");
            }
            else
            {
                _logger.LogWarning($"Unable to register user {dto.Username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return BadRequest("@UnableToRegister");
        }

        /// <summary>
        /// Creates or logs in a user using external provider
        /// </summary>
        /// <param name="dto">Required fields: provider, code</param>
        /// <returns>LoginDTO</returns>
        [HttpPost("external")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLogin([FromBody]AccountDTO dto)
        {
            if (!IsPopulated(dto?.code) || !IsPopulated(dto?.provider))
            {
                return BadRequest();
            }

            var appVerified = false;
            var emailVerified = false;

            switch (dto.provider)
            {
                case "Facebook":
                    {
                        try
                        {
                            var hc = new HttpClient();
                            var verifyUrl = _config["Facebook:VerifyUrl"];
                            var appId = _config["Facebook:AppId"];
                            var userUrl = _config["Facebook:UserUrl"];
                            if (string.IsNullOrWhiteSpace(verifyUrl) || string.IsNullOrWhiteSpace(userUrl))
                            {
                                return BadRequest("@ProviderNotEnabled");
                            }

                            var appString = await hc.GetStringAsync(verifyUrl + dto.code);
                            var userString = await hc.GetStringAsync(userUrl + dto.code);

                            if (!string.IsNullOrWhiteSpace(appString) && !string.IsNullOrWhiteSpace(userString))
                            {
                                var appResult = Newtonsoft.Json.JsonConvert.DeserializeObject(appString) as JObject;
                                var userResult = Newtonsoft.Json.JsonConvert.DeserializeObject(userString) as JObject;

                                if (userResult["email"] != null)
                                {
                                    dto.email = userResult["email"].ToString();
                                    emailVerified = true;
                                }
                                if (appResult["id"] != null && appResult["id"].ToString() == appId)
                                {
                                    appVerified = true;
                                }
                            }
                        } catch(Exception ex)
                        {
                            _logger.LogError($"Unable to log via {dto.provider}: {ex.Message}");
                            return BadRequest("@UnableToLogin" + dto.provider);
                        }
                    }
                    break;
                default:
                    return BadRequest("@UnknownLoginProvider");
            }

            if(string.IsNullOrWhiteSpace(dto.Username) || !emailVerified || !appVerified)
            {
                return BadRequest("@UnableToLogin" + dto.provider);
            }

            if (!EmailHelpers.Validate(dto.Username))
            {
                return BadRequest();
            }

            // create user if necessary
            var user = await _userManager.FindByEmailAsync(dto.Username);
            if (user == null)
            {
                if (!Boolean.Parse(_config["Tracktor:RegistrationEnabled"]))
                {
                    return BadRequest("@RegistrationDisabled");
                }

                user = new ApplicationUser { Email = dto.Username, UserName = dto.Username };
                await _userManager.CreateAsync(user);
                await _userManager.AddToRoleAsync(user, "User");
            }

            if (await _signInManager.CanSignInAsync(user))
            {
                await _signInManager.SignInAsync(user, true);
                _logger.LogInformation(1, $"User {dto.Username} logged in from {Request.HttpContext.Connection.RemoteIpAddress} via Facebook");
                return Ok(await CreateResponse(user));
            }
            else
            {
                _logger.LogWarning(2, $"Invalid login attempt for user {dto.Username} from {Request.HttpContext.Connection.RemoteIpAddress}.");
                return BadRequest("@UnableToLogin" + dto.provider);
            }
        }

        /// <summary>
        /// Logs in a user using password
        /// </summary>
        /// <param name="dto">Required fields: email, password</param>
        /// <returns>LoginDTO</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]AccountDTO dto)
        {
            if (!IsPopulated(dto?.email) || !IsPopulated(dto?.password))
            {
                return BadRequest();
            }

            var result = await _signInManager.PasswordSignInAsync(dto.Username, dto.password, dto.remember, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(dto.Username);
                _logger.LogInformation(1, $"User {dto.Username} logged in from {Request.HttpContext.Connection.RemoteIpAddress}");
                return Ok(await CreateResponse(user));
            }
            if (result.IsLockedOut || result.IsNotAllowed)
            {
                _logger.LogWarning(2, $"User {dto.Username} is locked out.");
                return BadRequest("@LockedOut");
            }
            else
            {
                _logger.LogWarning(2, $"Invalid login attempt for user {dto.Username} from {Request.HttpContext.Connection.RemoteIpAddress}.");
                return BadRequest("@InvalidAttempt");
            }
        }

        /// <summary>
        /// Logs out current user
        /// </summary>
        /// <returns>LoginDTO</returns>
        [HttpPost("logout")]
        [IgnoreAntiforgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, $"User {userName} logged out");
            return Ok(await CreateResponse(null));
        }

        /// <summary>
        /// Requests a password reset email
        /// </summary>
        /// <param name="dto">Required fields: email</param>
        [HttpPost("forgot")]
        [AllowAnonymous]
        public async Task<IActionResult> Forgot([FromBody]AccountDTO dto)
        {
            if (!IsPopulated(dto?.email))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(dto.Username);
            if (user == null)
            {
                return BadRequest("@UnknownUser");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = UrlHelperExtensions.Action(Url, "Index", "Home", new { reset = dto.Username, code = code }, HttpContext.Request.Scheme);
            _logger.LogInformation(5, $"User {dto.Username} requested a password reset link.");
            var subject = dto.messages != null && dto.messages.Length > 0 && !string.IsNullOrWhiteSpace(dto.messages[0]) ? dto.messages[0] : "Tracktor - password reset";
            var body = dto.messages != null && dto.messages.Length > 1 && !string.IsNullOrWhiteSpace(dto.messages[1]) ? dto.messages[1] : "Please click the link below to reset your password.";
            await _emailSender.SendEmailAsync(dto.Username, subject, body, callbackUrl);

            return Ok();
        }

        /// <summary>
        /// Changes current user's password
        /// </summary>
        /// <param name="dto">Required fields: password, newPassword</param>
        /// <returns>LoginDTO</returns>
        [HttpPost("change")]
        [Authorize]
        public async Task<IActionResult> Change([FromBody]AccountDTO dto)
        {
            if (!IsPopulated(dto?.password) || !IsPopulated(dto?.newPassword))
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // don't reveal anything
                return BadRequest("@UnknownUser");
            }
            var result = await _userManager.ChangePasswordAsync(user, dto.password, dto.newPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, true);
                _logger.LogInformation(6, $"User {user.Email} has successfully changed password.");
                return Ok(await CreateResponse(user));
            }
            return BadRequest("@InvalidChangeAttempt");
        }

        /// <summary>
        /// Resets current user's password using a reset code
        /// </summary>
        /// <param name="dto">Required fields: email, password, code</param>
        /// <returns>LoginDTO</returns>
        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> Reset([FromBody]AccountDTO dto)
        {
            if (!IsPopulated(dto?.email) || !IsPopulated(dto?.password) || !IsPopulated(dto?.code))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(dto.Username);
            if (user == null)
            {
                // don't reveal anything
                return Ok();
            }

            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }
            var result = await _userManager.ResetPasswordAsync(user, dto.code, dto.password);
            _logger.LogInformation(6, $"User {dto.Username} has successfully reset password.");
            await _signInManager.SignInAsync(user, true);
            return Ok(await CreateResponse(user));
        }

        /// <summary>
        /// Deletes current user and all their data
        /// </summary>
        /// <returns>LoginDTO</returns>
        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);

            await _signInManager.SignOutAsync();
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation(6, $"User {user.Email} has been removed.");
            }
            return Ok(await CreateResponse(null));
        }

        /// <summary>
        /// Initiate a new session
        /// </summary>
        /// <returns>LoginDTO</returns>
        [HttpGet("handshake")]
        [AllowAnonymous]
        public async Task<IActionResult> Handshake()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var user = await _userManager.GetUserAsync(User);
                return Ok(await CreateResponse(user));
            }
            return Ok(await CreateResponse(null));
        }
    }
}