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

        public AccountController(IMongoDatabase database)
        {
            _parentsCollection = database.GetCollection<Parent>("parents");
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string phone, string password)
        {
            // Kiểm tra email đã tồn tại trong MongoDB chưa
            var existingParent = await _parentsCollection.Find(p => p.Email == email).FirstOrDefaultAsync();
            if (existingParent != null)
            {
                ViewBag.Error = "Email is already in use. Please try another email.";
                return View();
            }

            // Tạo đối tượng Parent mới
            var newParent = new Parent
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                PasswordHash = password, // Nên mã hóa mật khẩu tại đây
                ParentID = await GenerateParentID()
            };

            // Lưu thông tin vào MongoDB
            await _parentsCollection.InsertOneAsync(newParent);

            // Tự động đăng nhập sau khi đăng ký
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, "Parent")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl)
        {
            // Kiểm tra email tồn tại trong MongoDB
            var parent = await _parentsCollection.Find(p => p.Email == email).FirstOrDefaultAsync();
            if (parent == null || parent.PasswordHash != password) // Kiểm tra mật khẩu
            {
                ViewBag.Error = "Your Password or Email is wrong. Please Check Again!.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Tạo Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim("ParentID", parent.ParentID.ToString()),
                new Claim(ClaimTypes.Role, "Parent")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(claimPrincipal);

            // Chuyển hướng theo ReturnUrl nếu có
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // Phương thức hỗ trợ tạo ParentID tự động
        private async Task<int> GenerateParentID()
        {
            var lastParent = await _parentsCollection.Find(_ => true).SortByDescending(p => p.ParentID).FirstOrDefaultAsync();
            return (lastParent?.ParentID ?? 0) + 1;
        }
    }
}
