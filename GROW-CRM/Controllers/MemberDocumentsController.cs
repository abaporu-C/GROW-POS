using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class MemberDocumentsController : Controller
    {
        private readonly GROWContext _context;

        public MemberDocumentsController(GROWContext context)
        {
            _context = context;
        }

        // GET: MemberDocuments
        public async Task<IActionResult> Index(string SearchString,
            int? MemberID, int? page, int? pageSizeID)
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = ""; //Asume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            ViewData["MemberID"] = new SelectList(_context.Members
                .Where(d => d.FirstName != "" && d.LastName != "")
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName), "ID", "FullName");

            var documents = from d in _context.MemberDocuments.Include(a => a.Member)
                            where d.Member.FirstName != "" && d.Member.LastName != ""
                            select d;            

            if (MemberID.HasValue)
            {
                documents = documents.Where(p => p.MemberID == MemberID);
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                documents = documents.Where(p => p.FileName.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            //Always sort by file name
            documents = documents.OrderBy(d => d.FileName);

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<MemberDocument>.CreateAsync(documents.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: MemberDocuments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id == null)
            {
                return NotFound();
            }

            var memberDocument = await _context.MemberDocuments
                .Include(a => a.Member)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (memberDocument == null)
            {
                return NotFound();
            }

            return View(memberDocument);
        }

        // POST: MemberDocuments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            var memberDocumentToUpdate = await _context.MemberDocuments
                .Include(a => a.Member)
                .FirstOrDefaultAsync(d => d.ID == id);

            //Check that you got it or exit with a not found error
            if (memberDocumentToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<MemberDocument>(memberDocumentToUpdate, "",
                p => p.FileName, p => p.Description))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberDocumentExists(memberDocumentToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    //Note: there is really no reason a delete should fail if you can "talk" to the database.
                    ModelState.AddModelError("", "Unable to edit file. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(memberDocumentToUpdate);
        }

        // GET: MemberDocuments/Delete/5

        public async Task<IActionResult> Delete(int? id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            if (id == null)
            {
                return NotFound();
            }

            var memberDocument = await _context.MemberDocuments
                .Include(a => a.Member)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (memberDocument == null)
            {
                return NotFound();
            }

            return View(memberDocument);
        }

        // POST: MemberDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //URL with the last filter, sort and page parameters for this controller
            ViewDataReturnURL();

            var memberDocument = await _context.MemberDocuments.FindAsync(id);
            try
            {
                _context.MemberDocuments.Remove(memberDocument);
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException)
            {
                //Note: there is really no reason a delete should fail if you can "talk" to the database.
                ModelState.AddModelError("", "Unable to delete file. Try again, and if the problem persists see your system administrator.");
            }
            return View(memberDocument);

        }

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.FileContent.MimeType, theFile.FileName);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private bool MemberDocumentExists(int id)
        {
            return _context.MemberDocuments.Any(e => e.ID == id);
        }
    }
}
