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

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

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
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        
                        // Role-based redirection
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        
                        // Redirect based on user role
                        if (roles.Contains("Administrator"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else if (roles.Contains("Ward Admin"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else if (roles.Contains("Doctor"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else if (roles.Contains("Nurse") || roles.Contains("Nursing Sister"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else if (roles.Contains("Script Manager"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else if (roles.Contains("Consumables Manager"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            // Default redirect for users without specific roles
                            return RedirectToAction("Index", "Home");
                        }
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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _userManager.FindByNameAsync(model.Username) != null)
                {
                    ModelState.AddModelError("Username", "Username already exists. Please choose a different one.");
                    return View(model);
                }

                // Check if email already exists
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email already exists. Please use a different email address.");
                    return View(model);
                }

                // Create the user
                var user = new IdentityUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    EmailConfirmed = true // Auto-confirm email for now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add user to role
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    // Create corresponding Staff record
                    var staff = new Staff
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Role = model.Role,
                        Email = model.Email,
                        IsActive = true
                    };

                    _context.Staff.Add(staff);
                    await _context.SaveChangesAsync();

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["SuccessMessage"] = "Account created successfully! Welcome to the Ward Management System.";
                    
                    // Redirect based on role
                    if (model.Role == "Doctor")
                    {
                        return RedirectToAction("Index", "DoctorPatient");
                    }
                    else if (model.Role == "Ward Admin")
                    {
                        return RedirectToAction("Index", "Administration");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Backward compatibility redirect for old Account routes
        [HttpGet]
        [Route("Account/Login")]
        public IActionResult AccountLoginRedirect(string? returnUrl = null)
        {
            return RedirectToAction("Login", new { returnUrl });
        }

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


