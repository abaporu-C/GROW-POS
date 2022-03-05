using GROW_CRM.Models.Utilities;

namespace GROW_CRM.Models
{
    public class MemberDocument : UploadedFile
    {
        //Foreign Keys
        public int MemberID { get; set; }

        public Member Member { get; set; }

        public int? DocumentTypeID { get; set; }

        public DocumentType DocumentType { get; set; }
    }
}