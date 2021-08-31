using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataAccess.Data;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CompanyApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/CompanyApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            var company = await _db.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        // PUT: api/CompanyApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(int id, [FromBody]Company model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!await CompanyExists(id))
            {
                return NotFound();
            }

            var company = await _db.Companies.Where(x => x.Id == model.Id).SingleOrDefaultAsync();
            
            company.Name = model.Name;
            company.StreetAddress = model.StreetAddress;
            company.City = model.City;
            company.State = model.State;
            company.PostalCode = model.PostalCode;
            company.PhoneNumber = model.PhoneNumber;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Accepted();
        }

        // POST: api/CompanyApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostCompany([FromBody]Company company)
        {
            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            return CreatedAtAction("GetCompany", new { id = company.Id }, company);
        }

        // GET: api/CompanyApi
        [HttpGet]
        public IActionResult GetCompanies([FromQuery]JqueryDatatableParam param)
        {
            var companyList = _db.Companies.ToList();

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

            return Ok(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }

        // DELETE: api/CompanyApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _db.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound(new { success = false, message = "Error while deleting" });
            }

            _db.Companies.Remove(company);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Delete Successful" });
        }

        private async Task<bool> CompanyExists(int id)
        {
            return await _db.Companies.AnyAsync(e => e.Id == id);
        }
    }
}
