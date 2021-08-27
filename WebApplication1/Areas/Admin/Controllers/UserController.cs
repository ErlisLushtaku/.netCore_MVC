using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using WebApplication1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebApplication1.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Areas.Identity.Pages.Account;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserController(
            ApplicationDbContext db, 
            RoleManager<IdentityRole> roleManager, 
            UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public UpdateUserViewModel Input { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Update(string id)
        {
            //ApplicationUser applicationUser = _db.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == id);
            ApplicationUser applicationUser = _db.ApplicationUsers.Find(id);

            if (applicationUser != null)
            {
                Input = new UpdateUserViewModel
                {
                    Id = id,
                    Email = applicationUser.Email,
                    Name = applicationUser.Name,
                    StreetAddress = applicationUser.StreetAddress,
                    City = applicationUser.City,
                    State = applicationUser.State,
                    PostalCode = applicationUser.PostalCode,
                    PhoneNumber = applicationUser.PhoneNumber,
                    CompanyId = applicationUser.CompanyId,
                    Role = applicationUser.Role
                };
            }
            else
            {
                return NotFound();
            }

            Input.CompanyList = _db.Set<Company>().ToList().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            Input.RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });

            return View(Input);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update()
        {
            if (ModelState.IsValid)
            {
                var user = await _db.ApplicationUsers.Where(x => x.Id == Input.Id).SingleOrDefaultAsync();

                user.UserName = Input.Email;
                user.Email = Input.Email;
                user.CompanyId = Input.CompanyId;
                user.StreetAddress = Input.StreetAddress;
                user.City = Input.City;
                user.State = Input.State;
                user.PostalCode = Input.PostalCode;
                user.Name = Input.Name;
                user.PhoneNumber = Input.PhoneNumber;
                user.Role = Input.Role;

                if (user.Role != null)
                {
                    await _userManager.AddToRoleAsync(user, user.Role);
                }

                await _db.SaveChangesAsync();

                if (User.IsInRole(SD.Role_Admin))
                {
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Index", "Home");
            }

            return View(Input);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);
                ViewBag.Message = "Password changed successfully!";

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(JqueryDatatableParam param)
        {
            var userList = _db.ApplicationUsers.Include(u => u.Company).ToList();
            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }

            // Filter by search
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                userList = userList.Where(x => x.Name.ToLower().Contains(param.sSearch.ToLower())
                                              || x.Email.ToLower().Contains(param.sSearch.ToLower())
                                              || x.PhoneNumber.ToLower().Contains(param.sSearch.ToLower())
                                              || x.Company.Name.ToLower().Contains(param.sSearch.ToLower())
                                              || x.Role.ToLower().Contains(param.sSearch.ToLower())).ToList();
            }

            // Sorting
            var sortColumnIndex = param.iSortCol_0;
            var sortDirection = param.sSortDir_0;
            //  var sortColumnIndex = Convert.ToInt32(HttpContext.Request.Query["iSortCol_0"]);
            //  var sortDirection = HttpContext.Request.Query["sSortDir_0"];
            if (sortColumnIndex == 0)
            {
                userList = sortDirection == "asc" ? userList.OrderBy(c => c.Name).ToList() : userList.OrderByDescending(c => c.Name).ToList();
            }
            else if (sortColumnIndex == 1)
            {
                userList = sortDirection == "asc" ? userList.OrderBy(c => c.Email).ToList() : userList.OrderByDescending(c => c.Email).ToList();
            }
            else if (sortColumnIndex == 2)
            {
                userList = sortDirection == "asc" ? userList.OrderBy(c => c.PhoneNumber).ToList() : userList.OrderByDescending(c => c.PhoneNumber).ToList();
            }
            else if (sortColumnIndex == 3)
            {
                userList = sortDirection == "asc" ? userList.OrderBy(c => c.Company.Name).ToList() : userList.OrderByDescending(c => c.Company.Name).ToList();
            }
            else
            {
                userList = sortDirection == "asc" ? userList.OrderBy(c => c.Role).ToList() : userList.OrderByDescending(c => c.Role).ToList();
            }

            // Pagination
            var displayResult = userList.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            var totalRecords = userList.Count();

            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var objFromDb = await _db.ApplicationUsers.FindAsync(id);

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _db.ApplicationUsers.Remove(objFromDb);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}