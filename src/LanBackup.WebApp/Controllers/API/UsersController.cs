using LanBackup.WebApp.Data;
using LanBackup.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanBackup.WebApp.Models.Telemetry;

namespace LanBackup.WebApp.Controllers
{
  /// <summary>
  /// BackupConfig REST Web api controller for handling users
  /// </summary>
  [Authorize]
  [Produces("application/json")]
  [Route("api/[controller]")]
  public class UsersController : Controller
  {


    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _appDbContext;
    private ITelemetryLogger telemetry;


    public UsersController(
      ApplicationDbContext appDbContex,
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager,
      ITelemetryLogger telemetry
      )
    {
      this.telemetry = telemetry;
      _userManager = userManager;
      _signInManager = signInManager;
      _appDbContext = appDbContex;
    }




    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IdentityResult), 200)]
    [ProducesResponseType(typeof(IdentityResult), 400)]
    public async Task<IActionResult> Register([FromBody]User dtouser)
    {
      try
      {

        dtouser.IsAdmin = false;

        var user = new ApplicationUser { UserName = dtouser.Email, Email = dtouser.Email };
        var result = await _userManager.CreateAsync(user, dtouser.Password);
        if (result.Succeeded)
        {

          //update is Admin
          var adminRole = _appDbContext.Roles.FirstOrDefault(r => r.NormalizedName == "ADMIN");
          var iuser = _appDbContext.Users.FirstOrDefault(u => u.Email == dtouser.Email);
          dtouser.IsAdmin = _appDbContext.UserRoles.FirstOrDefault(u => u.RoleId == adminRole.Id && u.UserId == iuser.Id) != null;

          await _signInManager.SignInAsync(user, isPersistent: false);
          dtouser.Succeeded = true;
          this.telemetry.TrackEvent("NewRegistration");
          return new ObjectResult(dtouser);
        }
        else
        {
          string msg = string.Join("|", result.Errors.Select(e => e.Description));
          return BadRequest(msg);
        }
      }
      catch (Exception ex)
      {
        this.telemetry.TrackException(ex);
        return BadRequest(ex.Message);
      }
      return BadRequest();
    }




    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(typeof(User), 400)]
    public async Task<IActionResult> Login([FromBody]User dtouser)
    {
      try
      {
        dtouser.Succeeded = false;
        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        var result = await _signInManager.PasswordSignInAsync(dtouser.Email, dtouser.Password, isPersistent: false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
          dtouser.Succeeded = result.Succeeded;
          dtouser.IsLockedOut = result.IsLockedOut;

          //update is Admin
          var adminRole = _appDbContext.Roles.FirstOrDefault(r => r.NormalizedName == "ADMIN");
          var iuser = _appDbContext.Users.FirstOrDefault(u => u.Email == dtouser.Email);
          dtouser.IsAdmin = _appDbContext.UserRoles.FirstOrDefault(u => u.RoleId == adminRole.Id && u.UserId == iuser.Id) != null;

          var user = new ApplicationUser { UserName = dtouser.Email, Email = dtouser.Email };

          this.telemetry.TrackEvent("LoginSuccess");
          return Ok(dtouser);
        }
        else
        {
          this.telemetry.TrackEvent("LoginFailure");
          dtouser.Errors = new IdentityError[] { new IdentityError { Description = "Login failed" } };
        }
        return Ok(dtouser);
      }
      catch (Exception ex)
      {
        this.telemetry.TrackException(ex);
        return BadRequest(ex.Message);
      }
    }





    [HttpPost("pwchange")]
    [AllowAnonymous] //TODO - allow only authenticated users to do the update
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 401)]
    public async Task<IActionResult> ChangePasssword([FromBody]User dtouser)
    {
      try
      {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user != null)
        {
          dtouser.Succeeded = false;
          dtouser.IsLoggedIn = true;
          var result = await _userManager.ChangePasswordAsync(user, dtouser.Password, dtouser.NewPassword);
          if (result.Succeeded)
          {
            dtouser.Succeeded = result.Succeeded;
            dtouser.Errors = result.Errors.ToArray();
            await _signInManager.SignInAsync(user, isPersistent: false);
            this.telemetry.TrackEvent("ChangePasswordSuccess");
            return Ok(dtouser);
          }
          else
          {
            this.telemetry.TrackEvent("ChangePasswordFailure");
            string msg = string.Join("|", result.Errors.Select(e => e.Description));
            return BadRequest(msg);
          }
        }
        else
        {
          this.telemetry.TrackEvent("ChangePasswordNotLoggedIn");
          dtouser.Succeeded = false;
          dtouser.IsLoggedIn = false;
          dtouser.Errors = new IdentityError[] { new IdentityError() { Description = "User not logged in" } };
          return Ok(dtouser);
        }
      }
      catch (Exception ex)
      {
        this.telemetry.TrackException(ex);
        return BadRequest(ex.Message);
      }
    }





    [HttpGet("list")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<User>), 200)]
    public async Task<IActionResult> Users()
    {
      var adminRole = _appDbContext.Roles.FirstOrDefault(r => r.NormalizedName == "ADMIN");
      List<User> allusers = new List<User>();
      foreach (var iuser in _appDbContext.Users)
      {
        var isAdmin = _appDbContext.UserRoles.FirstOrDefault(u => u.UserId == iuser.Id && u.RoleId == adminRole.Id) != null;
        allusers.Add(new User() { Email = iuser.Email, IsAdmin = isAdmin });
      }
      return Ok(allusers);
    }


    [HttpPost("list")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(User), 200)]
    public async Task<IActionResult> ChangeAdminRole([FromBody] User user)
    {
      user.Succeeded = true;
      try
      {
        var currUser = await _userManager.GetUserAsync(HttpContext.User);
        if (currUser.Email != user.Email)
        {

          var adminRole = _appDbContext.Roles.FirstOrDefault(r => r.NormalizedName == "ADMIN");
          var iuser = _appDbContext.Users.FirstOrDefault(u => u.Email == user.Email);
          if (iuser != null)
          {
            var role = _appDbContext.UserRoles.FirstOrDefault(u => u.UserId == iuser.Id && u.RoleId == adminRole.Id);
            if (role == null && user.IsAdmin)
            {
              //add
              _appDbContext.UserRoles.Add(new IdentityUserRole<string>() { RoleId = adminRole.Id, UserId = iuser.Id });
              await _appDbContext.SaveChangesAsync();
            }
            if (role != null && !user.IsAdmin)
            {
              //remove
              var ur = _appDbContext.UserRoles.FirstOrDefault(u => u.RoleId == adminRole.Id && u.UserId == iuser.Id);
              if (ur != null)
                _appDbContext.UserRoles.Remove(ur);
            }
          }
          //success
          this.telemetry.TrackEvent("ChangeAdminRole");
        }
        else
        {
          //admin cannot deactivate himself
          user.Succeeded = false;
          user.Errors = new IdentityError[] { new IdentityError() { Description = "User cannot operate over himself" } };
        }
      }
      catch (Exception ex)
      {
        this.telemetry.TrackException(ex);
        user.Succeeded = false;
        user.Errors = new IdentityError[] { new IdentityError() { Description = ex.Message } };
      }
      return Ok(user);
    }


  }
}
