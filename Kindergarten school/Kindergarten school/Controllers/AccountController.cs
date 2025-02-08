using Kindergarten_school.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
            var existingParent = await _parentsCollection.Find(p => p.Phone == phone).FirstOrDefaultAsync();
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
                ParentID = await GenerateParentID(),
                Roles = "Parent"
            };

            // Lưu thông tin vào MongoDB
            await _parentsCollection.InsertOneAsync(newParent);

            // Tự động đăng nhập sau khi đăng ký
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, phone),
                new Claim(ClaimTypes.Role, newParent.Roles)
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
        
        public async Task<IActionResult> Login(string email, string password, string phone, string? returnUrl)
        {
            // Tìm người dùng dựa trên số điện thoại
            var parent = await _parentsCollection.Find(p => p.Phone == phone).FirstOrDefaultAsync();

            // Kiểm tra người dùng có tồn tại và mật khẩu có khớp không
            if (parent == null || parent.PasswordHash != password)
            {
                ViewBag.Error = "Your Password or Phone is wrong. Please Check Again!";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Lưu thông tin vào Session
            HttpContext.Session.SetString("name", parent.FirstName);
            HttpContext.Session.SetString("role", parent.Roles);

            // Lấy Role từ MongoDB
            var role = parent.Roles;

            // Tạo Claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, parent.FirstName),
        new Claim("ParentID", parent.ParentID.ToString()),
        new Claim(ClaimTypes.Role, role) // Lấy role động từ MongoDB
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Đăng nhập bằng Claims
            await HttpContext.SignInAsync(claimPrincipal);

            // Chuyển hướng theo ReturnUrl nếu có
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }


            // Chuyển hướng theo Role
            if (role == "parent")
            {
                return RedirectToAction("Index", "Home"); // Trang dành cho Parent
            }
            else if (role == "admin")
            {
                return RedirectToAction("AdminHome", "Account"); // Trang dành cho Admin
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
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

        public IActionResult AdminHome(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }



    }



}
