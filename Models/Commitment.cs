using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models;

public class Commitment : IValidatableObject
{
    [Key]
    public int CommitmentId { get; set; }
    public int UserId { get; set; }
    public int WeddingId { get; set; }

    public User? Guest { get; set; }
    public Wedding? Wedding { get; set; }

    // validate commitment for valid attendees and uniqueness
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // ID TEST check that the ids have been set (0 is the default and is
        // not valid in the database)
        if (UserId == 0 || WeddingId == 0)
        {
            yield return new ValidationResult("Commitment isn't fully initialized",
                                              new[] { nameof(CommitmentId) });
            // break out of function (this reduces the nesting)
            yield break;
        }
        // CONTEXT TEST: get context and makes sure it succeeded (this should
        // not be null)
        WeddingPlannerContext? _context = (WeddingPlannerContext?)validationContext.GetService(typeof(WeddingPlannerContext));
        if (_context == null)
        {
            yield return new ValidationResult("Could not retrieve database context to perform validation.",
                                              new[] { nameof(CommitmentId) });
            // break out of function (this reduces the nesting)
            yield break;
        }
        // found context, proceed with test
        bool isGuestValid = _context.Users.Any(p => p.UserId == UserId);
        bool isWeddingValid = _context.Weddings.Any(p => p.WeddingId == WeddingId);
        // UNIQUENESS TEST: if both are valid, we check the uniqueness
        if (isGuestValid && isWeddingValid &&
            _context.Commitments.Any(a => a.UserId == UserId &&
                                     a.WeddingId == WeddingId))
        {
            // not unique
            yield return new ValidationResult("The commitment must not exist already.",
                                                new[] { nameof(CommitmentId) });
            // break out of function (this reduces the nesting)
            yield break;
        }
        // GUESTID TEST: check guest id
        if (!isGuestValid)
        {
            yield return new ValidationResult("The RSVPing guest must exist.",
                                                new[] { nameof(UserId) });
        }
        // WEDDINGID: check wedding id
        if (!isWeddingValid)
        {
            yield return new ValidationResult("The wedding must exist in order to RSVP.",
                                                new[] { nameof(WeddingId) });
        }
    }
}