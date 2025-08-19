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
                                            .header img {{ max-height: 50px; }}
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
                                            <p>If you didn't request this account, please contact support if you found them 😁.</p>
                                        </div>
                                    </body>
                                    </html>";
        }
    }
}
