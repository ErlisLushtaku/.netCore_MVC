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

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult Upsert(string id)
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
        // }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Upsert(Company company)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (company.Id == 0)
        //        {
        //            _unitOfWork.Company.Add(company);

        //        }
        //        else
        //        {
        //            _unitOfWork.Company.Update(company);
        //        }
        //        _unitOfWork.Save();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(company);
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