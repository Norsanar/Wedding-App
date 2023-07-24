namespace WeddingPlanner.Models;

public class WeddingView : Wedding
{
    public int GuestCount { get; set; }
    public bool CanDelete { get; set; }
}