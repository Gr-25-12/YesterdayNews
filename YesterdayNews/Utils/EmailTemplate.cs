using System.Text.Encodings.Web;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Utils
{
    public static class EmailTemplate
    {
        public static string GetAdminCreatedAccountEmail(string userName, string generatedPassword, string redirectUrl)
        {
            return $@"
                                    <!DOCTYPE html>
                                    <html>
                                    <head>
                                        <style>
                                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #3A2512; max-width: 600px; margin: 0 auto; padding: 20px; }}
                                            .header {{ background-color: #3A2512; padding: 20px; text-align: center; }}
                                            .header img {{ max-height: 100px; }}
                                            .content {{ padding: 30px; background-color: #f9f9f9; }}
                                            .password-box {{ background-color: #fff; border: 2px dashed #3A2512; padding: 15px; text-align: center; font-size: 18px; margin: 20px 0; font-weight: bold; }}
                                            .button {{ background-color: #3A2512; color: white !important; padding: 12px 25px; text-decoration: none; border-radius: 4px; display: inline-block; margin: 15px 0; }}
                                            .footer {{ margin-top: 30px; font-size: 12px; color: #777; text-align: center; }}
                                        </style>
                                    </head>
                                    <body>
                                        <div class='header'>
                                            <img  src='https://yesterdaystoragegr12.blob.core.windows.net/notarticles/ResizedLogo.jpg' alt='Yesterday News Logo'>
                                        </div>
                                        <div class='content'>
                                            <h6> <strong> Psssst! </strong> Your Account Is Ready! 🤝</h6>
                                            <p>Dear {userName},</p>
                                            <p>Your administrator has created an account for you on <strong>Yesterday News</strong>.</p>
        
                                            <div class='password-box'>
                                                Your Password:<br>
                                                <span style='font-size: 24px; letter-spacing: 2px;'>{generatedPassword}</span>
                                            </div>
        
                                            <p style='color: #d32f2f;'><strong>Important:</strong> We reccomend you change this password after your first login.👉<em><strong>Otherwise don't blame us if your account hacked!</strong></em></p>
        
                                            <p>Click below to activate your account:</p>
                                            <a href='{redirectUrl}' class='button'>Activate Account</a>
        
                                            <p>If the button doesn't work, copy and paste this URL into your browser:<br>
                                            <small>{redirectUrl}</small></p>
                                        </div>
                                        <div class='footer'>
                                            <p>© {DateTime.Now.Year} Yesterday News. All rights reserved.</p>
                                            <p>We bring you the news that you already know... but better! 📰</p>
                                            <p>If you didn't request this account, please contact support if you found them 😁.</p>
                                        </div>
                                    </body>
                                    </html>";
        }

        public static string GetConfirmationEmail(string userName, string redirectUrl)
        {
            return @$"<!DOCTYPE html>
                    <html> 
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #3A2512; max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #3A2512; padding: 20px; text-align: center; }}
                            .header img {{ max-height: 100px; }}
                            .content {{ padding: 30px; background-color: #f9f9f9; }}
                            .welcome-text {{ font-size: 18px; margin-bottom: 25px; color: #3A2512; }}
                            .button {{ background-color: #3A2512; color: white !important; padding: 14px 30px; text-decoration: none; border-radius: 6px; display: inline-block; margin: 20px 0; font-weight: 600; font-size: 16px; }}
                            .button:hover {{ background-color: #2a1c0d; }}
                            .security-note {{ background-color: #fff4f4; border-left: 4px solid #d32f2f; padding: 15px; margin: 20px 0; }}
                            .footer {{ margin-top: 30px; font-size: 12px; color: #777; text-align: center; }}
                            .divider {{ border-top: 2px solid #b39086; margin: 25px 0; opacity: 0.5; }}
                        </style>
                    </head>
                    <body>
                        <div class='header'>
                            <img src='https://yesterdaystoragegr12.blob.core.windows.net/notarticles/ResizedLogo.jpg' alt='Yesterday News Logo'>
                        </div>
    
                        <div class='content'>
                            <h4 style='color: #3A2512; margin-bottom: 10px;'>Welcome to Yesterday News! 🎉</h4>
        
                            <div class='welcome-text'>
                                <p>Hi <strong>{userName}</strong>,</p>
                                <p>Thank you for joining our community of news enthusiasts! We're excited to have you on board.</p>
                            </div>

                            <div class='divider'></div>

                            <p style='margin-bottom: 20px;'>To complete your registration and start exploring the latest news (Only after you pay 😅), please confirm your email address:</p>
        
                            <div style='text-align: center;'>
                                <a href='{redirectUrl}' class='button'>Confirm Email Address</a>
                            </div>

                            <div class='security-note'>
                                <p style='margin: 0; color: #d32f2f;'><strong>🔒 Security Tip:</strong> We're not sure if this link has expiry.</p>
                            </div>

                            <p style='font-size: 14px; color: #666;'>
                                If the button doesn't work, copy and paste this URL into your browser:<br>
                                <code style='background: #f0f0f0; padding: 8px; border-radius: 4px; word-break: break-all;'>{redirectUrl}</code>
                            </p>

                            <div class='divider'></div>

                            <p style='color: #666; font-size: 14px;'>
                                <strong>What's next?</strong><br>
                                After confirmation, you'll have access to our webiste, but you can't read any article unless you 💵 a little.
                            </p>
                        </div>
    
                        <div class='footer'>
                            <p>© {DateTime.Now.Year} Yesterday News. All rights reserved.</p>
                            <p>We bring you the news that you already know... but better! 📰</p>
                            <p style='font-size: 11px; color: #999; margin-top: 10px;'>
                                If you didn't create this account, please ignore this email or contact our support team if you can find them 😁.
                            </p>
                        </div>
                    </body>
                    </html>";
        }

        public static string GetConfirmationSubscriptionEmail(string userName, string planName, decimal amount, string transactionId,string WebsiteUrl)
        {
            return @$"
<!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #3A2512; max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #3A2512; padding: 20px; text-align: center; }}
                    .header img {{ max-height: 100px; }}
                    .content {{ padding: 30px; background-color: #f9f9f9; }}
                     .button {{ background-color: #3A2512; color: white !important; padding: 14px 30px; text-decoration: none; border-radius: 6px; display: inline-block; margin: 20px 0; font-weight: 600; font-size: 16px; }}
                    .receipt-box {{ background-color: #e9f7ef; border-left: 4px solid #4caf50; padding: 15px; margin: 20px 0; }}
                    .footer {{ margin-top: 30px; font-size: 12px; color: #777; text-align: center; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <img src='https://yesterdaystoragegr12.blob.core.windows.net/notarticles/ResizedLogo.jpg' alt='Yesterday News Logo'>
                </div>

                <div class='content'>
                    <h2 style='color: #3A2512;'>Payment Receipt</h2>
                    <p>Hi <strong>{userName}</strong>,</p>
                    <p>Thank you for subscribing to <strong>{planName}</strong> at Yesterday News!</p>

                    <div class='receipt-box'>
                        <p style='margin: 0;'><strong>✅ Payment Confirmed</strong></p>
                        <p style='margin: 5px 0 0 0;'>Amount: <strong>{amount:C} SEK</strong></p>
                        <p style='margin: 0;'>Transaction ID: <code>{transactionId}</code></p>
                    </div>

                            <div style='text-align: center;'>
                                <a href='{WebsiteUrl}' class='button'>Take me to the news 😎</a>
                            </div>

                    <p>Your subscription is now active and you can start enjoying our premium content.</p>
                    <p>If you have any questions, feel free to reach out to our support team (if you can find them 😁).</p>
                </div>

                <div class='footer'>
                    <p>© {DateTime.Now.Year} Yesterday News. All rights reserved.</p>
                    <p>We bring you the news that you already know! 📰</p>
                </div>
            </body>
            </html>";
        }


    }
}
