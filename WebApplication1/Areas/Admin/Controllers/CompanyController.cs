using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;
using WebApplication1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DataAccess.Data;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null)
            {
                //this is for create
                return View(company);
            }
            //this is for edit
            company = _unitOfWork.Company.Get(id.GetValueOrDefault());
            if (company == null)
            {
                return NotFound();
            }
            return View(company);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                    
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(JqueryDatatableParam param)
        {
            var companyList = _unitOfWork.Company.GetAll().ToList();

            // Filter by search
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                companyList = companyList.Where(x => x.Name.ToLower().Contains(param.sSearch.ToLower())
                                              || x.StreetAddress.ToLower().Contains(param.sSearch.ToLower())
                                              || x.City.ToLower().Contains(param.sSearch.ToLower())
                                              || x.State.ToLower().Contains(param.sSearch.ToLower())
                                              || x.PhoneNumber.ToLower().Contains(param.sSearch.ToLower())).ToList();
            }

            // Sorting
            var sortColumnIndex = param.iSortCol_0;
            var sortDirection = param.sSortDir_0;
            //  var sortColumnIndex = Convert.ToInt32(HttpContext.Request.Query["iSortCol_0"]);
            //  var sortDirection = HttpContext.Request.Query["sSortDir_0"];
            if (sortColumnIndex == 0)
            {
                companyList = sortDirection == "asc" ? companyList.OrderBy(c => c.Name).ToList() : companyList.OrderByDescending(c => c.Name).ToList();
            }
            else if (sortColumnIndex == 1)
            {
                companyList = sortDirection == "asc" ? companyList.OrderBy(c => c.StreetAddress).ToList() : companyList.OrderByDescending(c => c.StreetAddress).ToList();
            }
            else if (sortColumnIndex == 2)
            {
                companyList = sortDirection == "asc" ? companyList.OrderBy(c => c.City).ToList() : companyList.OrderByDescending(c => c.City).ToList();
            }
            else if (sortColumnIndex == 3)
            {
                companyList = sortDirection == "asc" ? companyList.OrderBy(c => c.State).ToList() : companyList.OrderByDescending(c => c.State).ToList();
            }
            else
            {
                companyList = sortDirection == "asc" ? companyList.OrderBy(c => c.PhoneNumber).ToList() : companyList.OrderByDescending(c => c.PhoneNumber).ToList();
            }

            // Pagination
            var displayResult = companyList.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            var totalRecords = companyList.Count();

            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Company.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}