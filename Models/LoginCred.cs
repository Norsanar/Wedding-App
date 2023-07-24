#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models;

public class LoginCred
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string EmailCred { get; set; }
    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string PasswordCred { get; set; }
}