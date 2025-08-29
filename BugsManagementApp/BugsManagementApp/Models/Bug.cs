using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugsManagementApp.Models
{
    public class Bug
    {
        public int BugID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }

}
