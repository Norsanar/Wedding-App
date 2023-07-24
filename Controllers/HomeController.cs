using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private WeddingPlannerContext _context;

    public HomeController(ILogger<HomeController> logger, WeddingPlannerContext context)
    {
        _logger = logger;
        _context = context;
    }

    // login page
    [HttpGet("")]
    public IActionResult Index()
    {
        // are we already logged in?
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            return RedirectToAction("Weddings");
        }
        return View();
    }

    // register new user and add to database
    [HttpPost("users/create")]
    public IActionResult CreateUser(User newUser)
    {
        // are we already logged in?
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            return RedirectToAction("Weddings");
        }
        // run the user validations
        if (!ModelState.IsValid)
        {
            // not valid, show login page again
            return View("Index");
        }
        // create a password hasher
        PasswordHasher<User> hasher = new PasswordHasher<User>();
        // so we can hash the password before storing it in the database
        newUser.Password = hasher.HashPassword(newUser, newUser.Password);
        // add user to database
        _context.Add(newUser);
        _context.SaveChanges();
        // save their id to session so they are logged in
        HttpContext.Session.SetInt32("UserId", newUser.UserId);
        // go to a weddings page
        return RedirectToAction("Weddings");
    }

    // login user
    [HttpPost("login")]
    public IActionResult Login(LoginCred loginCred)
    {
        // are we already logged in?
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            return RedirectToAction("Weddings");
        }
        // run the user login validations
        if (!ModelState.IsValid)
        {
            // not valid, show login page again
            return View("Index");
        }
        // find the corresponding user in the database by email
        User? user = _context.Users.FirstOrDefault(u => u.Email == loginCred.EmailCred);
        // does a user with this email exist?
        if (user == null)
        {
            // No user has that email, go to login page and display error
            ModelState.AddModelError("EmailCred", "Email or Password is invalid");
            return View("Index");
        }
        // Some user has that email, so we check the login password against
        // their password. Create a password hasher
        PasswordHasher<LoginCred> hasher = new PasswordHasher<LoginCred>();
        // Check password using hasher
        var passCheck = hasher.VerifyHashedPassword(loginCred, user.Password, loginCred.PasswordCred);
        if (passCheck == 0)
        {
            // Invalid email/password combo,  go to login and display error
            ModelState.AddModelError("EmailCred", "Email or Password is invalid");
            return View("Index");
        }
        // The email and password match, go to weddings page
        HttpContext.Session.SetInt32("UserId", user.UserId);
        return RedirectToAction("Weddings");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    // see all weddings
    [UserLoggedIn]
    [HttpGet("weddings")]
    public IActionResult Weddings()
    {
        int userId = (int)HttpContext.Session.GetInt32("UserId")!;
        List<WeddingView> allWeddings = _context.Weddings.Include(w => w.WeddingGuests)
                                        .Select(w => new WeddingView
                                        {
                                            WeddingId = w.WeddingId,
                                            NearlywedOne = w.NearlywedOne,
                                            NearlywedTwo = w.NearlywedTwo,
                                            Date = w.Date,
                                            // we need the commitment Id if the current user is going to this wedding.
                                            // thus we take only the rsvps where the logged in user is involved
                                            // I would have liked to condense this into a single int, but I could
                                            // not figure out how to do that in such a way that if the list was null
                                            // I didn't get an error
                                            WeddingGuests = w.WeddingGuests.Where(c => c.UserId == userId).ToList(),
                                            GuestCount = w.WeddingGuests.Count(),
                                            // this allows us to not call session on the front end
                                            CanDelete = w.UserId == userId,
                                        }).ToList();
        return View(allWeddings);
    }

    // see form to plan a new wedding
    [UserLoggedIn]
    [HttpGet("weddings/new")]
    public IActionResult NewWedding()
    {
        return View();
    }

    // create a new wedding and add it to the database
    [UserLoggedIn]
    [HttpPost("weddings/create")]
    public IActionResult CreateWedding(Wedding newWedding)
    {
        // check if the wedding is valid
        if (!ModelState.IsValid)
        {
            // validation failed, see errors
            return View("NewWedding");
        }
        // validation successful, add current user id to it as creator
        // since user is logged int, we don't need to check that it isn't null
        newWedding.UserId = (int)HttpContext.Session.GetInt32("UserId")!;
        // I'm not sure whether I should add the current user as an RSVP by
        // default or leave that as a later action for them to do (or whether
        // the planner of a wedding can RSVP at all)
        // _context.Add(new Commitment()
        // {
        //     UserId = newWedding.UserId,
        //     WeddingId = newWedding.WeddingId
        // });
        // add wedding to database
        _context.Add(newWedding);
        _context.SaveChanges();
        return RedirectToAction("Weddings");
    }

    // Show details of one wedding
    [UserLoggedIn]
    [HttpGet("weddings/{id}")]
    public IActionResult ShowWedding(int id)
    {
        // retrieve the wedding
        Wedding? wedding = _context.Weddings.Include(w => w.WeddingGuests)
                                            .ThenInclude(g => g.Guest)
                                            .FirstOrDefault(w => w.WeddingId == id);
        // does a wedding with this id exist?
        if (wedding == null)
        {
            // no, redirect to weddings page
            return RedirectToAction("Weddings");
        }
        return View(wedding);
    }

    // Delete one wedding
    [UserLoggedIn]
    [HttpPost("weddings/{id}/destroy")]
    public IActionResult DestroyWedding(int id)
    {
        // retrieve the wedding to destroy
        Wedding? wedding = _context.Weddings.SingleOrDefault(w => w.WeddingId == id);
        // does a wedding with this id exist? Is the logged user in the
        // creator of this wedding?
        if (wedding != null &&
            wedding.UserId == (int)HttpContext.Session.GetInt32("UserId")!)
        {
            // yes, remove from database
            _context.Weddings.Remove(wedding);
            _context.SaveChanges();
        }
        // go back to weddings page
        return RedirectToAction("Weddings");
    }

    // add a commitment to database
    [UserLoggedIn]
    [HttpPost("commitments/create")]
    public IActionResult CreateCommitment(Commitment newCommitment)
    {
        // add current user id to commitment since user is logged int, we
        // don't need to check that it isn't null
        newCommitment.UserId = (int)HttpContext.Session.GetInt32("UserId")!;
        // need to rerun validations after adding UserId
        ModelState.ClearValidationState("CommitmentId");
        if (!TryValidateModel(newCommitment))
        {
            // this shouldn't be triggerable by navigating the UI, so it is
            // primarily for testing
            return View();
        }
        // validation successful add commitment to database
        _context.Add(newCommitment);
        _context.SaveChanges();
        return RedirectToAction("Weddings");
    }

    // Delete one commitment (of the users)
    [UserLoggedIn]
    [HttpPost("commitments/{id}/destroy")]
    public IActionResult DestroyCommitment(int id)
    {
        // retrieve the commitment to destroy
        Commitment? commitment = _context.Commitments.SingleOrDefault(c => c.CommitmentId == id);
        // does a commitment with this id exist? Is the logged user the one
        // referred to in this commitment?
        if (commitment != null &&
            commitment.UserId == (int)HttpContext.Session.GetInt32("UserId")!)
        {
            // yes, remove from database
            _context.Commitments.Remove(commitment);
            _context.SaveChanges();
        }
        // go back to weddings page
        return RedirectToAction("Weddings");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class UserLoggedInAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // If the user has logged in, there will be a User Id in session
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        // Session will return null if it is not present, so we check for that
        if (userId == null)
        {
            // If it is not in session, the user is not logged in
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}