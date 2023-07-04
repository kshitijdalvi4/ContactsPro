using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ContactsPro.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [Display(Name="Categories")]
        public string? Name { get; set; }

        //Virtuals
        public virtual AppUser? AppUser { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; } =new HashSet<Contact>();
    }
}
