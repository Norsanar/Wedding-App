#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models;

public class User
{
    [Key]
    public int UserId { get; set; }
    [Required(ErrorMessage = "First name is required.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [UniqueEmail]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<Wedding> CreatedWeddings { get; set; } = new List<Wedding>();
    public List<Commitment> Commitments { get; set; } = new List<Commitment>();

    [NotMapped]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password")]
    public string ConfPass { get; set; }
}

public class UniqueEmailAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Just in case the required attribute fails
        if (value == null)
        {
            return new ValidationResult("Email is required.");
        }

        WeddingPlannerContext? _context = (WeddingPlannerContext?)validationContext.GetService(typeof(WeddingPlannerContext));
        // sanity check (there is basically a zero chance of this being null)
        if(_context == null)
        {
            return new ValidationResult("Could not retrieve database context to perform validation.");
        }

        // Is there a user with this email in the database?
        if (_context.Users.Any(e => e.Email == value.ToString()))
        {
            // a user with this email already exists
            return new ValidationResult("Email must be unique.");
        }
        // no user has this email so we go on our merry way
        return ValidationResult.Success;
    }
}