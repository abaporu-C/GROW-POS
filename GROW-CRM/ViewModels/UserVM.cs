using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.ViewModels
{
    public class UserVM
    {
        public string Id { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Roles")]
        public IList<string> UserRoles { get; set; }
    }
}