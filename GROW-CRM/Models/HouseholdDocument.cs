using GROW_CRM.Models.Utilities;

namespace GROW_CRM.Models
{
    public class HouseholdDocument : UploadedFile
    {
        //Foreign Keys
        public int HouseholdID { get; set; }

        public Household Household { get; set; }

        public int DocumentTypeID { get; set; }

        public DocumentType DocumentType { get; set; }
    }
}