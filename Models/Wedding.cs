#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models;

public class Wedding
{
    [Key]
    public int WeddingId { get; set; }
    [Required(ErrorMessage = "Wedding must have two Nearlyweds.")]
    [Display(Name = "Nearlywed One")]
    public string NearlywedOne { get; set; }
    [Required(ErrorMessage = "Wedding must have two Nearlyweds.")]
    [Display(Name = "Nearlywed Two")]
    public string NearlywedTwo { get; set; }
    [Required(ErrorMessage = "Wedding must have date.")]
    [FutureDate]
    public DateTime? Date { get; set; }
    [Required(ErrorMessage = "Wedding must have address.")]
    public string Address { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public User? Creator { get; set; }
    public List<Commitment> WeddingGuests { get; set; } = new List<Commitment>();

}

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Wedding must have date.");
        }
        // if value is null, this is false
        // if they added a model binding for DateOnly, I would use this
        // if ((DateOnly?)value <= DateOnly.FromDateTime(DateTime.Today))
        if ((DateTime?)value > DateTime.Today)
        {
            return ValidationResult.Success;
        }
        else
        {
            return new ValidationResult("Wedding must be in future.");
        }
    }
}