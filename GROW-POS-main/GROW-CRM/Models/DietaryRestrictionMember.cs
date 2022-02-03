namespace GROW_CRM.Models
{
    public class DietaryRestrictionMember
    {
        //Composite Keys
        public int MemberID { get; set; }

        public Member Member { get; set; }

        public int DietaryRestrictionID { get; set; }

        public DietaryRestriction DietaryRestriction { get; set; }
    }
}