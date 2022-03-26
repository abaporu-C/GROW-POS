using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.ViewModels
{
    public class RoleWithUserVM
    {
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

        [Display(Name = "User Account")]
        public string UserName { get; set; }

        [Display(Name = "User Name")]
        public string UserFullName { get; set; }
    }
}
