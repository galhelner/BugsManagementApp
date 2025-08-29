using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugsManagementApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ParentId { get; set; }
        public string? ParentName { get; set; }
        public List<Category> ChildCategories { get; set; } = new List<Category>();
        public List<Bug> Bugs { get; set; } = new List<Bug>();
    }

}
