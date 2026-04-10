using EmployeePortal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace EmployeePortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly DbHelper _db;
        private readonly IConfiguration _config;

        public AccountController(DbHelper db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ==========================================
        // 1. LOGIN
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AdminName") != null)
            {
                return RedirectToAction("Index", "Employee");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            using var conn = _db.GetConnection();
            string sql = "SELECT FullName FROM Admins WHERE Username = @User AND Password = @Pass AND IsActive = 1";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@User", Username);
            cmd.Parameters.AddWithValue("@Pass", Password);

            conn.Open();
            var adminName = cmd.ExecuteScalar();

            if (adminName != null)
            {
                HttpContext.Session.SetString("AdminName", adminName.ToString()!);
                return RedirectToAction("Index", "Employee");
            }

            ViewBag.Error = "Identity verification failed. Please check your credentials.";
            return View();
        }

        // ==========================================
        // 2. FORGOT PASSWORD (EMAIL LOGIC)
        // ==========================================
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            using var conn = _db.GetConnection();
            string findUserSql = "SELECT AdminId FROM Admins WHERE Email = @Email AND IsActive = 1";
            using var cmd = new SqlCommand(findUserSql, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            conn.Open();

            var adminId = cmd.ExecuteScalar();

            if (adminId != null)
            {
                string token = Guid.NewGuid().ToString();
                DateTime expiry = DateTime.Now.AddMinutes(15);

                // Save Token to Database
                string saveTokenSql = "INSERT INTO PasswordResets (AdminId, Token, ExpiryTime) VALUES (@AId, @Tok, @Exp)";
                using var cmdSave = new SqlCommand(saveTokenSql, conn);
                cmdSave.Parameters.AddWithValue("@AId", adminId);
                cmdSave.Parameters.AddWithValue("@Tok", token);
                cmdSave.Parameters.AddWithValue("@Exp", expiry);
                cmdSave.ExecuteNonQuery();

                // Generate Link
                string resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme)!;

                // Send Email via Mailtrap
                try
                {
                    var smtpServer = _config["EmailSettings:SmtpServer"];
                    var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]!);
                    var smtpUser = _config["EmailSettings:SmtpUser"];
                    var smtpPass = _config["EmailSettings:SmtpPass"];
                    var fromEmail = _config["EmailSettings:FromEmail"];

                    using (var client = new SmtpClient(smtpServer, smtpPort))
                    {
                        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                        client.EnableSsl = true;

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(fromEmail!, "ERMS Security"),
                            Subject = "AUTHENTICATION PROTOCOL: Password Reset Request",
                            IsBodyHtml = true,
                            Body = $@"
                                <div style='font-family: sans-serif; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px; max-width: 600px;'>
                                    <h2 style='color: #0f172a;'>Identity Recovery Initialized</h2>
                                    <p>A secure request has been made to reset your administrative credentials.</p>
                                    <p>Click the button below within the next 15 minutes to establish a new passcode:</p>
                                    <div style='text-align: center; margin: 30px 0;'>
                                        <a href='{resetLink}' style='background: #0f172a; color: white; padding: 12px 25px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Authorize Reset</a>
                                    </div>
                                    <p style='font-size: 11px; color: #64748b;'>Node ID: {Environment.MachineName.ToUpper()}</p>
                                </div>"
                        };
                        mailMessage.To.Add(email);
                        client.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Mail Error: " + ex.Message);
                }
            }

            ViewBag.Message = "If an account matches that email, a secure recovery protocol has been initiated.";
            return View();
        }

        // ==========================================
        // 3. RESET PASSWORD (FINAL STEP)
        // ==========================================
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            using var conn = _db.GetConnection();
            string sql = "SELECT AdminId FROM PasswordResets WHERE Token = @Tok AND IsUsed = 0 AND ExpiryTime > GETDATE()";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Tok", token);
            conn.Open();

            var adminId = cmd.ExecuteScalar();
            if (adminId == null) return Content("Access Denied: The security token is either invalid or has expired.");

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string newPassword)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                string findSql = "SELECT AdminId FROM PasswordResets WHERE Token = @Tok";
                var cmdFind = new SqlCommand(findSql, conn, trans);
                cmdFind.Parameters.AddWithValue("@Tok", token);
                int adminId = (int)cmdFind.ExecuteScalar();

                string updateSql = "UPDATE Admins SET Password = @Pass WHERE AdminId = @Id";
                var cmdUpdate = new SqlCommand(updateSql, conn, trans);
                cmdUpdate.Parameters.AddWithValue("@Pass", newPassword);
                cmdUpdate.Parameters.AddWithValue("@Id", adminId);
                cmdUpdate.ExecuteNonQuery();

                string markUsedSql = "UPDATE PasswordResets SET IsUsed = 1 WHERE Token = @Tok";
                var cmdMark = new SqlCommand(markUsedSql, conn, trans);
                cmdMark.Parameters.AddWithValue("@Tok", token);
                cmdMark.ExecuteNonQuery();

                trans.Commit();
                return RedirectToAction("Login");
            }
            catch
            {
                trans.Rollback();
                return View();
            }
        }

        // ==========================================
        // 4. LOGOUT
        // ==========================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}