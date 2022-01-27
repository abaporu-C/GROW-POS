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
        public async Task<IActionResult> Index()
        {
            var gROWContext = _context.Households.Include(h => h.Province);
            return View(await gROWContext.ToListAsync());
        }

        // GET: Households/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
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
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID");
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StreetNumber,StreetName,AptNumber,City,PostalCode,YearlyIncome,NumberOfMembers,LICOVerified,ProvinceID")] Household household)
        {
            if (ModelState.IsValid)
            {
                _context.Add(household);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);
            return View(household);
        }

        // GET: Households/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households.FindAsync(id);
            if (household == null)
            {
                return NotFound();
            }
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,StreetNumber,StreetName,AptNumber,City,PostalCode,YearlyIncome,NumberOfMembers,LICOVerified,ProvinceID")] Household household)
        {
            if (id != household.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(household);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HouseholdExists(household.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);
            return View(household);
        }

        // GET: Households/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
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
