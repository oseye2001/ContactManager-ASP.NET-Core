using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ContactManager.Models;

public class Categorie
{
    public int CategorieID { get; set; }

    [Required, StringLength(50)]
    public string Nom { get; set; } = string.Empty;

  
    public string? UserId { get; set; }
    [ValidateNever]
    public IdentityUser User { get; set; } = null!;
    [ValidateNever]

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
