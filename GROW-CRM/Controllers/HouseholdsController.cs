using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.AspNetCore.Http.Features;
using System.IO;

namespace GROW_CRM.Controllers
{
    public class HouseholdsController : Controller
    {
        private readonly GROWContext _context;

        public HouseholdsController(GROWContext context)
        {
            _context = context;
        }

        // GET: Households
        public async Task<IActionResult> Index( string StreetSearch, string CitySearch, 
            int? HouseholdID, int? HouseholdStatusID,
            int? page, int? pageSizeID, string actionButton,
            string sortDirection = "asc", string sortField = "Code")
        {
            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = ""; //Asume not filtering

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "#","Street", "City", "Province", "Members", "LICO", "Status" };

            PopulateDropDownLists();



            var households = from h in _context.Households
                                .Include(h => h.HouseholdStatus)
                                .Include(h => h.City)
                                .Include(h => h.Province)
                                .Include(h => h.Members)

                             select h;

            //Add as many filters as needed
            if (HouseholdStatusID.HasValue)
            {
                households = households.Where(h => h.HouseholdStatusID == HouseholdStatusID);
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(StreetSearch))
            {
                households = households.Where(h => h.StreetName.ToUpper().Contains(StreetSearch.ToUpper())
                                       || h.City.Name.ToUpper().Contains(StreetSearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(CitySearch))
            {
                households = households.Where(h => h.StreetName.ToUpper().Contains(CitySearch.ToUpper())
                                       || h.City.Name.ToUpper().Contains(CitySearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }            


            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }


            if (sortField == "#")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.ID);
                }
                else
                {
                    households = households
                   .OrderByDescending(h => h.ID);
                }
            }
            else if (sortField == "Street")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.StreetName)
                    .ThenBy(h => h.StreetNumber);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.StreetName)
                    .ThenByDescending(h => h.StreetNumber);
                }
            }
            else if (sortField == "City")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.City.Name)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.City.Name)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Province")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.Province.Name)
                    .ThenBy(h => h.City.Name)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                    .OrderByDescending(h => h.Province.Name)
                    .ThenByDescending(h => h.City.Name)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Members")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.NumberOfMembers)
                    .ThenBy(h => h.City.Name);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.NumberOfMembers)
                    .ThenByDescending(h => h.City.Name);
                }
            }
            else if (sortField == "LICO")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.LICOVerified)
                    .ThenBy(h => h.City.Name)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.LICOVerified)
                    .ThenByDescending(h => h.City.Name)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.HouseholdStatus.Name)
                    .ThenBy(h => h.City.Name)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.HouseholdStatus.Name)
                    .ThenByDescending(h => h.City.Name)
                    .ThenByDescending(h => h.StreetName);
                }
            }



            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            return View(await households.ToListAsync());
        }

        // GET: Households/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.Members).ThenInclude(m => m.IncomeSituation)
                .Include(h => h.HouseholdStatus)
                .Include(h => h.City)
                .Include(h => h.Province)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (household == null)
            {
                return NotFound();
            }

            return View(household);
        }

        // GET: Households/Create
        public IActionResult Create()
        {
            var household = new Household();
            
            PopulateDropDownLists(household);
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StreetNumber,StreetName,AptNumber,PostalCode,LICOVerified,LastVerification,CityID,ProvinceID,HouseholdStatusID")] Household household, List<IFormFile> theFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(household, theFiles);
                    _context.Add(household);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "HouseholdMembers", new { HouseholdID = household.ID });
                }
            }
            catch (DbUpdateException)
            {

                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
           
           /* ViewData["HouseholdStatusID"] = new SelectList(_context.HouseholdStatuses, "ID", "ID", household.HouseholdStatusID);
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);*/
            return View(household);
        }

        // GET: Households/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.HouseholdStatus)
                .Include(h => h.City)
                .Include(h => h.Province)
                .FirstOrDefaultAsync(h => h.ID == id);

            if (household == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(household);
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {

            //Go get the Household to update

            var householdToUpdate = await _context.Households
                .Include(h => h.City)
                .Include(h => h.Province)
                .Include(h => h.HouseholdStatus)
                .SingleOrDefaultAsync(h => h.ID == id);

            //Check that you got it or exit with a not found error
            if (householdToUpdate == null)
            {
                return NotFound();
            }

            

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Household>(householdToUpdate, "",
                h => h.StreetName, h => h.StreetNumber, h => h.AptNumber, h => h.PostalCode,
                h => h.LICOVerified, h => h.LastVerification, h => h.CityID,
                h => h.ProvinceID, h => h.HouseholdStatusID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "HouseholdMembers", new { HouseholdID = householdToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HouseholdExists(householdToUpdate.ID))
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
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }


            PopulateDropDownLists(householdToUpdate);
            return View(householdToUpdate);
        }

        // GET: Households/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.HouseholdStatus)
                .Include(h => h.City)
                .Include(h => h.Province)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (household == null)
            {
                return NotFound();
            }

            return View(household);
        }

        // POST: Households/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var household = await _context.Households.FindAsync(id);
            _context.Households.Remove(household);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HouseholdExists(int id)
        {
            return _context.Households.Any(e => e.ID == id);
        }

        public IActionResult DownloadYearlyReassessments()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .Include(p => p.Household).ThenInclude(p => p.Province)
                        .Include(p => p.DietaryRestrictionMembers).ThenInclude(p => p.DietaryRestriction)
                        .Include(p => p.Gender)
                        .Include(p => p.IncomeSituation)
                        .AsEnumerable()
                        .GroupBy(a => new { a.Household })
                        .Select(grp => new
                        {
                            
                        });

            //How many rows?
            int numRows = householdInfo.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    //Note: you can also pull a spreadsheet out of the database if you
                    //have saved it in the normal way we do, as a Byte Array in a Model
                    //such as the UploadedFile class.
                    //
                    // Suppose...
                    //
                    // var theSpreadsheet = _context.UploadedFiles.Include(f => f.FileContent).Where(f => f.ID == id).SingleOrDefault();
                    //
                    //    //Pass the Byte[] FileContent to a MemoryStream
                    //
                    // using (MemoryStream memStream = new MemoryStream(theSpreadsheet.FileContent.Content))
                    // {
                    //     ExcelPackage package = new ExcelPackage(memStream);
                    // }

                    var workSheet = excel.Workbook.Worksheets.Add("Household Information");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(householdInfo, true);

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Athlete Bold
                    workSheet.Cells[4, 1, numRows + 2, 1].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totals = workSheet.Cells[numRows + 6, 2])
                    {
                        totals.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
                        totals.Style.Font.Bold = true;
                    }
                    workSheet.Cells[numRows + 5, 1].Value = "Total Number of Athletes";
                    workSheet.Cells[numRows + 5, 1].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 5, 2].Value = numRows;
                    workSheet.Cells[numRows + 5, 2].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 6, 1].Value = "Total Number of Events";
                    workSheet.Cells[numRows + 6, 1].Style.Font.Bold = true;

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 7])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    ////Boy those notes are BIG!
                    ////Lets put them in comments instead.
                    for (int i = 4; i < numRows + 4; i++)
                    {
                        using (ExcelRange Rng = workSheet.Cells[i, 7])
                        {
                            string[] commentWords = Rng.Value.ToString().Split(' ');
                            Rng.Value = commentWords[0] + "...";
                            //This LINQ adds a newline every 7 words
                            string comment = string.Join(Environment.NewLine, commentWords
                                .Select((word, index) => new { word, index })
                                .GroupBy(x => x.index / 7)
                                .Select(grp => string.Join(" ", grp.Select(x => x.word))));
                            ExcelComment cmd = Rng.AddComment(comment, "Dietary Restrictions");
                            cmd.AutoFit = true;
                        }
                    }

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Household Report";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 7])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 7])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    //I usually stream the response back to avoid possible
                    //out of memory errors on the server if you have a large spreadsheet.
                    //NOTE: Since .NET Core 3 most Web Servers disallow sync IO so we
                    //need to temporarily change the setting for the server.
                    //If we can't then we will try to build the file and return a FileContentResult
                    var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                        using (var memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.Headers["content-disposition"] = "attachment;  filename=HouseholdReport.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "HouseholdReport.xlsx";
                            string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            return File(theData, mimeType, filename);
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return NotFound();
        }


        public IActionResult DownloadHouseholds()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .Include(p => p.Household).ThenInclude(p => p.Province)
                        .Include(p => p.DietaryRestrictionMembers).ThenInclude(p => p.DietaryRestriction)
                        .Include(p => p.Gender)
                        .Include(p => p.IncomeSituation)
                        .AsEnumerable()
                        .GroupBy(a => new { a.Household, a.FullName, a.Gender.Name, a.Age, a.Household.PostalCode, a.DietaryRestrictionMembers, a.IncomeSituation.Situation, a.Household.YearlyIncome })
                        .Select(grp => new
                        {
                            Name = grp.Key.FullName,
                            Gender = grp.Key.Name,
                            Age = grp.Key.Age,
                            Postal_Code = grp.Key.PostalCode,
                            Total_Household_Income = grp.Key.YearlyIncome,
                            Income_Source = grp.Key.Situation,
                            Dietary_Restrictions = grp.Key.DietaryRestrictionMembers
                        });

            //How many rows?
            int numRows = householdInfo.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    //Note: you can also pull a spreadsheet out of the database if you
                    //have saved it in the normal way we do, as a Byte Array in a Model
                    //such as the UploadedFile class.
                    //
                    // Suppose...
                    //
                    // var theSpreadsheet = _context.UploadedFiles.Include(f => f.FileContent).Where(f => f.ID == id).SingleOrDefault();
                    //
                    //    //Pass the Byte[] FileContent to a MemoryStream
                    //
                    // using (MemoryStream memStream = new MemoryStream(theSpreadsheet.FileContent.Content))
                    // {
                    //     ExcelPackage package = new ExcelPackage(memStream);
                    // }

                    var workSheet = excel.Workbook.Worksheets.Add("Household Information");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(householdInfo, true);

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Athlete Bold
                    workSheet.Cells[4, 1, numRows + 2, 1].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totals = workSheet.Cells[numRows + 6, 2])
                    {
                        totals.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
                        totals.Style.Font.Bold = true;
                    }
                    workSheet.Cells[numRows + 5, 1].Value = "Total Number of Athletes";
                    workSheet.Cells[numRows + 5, 1].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 5, 2].Value = numRows;
                    workSheet.Cells[numRows + 5, 2].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 6, 1].Value = "Total Number of Events";
                    workSheet.Cells[numRows + 6, 1].Style.Font.Bold = true;

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 7])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    ////Boy those notes are BIG!
                    ////Lets put them in comments instead.
                    for (int i = 4; i < numRows + 4; i++)
                    {
                        using (ExcelRange Rng = workSheet.Cells[i, 7])
                        {
                            string[] commentWords = Rng.Value.ToString().Split(' ');
                            Rng.Value = commentWords[0] + "...";
                            //This LINQ adds a newline every 7 words
                            string comment = string.Join(Environment.NewLine, commentWords
                                .Select((word, index) => new { word, index })
                                .GroupBy(x => x.index / 7)
                                .Select(grp => string.Join(" ", grp.Select(x => x.word))));
                            ExcelComment cmd = Rng.AddComment(comment, "Dietary Restrictions");
                            cmd.AutoFit = true;
                        }
                    }

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Household Report";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 7])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 7])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    //I usually stream the response back to avoid possible
                    //out of memory errors on the server if you have a large spreadsheet.
                    //NOTE: Since .NET Core 3 most Web Servers disallow sync IO so we
                    //need to temporarily change the setting for the server.
                    //If we can't then we will try to build the file and return a FileContentResult
                    var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                        using (var memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.Headers["content-disposition"] = "attachment;  filename=HouseholdReport.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "HouseholdReport.xlsx";
                            string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            return File(theData, mimeType, filename);
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return NotFound();
        }

        public IActionResult DownloadAges()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .AsEnumerable()
                        .GroupBy(a => new { a.Household, a.Age })
                        .Select(grp => new
                        {
                            Children = grp.Key.Age,
                            Teenagers = grp.Key.Age,
                            Adults = grp.Key.Age,
                            Seniors = grp.Key.Age
                        });

            //How many rows?
            int numRows = householdInfo.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    //Note: you can also pull a spreadsheet out of the database if you
                    //have saved it in the normal way we do, as a Byte Array in a Model
                    //such as the UploadedFile class.
                    //
                    // Suppose...
                    //
                    // var theSpreadsheet = _context.UploadedFiles.Include(f => f.FileContent).Where(f => f.ID == id).SingleOrDefault();
                    //
                    //    //Pass the Byte[] FileContent to a MemoryStream
                    //
                    // using (MemoryStream memStream = new MemoryStream(theSpreadsheet.FileContent.Content))
                    // {
                    //     ExcelPackage package = new ExcelPackage(memStream);
                    // }

                    var workSheet = excel.Workbook.Worksheets.Add("Total Age Groups");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(householdInfo, true);

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Athlete Bold
                    workSheet.Cells[4, 1, numRows + 2, 1].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totals = workSheet.Cells[numRows + 6, 2])
                    {
                        totals.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
                        totals.Style.Font.Bold = true;
                    }
                    workSheet.Cells[numRows + 5, 1].Value = "Total Number of Athletes";
                    workSheet.Cells[numRows + 5, 1].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 5, 2].Value = numRows;
                    workSheet.Cells[numRows + 5, 2].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 6, 1].Value = "Total Number of Events";
                    workSheet.Cells[numRows + 6, 1].Style.Font.Bold = true;

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 7])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Total Age Groups";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 7])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 7])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    //I usually stream the response back to avoid possible
                    //out of memory errors on the server if you have a large spreadsheet.
                    //NOTE: Since .NET Core 3 most Web Servers disallow sync IO so we
                    //need to temporarily change the setting for the server.
                    //If we can't then we will try to build the file and return a FileContentResult
                    var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                        using (var memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.Headers["content-disposition"] = "attachment;  filename=TotalAgeGroups.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "TotalAgeGroups.xlsx";
                            string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            return File(theData, mimeType, filename);
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return NotFound();
        }

        public IActionResult DownloadGenders()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .Include(p => p.Gender)
                        .AsEnumerable()
                        .GroupBy(a => new { a.Household, a.Gender.Name })
                        .Select(grp => new
                        {
                            Male = grp.Key.Name,
                            Female = grp.Key.Name,
                            Non_Binary = grp.Key.Name,
                            Other = grp.Key.Name,
                            Prefer_Not_To_Say = grp.Key.Name
                        });

            //How many rows?
            int numRows = householdInfo.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    //Note: you can also pull a spreadsheet out of the database if you
                    //have saved it in the normal way we do, as a Byte Array in a Model
                    //such as the UploadedFile class.
                    //
                    // Suppose...
                    //
                    // var theSpreadsheet = _context.UploadedFiles.Include(f => f.FileContent).Where(f => f.ID == id).SingleOrDefault();
                    //
                    //    //Pass the Byte[] FileContent to a MemoryStream
                    //
                    // using (MemoryStream memStream = new MemoryStream(theSpreadsheet.FileContent.Content))
                    // {
                    //     ExcelPackage package = new ExcelPackage(memStream);
                    // }

                    var workSheet = excel.Workbook.Worksheets.Add("Gender Totals");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(householdInfo, true);

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Athlete Bold
                    workSheet.Cells[4, 1, numRows + 2, 1].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totals = workSheet.Cells[numRows + 6, 2])
                    {
                        totals.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
                        totals.Style.Font.Bold = true;
                    }
                    workSheet.Cells[numRows + 5, 1].Value = "Total Number of Athletes";
                    workSheet.Cells[numRows + 5, 1].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 5, 2].Value = numRows;
                    workSheet.Cells[numRows + 5, 2].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 6, 1].Value = "Total Number of Events";
                    workSheet.Cells[numRows + 6, 1].Style.Font.Bold = true;

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 7])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Gender Totals";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 7])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 7])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    //I usually stream the response back to avoid possible
                    //out of memory errors on the server if you have a large spreadsheet.
                    //NOTE: Since .NET Core 3 most Web Servers disallow sync IO so we
                    //need to temporarily change the setting for the server.
                    //If we can't then we will try to build the file and return a FileContentResult
                    var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                        using (var memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.Headers["content-disposition"] = "attachment;  filename=GenderTotals.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "GenderTotals.xlsx";
                            string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            return File(theData, mimeType, filename);
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return NotFound();
        }

        public IActionResult DownloadIncomeSources()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .Include(p => p.Household)
                        .Include(p => p.IncomeSituation)
                        .AsEnumerable()
                        .GroupBy(a => new { a.Household, a.IncomeSituation.Situation })
                        .Select(grp => new
                        {
                            ODSP = grp.Key.Situation.Equals("ODSP"),
                            Ontario_Works = grp.Key.Situation.Equals("Ontario Works"),
                            CPP_Disability = grp.Key.Situation.Equals("CPP-Disability"),
                            EI = grp.Key.Situation.Equals("EI"),
                            GAINS = grp.Key.Situation.Equals("GAINS"),
                            Post_Sec_Student = grp.Key.Situation.Equals("Post. Sec. Student"),
                            Other = grp.Key.Situation.Equals("Other"),
                            Volunteer = grp.Key.Situation.Equals("Volunteer")
                        });

            //How many rows?
            int numRows = householdInfo.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    //Note: you can also pull a spreadsheet out of the database if you
                    //have saved it in the normal way we do, as a Byte Array in a Model
                    //such as the UploadedFile class.
                    //
                    // Suppose...
                    //
                    // var theSpreadsheet = _context.UploadedFiles.Include(f => f.FileContent).Where(f => f.ID == id).SingleOrDefault();
                    //
                    //    //Pass the Byte[] FileContent to a MemoryStream
                    //
                    // using (MemoryStream memStream = new MemoryStream(theSpreadsheet.FileContent.Content))
                    // {
                    //     ExcelPackage package = new ExcelPackage(memStream);
                    // }

                    var workSheet = excel.Workbook.Worksheets.Add("Income Source Totals");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(householdInfo, true);

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Athlete Bold
                    workSheet.Cells[4, 1, numRows + 2, 1].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totals = workSheet.Cells[numRows + 6, 2])
                    {
                        totals.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
                        totals.Style.Font.Bold = true;
                    }
                    workSheet.Cells[numRows + 5, 1].Value = "Total Number of Athletes";
                    workSheet.Cells[numRows + 5, 1].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 5, 2].Value = numRows;
                    workSheet.Cells[numRows + 5, 2].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 6, 1].Value = "Total Number of Events";
                    workSheet.Cells[numRows + 6, 1].Style.Font.Bold = true;

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 8])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Income Source Totals";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 8])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 7])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    //I usually stream the response back to avoid possible
                    //out of memory errors on the server if you have a large spreadsheet.
                    //NOTE: Since .NET Core 3 most Web Servers disallow sync IO so we
                    //need to temporarily change the setting for the server.
                    //If we can't then we will try to build the file and return a FileContentResult
                    var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                        using (var memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.Headers["content-disposition"] = "attachment;  filename=IncomeSourceTotals.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "IncomeSourceTotals.xlsx";
                            string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            return File(theData, mimeType, filename);
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return NotFound();
        }

        public IActionResult DownloadPostalCodes()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .Include(p => p.Household).ThenInclude(p => p.PostalCode)
                        .AsEnumerable()
                        .GroupBy(a => new { a.Household, a.Household.PostalCode })
                        .Select(grp => new
                        {
                            
                        });

            //How many rows?
            int numRows = householdInfo.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    //Note: you can also pull a spreadsheet out of the database if you
                    //have saved it in the normal way we do, as a Byte Array in a Model
                    //such as the UploadedFile class.
                    //
                    // Suppose...
                    //
                    // var theSpreadsheet = _context.UploadedFiles.Include(f => f.FileContent).Where(f => f.ID == id).SingleOrDefault();
                    //
                    //    //Pass the Byte[] FileContent to a MemoryStream
                    //
                    // using (MemoryStream memStream = new MemoryStream(theSpreadsheet.FileContent.Content))
                    // {
                    //     ExcelPackage package = new ExcelPackage(memStream);
                    // }

                    var workSheet = excel.Workbook.Worksheets.Add("Income Source Totals");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(householdInfo, true);

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Athlete Bold
                    workSheet.Cells[4, 1, numRows + 2, 1].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totals = workSheet.Cells[numRows + 6, 2])
                    {
                        totals.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
                        totals.Style.Font.Bold = true;
                    }
                    workSheet.Cells[numRows + 5, 1].Value = "Total Number of Athletes";
                    workSheet.Cells[numRows + 5, 1].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 5, 2].Value = numRows;
                    workSheet.Cells[numRows + 5, 2].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 6, 1].Value = "Total Number of Events";
                    workSheet.Cells[numRows + 6, 1].Style.Font.Bold = true;

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 8])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Income Source Totals";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 8])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 7])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    //I usually stream the response back to avoid possible
                    //out of memory errors on the server if you have a large spreadsheet.
                    //NOTE: Since .NET Core 3 most Web Servers disallow sync IO so we
                    //need to temporarily change the setting for the server.
                    //If we can't then we will try to build the file and return a FileContentResult
                    var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                        using (var memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.Headers["content-disposition"] = "attachment;  filename=IncomeSourceTotals.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "IncomeSourceTotals.xlsx";
                            string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            return File(theData, mimeType, filename);
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return NotFound();
        }

        public IActionResult DownloadHouseholdIncomes()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .Include(p => p.Household)
                        .AsEnumerable()
                        .GroupBy(a => new { a.Household, a.Household.YearlyIncome, a.Household.FullAddress })
                        .Select(grp => new
                        {
                            Household = grp.Key.FullAddress,
                            Yearly_Income = grp.Key.YearlyIncome
                        });

            //How many rows?
            int numRows = householdInfo.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    //Note: you can also pull a spreadsheet out of the database if you
                    //have saved it in the normal way we do, as a Byte Array in a Model
                    //such as the UploadedFile class.
                    //
                    // Suppose...
                    //
                    // var theSpreadsheet = _context.UploadedFiles.Include(f => f.FileContent).Where(f => f.ID == id).SingleOrDefault();
                    //
                    //    //Pass the Byte[] FileContent to a MemoryStream
                    //
                    // using (MemoryStream memStream = new MemoryStream(theSpreadsheet.FileContent.Content))
                    // {
                    //     ExcelPackage package = new ExcelPackage(memStream);
                    // }

                    var workSheet = excel.Workbook.Worksheets.Add("Yearly Income");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(householdInfo, true);

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Athlete Bold
                    workSheet.Cells[4, 1, numRows + 2, 1].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totals = workSheet.Cells[numRows + 6, 2])
                    {
                        totals.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
                        totals.Style.Font.Bold = true;
                    }
                    workSheet.Cells[numRows + 5, 1].Value = "Total Number of Athletes";
                    workSheet.Cells[numRows + 5, 1].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 5, 2].Value = numRows;
                    workSheet.Cells[numRows + 5, 2].Style.Font.Bold = true;
                    workSheet.Cells[numRows + 6, 1].Value = "Total Number of Events";
                    workSheet.Cells[numRows + 6, 1].Style.Font.Bold = true;

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 8])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Yearly Income";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 8])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 7])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    //I usually stream the response back to avoid possible
                    //out of memory errors on the server if you have a large spreadsheet.
                    //NOTE: Since .NET Core 3 most Web Servers disallow sync IO so we
                    //need to temporarily change the setting for the server.
                    //If we can't then we will try to build the file and return a FileContentResult
                    var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
                    if (syncIOFeature != null)
                    {
                        syncIOFeature.AllowSynchronousIO = true;
                        using (var memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.Headers["content-disposition"] = "attachment;  filename=YearlyIncome.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "YearlyIncome.xlsx";
                            string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            return File(theData, mimeType, filename);
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return NotFound();
        }

    }
}
