using ContactManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userName = User.Identity.Name!;

            ViewBag.TotalContacts = await _context.Contacts
                .CountAsync(c => c.UserName == userName);

            //  filtrer par user (comme contacts)
            ViewBag.TotalCategories = await _context.Categories
                .CountAsync(c => c.User.UserName == userName);
          

            return View("IndexUser");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
