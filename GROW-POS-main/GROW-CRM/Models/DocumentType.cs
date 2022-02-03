using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class DocumentType
    {
        //Constructor
        public DocumentType()
        {
            HouseholdDocuments = new HashSet<HouseholdDocument>();
        }

        //Fields
        public int ID { get; set; }

        public string Type { get; set; }

        //O:M Relationships

        public ICollection<HouseholdDocument> HouseholdDocuments { get; set; }
    }
}