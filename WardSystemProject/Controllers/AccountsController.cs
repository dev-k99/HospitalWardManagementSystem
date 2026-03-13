using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using WardSystemProject.Models;
using WardSystemProject.ViewModels;
using WardSystemProject.Data;

namespace WardSystemProject.Controllers
{
    public class AccountsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly WardSystemDBContext _context;

        public AccountsController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            WardSystemDBContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }
        // Landing page - default route
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Try to find user by username or email
                var user = await _userManager.FindByNameAsync(model.UsernameOrEmail) ?? 
                          await _userManager.FindByEmailAsync(model.UsernameOrEmail);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty,
                            "This account has been locked after too many failed attempts. Please try again in 15 minutes.");
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid password.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found. Please check your username/email.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// User account creation is restricted to Administrators only.
        /// Self-registration is disabled — a healthcare system must control
        /// who can access patient data and with what role.
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName       = model.Username,
                Email          = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            // Create matching Staff record — link via IdentityUserId for fast lookup
            _context.Staff.Add(new Staff
            {
                FirstName      = model.FirstName,
                LastName       = model.LastName,
                Role           = model.Role,
                Email          = model.Email,
                IdentityUserId = user.Id,
                IsActive       = true
            });
            await _context.SaveChangesAsync();

            // Administrator created the account — do NOT auto-sign-in as the new user.
            TempData["SuccessMessage"] = $"Account for {model.FirstName} {model.LastName} ({model.Role}) created successfully.";
            return RedirectToAction("Index", "StaffManagement");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Backward compatibility redirect for old Account routes
        //[HttpGet]
        //[Route("Account/Login")]
        //public IActionResult AccountLoginRedirect(string? returnUrl = null)
        //{
        //    return RedirectToAction("Login", new { returnUrl });
        //}

        [HttpGet]
        [Route("Account/AccessDenied")]
        public IActionResult AccountAccessDeniedRedirect()
        {
            return RedirectToAction("AccessDenied");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}


