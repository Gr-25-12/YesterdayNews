// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using YesterdayNews.Models.Db;
using YesterdayNews.Utils;

namespace YesterdayNews.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string FirstName { get; set; }
            [Required]
            public string LastName { get; set; }

            public DateOnly? DateOfBirth { get; set; }

            public string? Role { get; set; }
            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(StaticConsts.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticConsts.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticConsts.Role_Journalist)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticConsts.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticConsts.Role_Editor)).GetAwaiter().GetResult();
            }
            

            Input = new()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.DateOfBirth = Input.DateOfBirth;
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;

                IdentityResult result;

                if (User.IsInRole(StaticConsts.Role_Admin))
                {
                    // Admin-created account flow
                    var generatedPassword = GenerateRandomPassword();
                    result = await _userManager.CreateAsync(user, generatedPassword);

                    if (result.Succeeded)
                    {
                       
                        // Admin must selet a role 
                        await _userManager.AddToRoleAsync(user, Input.Role ?? StaticConsts.Role_Customer);

                        // Send admin-created account email (with password)
                        var emailBody = $@"
                                    <!DOCTYPE html>
                                    <html>
                                    <head>
                                        <style>
                                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                                            .header {{ background-color: #3A2512; padding: 20px; text-align: center; }}
                                            .header img {{ max-height: 50px; }}
                                            .content {{ padding: 30px; background-color: #f9f9f9; }}
                                            .password-box {{ background-color: #fff; border: 2px dashed #3A2512; padding: 15px; text-align: center; font-size: 18px; margin: 20px 0; font-weight: bold; }}
                                            .button {{ background-color: #3A2512; color: white !important; padding: 12px 25px; text-decoration: none; border-radius: 4px; display: inline-block; margin: 15px 0; }}
                                            .footer {{ margin-top: 30px; font-size: 12px; color: #777; text-align: center; }}
                                        </style>
                                    </head>
                                    <body>
                                        <div class='header'>
                                            <img src='https://yourwebsite.com/logo.png' alt='Yesterday News Logo'>
                                        </div>
                                        <div class='content'>
                                            <h2>Your Account Is Ready!</h2>
                                            <p>Dear {user.UserName},</p>
                                            <p>Your administrator has created an account for you on <strong>Yesterday News</strong>.</p>
        
                                            <div class='password-box'>
                                                One-Time Password:<br>
                                                <span style='font-size: 24px; letter-spacing: 2px;'>{generatedPassword}</span>
                                            </div>
        
                                            <p style='color: #d32f2f;'><strong>Important:</strong> Please change this password after your first login.</p>
        
                                            <p>Click below to activate your account:</p>
                                            <a href='{HtmlEncoder.Default.Encode(returnUrl)}' class='button'>Activate Account</a>
        
                                            <p>If the button doesn't work, copy and paste this URL into your browser:<br>
                                            <small>{returnUrl}</small></p>
                                        </div>
                                        <div class='footer'>
                                            <p>© {DateTime.Now.Year} Yesterday News. All rights reserved.</p>
                                            <p>If you didn't request this account, please contact support.</p>
                                        </div>
                                    </body>
                                    </html>";
                        await _emailSender.SendEmailAsync(
                            Input.Email,
                            "Your Yesterday News Account is Ready",
                            emailBody
                        );

                        TempData["success"] = "New user created successfully!";
                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    // Normal flow
                    result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");
                        await _userManager.AddToRoleAsync(user, StaticConsts.Role_Customer);

                        
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }

        private string GenerateRandomPassword()
        {
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specials = "!@#$%^&*_-=+";

            var passwordChars = new List<char>();
            var random = RandomNumberGenerator.Create();

            // Ensure at least one of each required type
            passwordChars.Add(GetRandomChar(letters.ToUpper(), random));
            passwordChars.Add(GetRandomChar(letters, random));
            passwordChars.Add(GetRandomChar(digits, random));
            passwordChars.Add(GetRandomChar(specials, random));

            // Fill remaining length
            var allChars = letters + letters.ToUpper() + digits + specials;
            for (int i = passwordChars.Count; i < 12; i++) // Default 12 chars
            {
                passwordChars.Add(GetRandomChar(allChars, random));
            }

            // Shuffle
            for (int i = 0; i < passwordChars.Count; i++)
            {
                int j = RandomNumberGenerator.GetInt32(i, passwordChars.Count);
                (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
            }

            return new string(passwordChars.ToArray());
        }

        private char GetRandomChar(string chars, RandomNumberGenerator random)
        {
            return chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
    }
}
