using System.ComponentModel.DataAnnotations;

namespace ContactManager.Models;

public class Contact
{
    public int ContactID { get; set; }

    [Required, StringLength(50)]
    public string Prenom { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Adresse { get; set; }

    [StringLength(60)]
    public string? Ville { get; set; }

    [StringLength(60)]
    public string? Province { get; set; }

    [StringLength(10)]
    public string? CodePostal { get; set; }

    [Phone]
    public string? Telephone { get; set; }

    [EmailAddress]
    public string? Courriel { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    [Required]
    public int CategorieID { get; set; }
    public Categorie? Categorie { get; set; }

    
    public string UserName { get; set; } = string.Empty;
}
