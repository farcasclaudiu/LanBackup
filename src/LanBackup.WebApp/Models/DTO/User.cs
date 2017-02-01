using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace LanBackup.WebApp.Models
{
  public class User
  {
    public IdentityError[] Errors;

    public string Email { get; set; }
    public string Password { get; set; }
    public string NewPassword { get; set; }
    public bool IsAdmin { get; set; }
    public bool Succeeded { get; set; }
    public bool IsLockedOut { get; set; }
    

  }
}
