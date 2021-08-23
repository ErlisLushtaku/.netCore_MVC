using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;
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

        public UserController(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [BindProperty]
        public UpdateUser Input { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Update(string id)
        {
            ApplicationUser applicationUser = _db.ApplicationUsers.Find(id);

            if (applicationUser != null)
            {
                Input = new UpdateUser
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
                var user = await _db.ApplicationUsers.Where(x => x.Id == Input.Id)
                    //.Where(x => x.Email == Input.Email)
                    .SingleOrDefaultAsync();

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

                //_unitOfWork.Company.Update(user);

                //_unitOfWork.Save();

                //if (result.Succeeded)
                //{
                    if (user.Role == null)
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Employee);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, user.Role);
                    }

                    await _db.SaveChangesAsync();


                    return RedirectToAction(nameof(Index));
                //}
                //foreach (var error in result.Errors)
                //{
                //    ModelState.AddModelError(string.Empty, error.Description);
                //}
            }

            return View(Input);
        }

        //public IActionResult Update(string id)
        //{
        //    ApplicationUser applicationUser = _db.ApplicationUsers.Find(id);
        //    if (applicationUser == null)
        //    {
        //        return NotFound();
        //    }
        //    var input = new RegisterModel.InputModel
        //    {
        //        Email = applicationUser.Email,
        //        Password = "",
        //        ConfirmPassword = "",
        //        Name = applicationUser.Name,
        //        StreetAddress = applicationUser.StreetAddress,
        //        City = applicationUser.City,
        //        State = applicationUser.State,
        //        PostalCode = applicationUser.PostalCode,
        //        PhoneNumber = applicationUser.PhoneNumber,
        //        CompanyId = applicationUser.CompanyId,
        //        Role = applicationUser.Role
        //    };

        //    string _sinput = JsonSerializer.Serialize(input);

        //    return RedirectToPage("/Identity/Account/Register", new { sinput = _sinput });
        //}

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
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

            return Json(new { data = userList });
        }

        //[HttpDelete]
        //public IActionResult Delete(int id)
        //{
        //    var objFromDb = _unitOfWork.Company.Get(id);
        //    if (objFromDb == null)
        //    {
        //        return Json(new { success = false, message = "Error while deleting" });
        //    }
        //    _unitOfWork.Company.Remove(objFromDb);
        //    _unitOfWork.Save();
        //    return Json(new { success = true, message = "Delete Successful" });
        //}

        #endregion
    }
}