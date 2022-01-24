using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models.Utilities
{
    public class UploadedFile : Auditable
    {
        public UploadedFile()
        {
            FileContent = new FileContent();
        }
        public int ID { get; set; }

        [StringLength(255, ErrorMessage = "The name of the file cannot be more than 255 characters.")]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        public FileContent FileContent { get; set; }
    }
}