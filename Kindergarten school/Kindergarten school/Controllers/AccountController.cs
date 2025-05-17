using Kindergarten_school.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace Kindergarten_school.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMongoCollection<Parent> _parentsCollection;
        private readonly IMongoCollection<TeacherModel> teachersCollection;
        private readonly IMongoCollection<AccountModel> _accountsCollection;

        public AccountController(IMongoDatabase database)
        {
            _parentsCollection = database.GetCollection<Parent>("parents");
            teachersCollection = database.GetCollection<TeacherModel>("teachers");
            _accountsCollection = database.GetCollection<AccountModel>("accounts");
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string phone, string password)
        {
            // Kiểm tra trùng phone
            var existingParent = await _parentsCollection.Find(p => p.Phone == phone).FirstOrDefaultAsync();
            if (existingParent != null)
            {
                ViewBag.Error = "Số điện thoại đã được sử dụng.";
                return View();
            }

            // Mã hóa mật khẩu
            //var passwordHash = HashPassword(password);

            // Tạo ParentID mới
            string parentId = await GenerateParentID();

            // Tạo đối tượng Account
            var account = new AccountModel
            {
                Username = phone,
                PasswordHash = password,
                Role = "parent",
                AccountID = parentId // Liên kết với bảng Parents
            };

            // Tạo đối tượng Parent
            var parent = new Parent
            {
                ParentID = parentId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                PasswordHash = password,
                Roles = "parent"
            };

            // Lưu vào MongoDB
            await _accountsCollection.InsertOneAsync(account);
            await _parentsCollection.InsertOneAsync(parent);

            // Tự động đăng nhập
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, parent.FirstName),
        new Claim("ParentID", parent.ParentID),
        new Claim(ClaimTypes.Role, parent.Roles)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string password, string phone, string? returnUrl)
        {
            // Tìm account theo số điện thoại
            var account = await _accountsCollection.Find(a => a.Phone == phone).FirstOrDefaultAsync();

            if (account != null && account.PasswordHash == password) // Nếu dùng hash, nên so sánh bằng mã hóa
            {
                // Kiểm tra role để lấy thông tin người dùng
                string userName = "";
                string userId = "";

                if (account.Role == "parent")
                {
                    var parent = await _parentsCollection.Find(p => p.AccountID == account.AccountID).FirstOrDefaultAsync();
                    if (parent == null)
                    {
                        ViewBag.Error = "Thông tin phụ huynh không tồn tại.";
                        return View();
                    }

                    userName = parent.FirstName;
                    userId = parent.AccountID;
                }
                else if (account.Role == "teacher")
                {
                    var teacher = await teachersCollection.Find(t => t.AccountID == account.AccountID).FirstOrDefaultAsync();
                    if (teacher == null)
                    {
                        ViewBag.Error = "Thông tin giáo viên không tồn tại.";
                        return View();
                    }

                    userName = teacher.FirstName;
                    userId = teacher.AccountID;
                }

                else if (account.Role == "admin")
                {
                    // Bạn có thể tạo bảng Admin riêng nếu cần, hoặc chỉ cần dùng thông tin từ bảng Account
                    userName = account.Email ?? "Admin";
                    userId = account.AccountID;
                }

                else
                {
                    ViewBag.Error = "Loại tài khoản không hợp lệ.";
                    return View();
                }

                // Set session
                HttpContext.Session.SetString("name", userName);
                HttpContext.Session.SetString("role", account.Role);

                // Set Claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("Phone", account.Phone),
            new Claim(ClaimTypes.Role, account.Role),
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimPrincipal);

                // Chuyển hướng
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (account.Role == "parent")
                    return RedirectToAction("Index", "Home");

                if (account.Role == "teacher")
                    return RedirectToAction("TeacherHomeLand", "Admin");

                if (account.Role == "admin")
                    return RedirectToAction("AdminDashboard", "Admin");

                return RedirectToAction("Index", "Home");
            }

            // Nếu thông tin không hợp lệ
            ViewBag.Error = "Your Phone or Password is incorrect. Please try again.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }





        // Logout
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear(); // Nếu bạn dùng Session để lưu tên hoặc role
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Xóa cookie chứa Claims
            return RedirectToAction("Login", "Account");
        }


        // Phương thức hỗ trợ tạo ParentID tự động
        private async Task<string> GenerateParentID()
        {
            // Tùy cách bạn tạo mã ID
            var count = await _parentsCollection.CountDocumentsAsync(_ => true);
            return $"P{count + 1:D4}"; // Ví dụ: P0001, P0002,...
        }


        //private string HashPassword(string password)
        //{
        //    using var sha256 = System.Security.Cryptography.SHA256.Create();
        //    var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        //    var hash = sha256.ComputeHash(bytes);
        //    return Convert.ToBase64String(hash);
        //}
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


    }



}
