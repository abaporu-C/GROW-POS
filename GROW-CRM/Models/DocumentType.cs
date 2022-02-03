using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class DocumentType
    {
        //Constructor
        public DocumentType()
        {
            MemberDocuments = new HashSet<MemberDocument>();
        }

        //Fields
        public int ID { get; set; }

        public string Type { get; set; }

        //O:M Relationships

        public ICollection<MemberDocument> MemberDocuments { get; set; }
    }
}