using GROW_CRM.Data;
using GROW_CRM.Models;
using System.Linq;

namespace GROW_CRM.Controllers.Helpers
{
    public static class VoidHelper
    {
        public async static void CheckVoidMembers(GROWContext _context)
        {
            var members = from m in _context.Members select m;

            foreach (Member m in members)
            {
                if (m.FirstName == "" && m.LastName == "")
                {
                    _context.Remove(m);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async static void CheckVoidMembers(IQueryable<Member> members, GROWContext _context)
        {
            foreach (Member m in members)
            {
                if (m.FirstName == "" && m.LastName == "")
                {
                    _context.Remove(m);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
