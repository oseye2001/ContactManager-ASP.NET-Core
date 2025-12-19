using ContactManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContactManager.ViewModels;

public class ContactsIndexVM
{
    public List<Contact> Items { get; set; } = new();

    public string? Search { get; set; }
    public int? CategorieId { get; set; }
    public SelectList? Categories { get; set; }

    public string Sort { get; set; } = "nom_asc";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
