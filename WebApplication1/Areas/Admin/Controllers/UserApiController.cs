using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Areas.Identity.Pages.Account;
using WebApplication1.DataAccess.Data;
using WebApplication1.Models;
using WebApplication1.Utility;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserApiController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: api/UserApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _db.ApplicationUsers.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/UserApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, [FromBody]ApplicationUser model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!UserExists(id))
            {
                return NotFound();
            }

            var user = await _db.ApplicationUsers.Where(x => x.Id == model.Id).SingleOrDefaultAsync();
            user.UserName = model.Email;
            user.Email = model.Email;
            user.CompanyId = model.CompanyId;
            user.StreetAddress = model.StreetAddress;
            user.City = model.City;
            user.State = model.State;
            user.PostalCode = model.PostalCode;
            user.Name = model.Name;
            user.PhoneNumber = model.PhoneNumber;
            user.Role = model.Role;

            if (user.Role != null)
            {
                await _userManager.AddToRoleAsync(user, user.Role);
            }

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

        // POST: api/UserApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody]RegisterModel.InputModel Input)
        {
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                CompanyId = Input.CompanyId,
                StreetAddress = Input.StreetAddress,
                City = Input.City,
                State = Input.State,
                PostalCode = Input.PostalCode,
                Name = Input.Name,
                PhoneNumber = Input.PhoneNumber,
                Role = Input.Role
            };

            await _userManager.CreateAsync(user, Input.Password);

            if (user.Role == null)
            {
                await _userManager.AddToRoleAsync(user, SD.Role_Employee);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, user.Role);
            }

            return CreatedAtAction("GetCompany", new { id = user.Id }, user);
        }

        // GET: api/CompanyApi
        [HttpGet]
        public IActionResult GetUsers([FromQuery]JqueryDatatableParam param)
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

            return Ok(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }

        // DELETE: api/UserApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _db.ApplicationUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Error while deleting" });
            }

            _db.ApplicationUsers.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Delete Successful" });
        }

        private async Task<bool> UserExists(string id)
        {
            return await _db.ApplicationUsers.AnyAsync(e => e.Id == id);
        }
    }
}
