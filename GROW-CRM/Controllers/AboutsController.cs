using GROW_CRM.Data;
using GROW_CRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize(Roles="Admin,SuperAdmin")]
    public class AboutsController : Controller
    {
        private readonly GROWContext _context;

        public AboutsController(GROWContext context)
        {
            _context = context;
        }

        // GET: Abouts
        public async Task<IActionResult> Index()
        {
            var gROWContext = _context.Abouts.Include(a => a.City).Include(a => a.Province);



            return View(await gROWContext.ToListAsync());
        }

        // GET: Abouts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var about = await _context.Abouts
                .Include(a => a.City)
                .Include(a => a.Province)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (about == null)
            {
                return NotFound();
            }

            return View(about);
        }

        // GET: Abouts/Create
        public IActionResult Create()
        {
            ViewData["CityID"] = new SelectList(_context.Cities, "ID", "ID");
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID");
            return View();
        }

        // POST: Abouts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,OrgName,PhoneNumber,Email,WebSite,StreetNumber,StreetName,AptNumber,PostalCode,CityID,ProvinceID")] About about)
        {
            if (ModelState.IsValid)
            {
                _context.Add(about);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CityID"] = new SelectList(_context.Cities, "ID", "ID", about.CityID);
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", about.ProvinceID);
            return View(about);
        }

        // GET: Abouts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            /*var about = await _context.Abouts.FindAsync(id);*/

            var about = await _context.Abouts
               .Include(h => h.City)
               .Include(h => h.Province)
               .FirstOrDefaultAsync(h => h.ID == id);


            if (about == null)
            {
                return NotFound();
            }
            /* ViewData["CityID"] = new SelectList(_context.Cities, "ID", "ID", about.CityID);
             ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", about.ProvinceID);*/
            PopulateDropDownLists(about);
            return View(about);
        }

        // POST: Abouts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,OrgName,PhoneNumber,Email,WebSite,StreetNumber,StreetName,AptNumber,PostalCode,CityID,ProvinceID")] About about)
        {
            if (id != about.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(about);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AboutExists(about.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                /*return RedirectToAction(nameof(Index));*/
                return RedirectToAction("Create", "Households");
            }
            /* ViewData["CityID"] = new SelectList(_context.Cities, "ID", "ID", about.CityID);
             ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", about.ProvinceID);*/

            PopulateDropDownLists(about);



            /*return View("Create", household);*/
            return View(about);
        }

        // GET: Abouts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var about = await _context.Abouts
                .Include(a => a.City)
                .Include(a => a.Province)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (about == null)
            {
                return NotFound();
            }

            return View(about);
        }

        // POST: Abouts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var about = await _context.Abouts.FindAsync(id);
            _context.Abouts.Remove(about);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AboutExists(int id)
        {
            return _context.Abouts.Any(e => e.ID == id);
        }

        private SelectList CitySelectList(int? selectedId)
        {
            return new SelectList(_context.Cities
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private SelectList ProvinceSelectList(int? selectedId)
        {
            return new SelectList(_context.Provinces

                .OrderByDescending(d => d.Name == "Ontario")
                .ThenBy(d => d.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(About about = null)
        {
            ViewData["CityID"] = CitySelectList(about?.CityID);
            ViewData["ProvinceID"] = ProvinceSelectList(about?.ProvinceID);
           
        }

    }
}
