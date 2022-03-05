using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using GROW_CRM.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.AspNetCore.Http.Features;

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
        public async Task<IActionResult> Index( string StreetSearch, string CitySearch, string CodeSearch,
            string HouseholdCodeSearch,
            int? HouseholdID, int? HouseholdStatusID,
            int? page, int? pageSizeID, string actionButton,
            string sortDirection = "asc", string sortField = "Code")
        {
            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = ""; //Asume not filtering

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Code","Street", "City", "Province", "Members", "LICO", "Status" };

            PopulateDropDownLists();



            var households = from h in _context.Households
                                .Include(h => h.HouseholdStatus)
                                .Include(h => h.Province)

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
                                       || h.City.ToUpper().Contains(StreetSearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(CitySearch))
            {
                households = households.Where(h => h.StreetName.ToUpper().Contains(CitySearch.ToUpper())
                                       || h.City.ToUpper().Contains(CitySearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(CodeSearch))
            {
                households = households.Where(p => p.HouseholdCode.Contains(CodeSearch));
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


            if (sortField == "Code")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.HouseholdCode);
                }
                else
                {
                    households = households
                   .OrderByDescending(h => h.HouseholdCode);
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
                    .OrderBy(h => h.City)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.City)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Province")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.Province.Name)
                    .ThenBy(h => h.City)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                    .OrderByDescending(h => h.Province.Name)
                    .ThenByDescending(h => h.City)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Members")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.NumberOfMembers)
                    .ThenBy(h => h.City);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.NumberOfMembers)
                    .ThenByDescending(h => h.City);
                }
            }
            else if (sortField == "LICO")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.LICOVerified)
                    .ThenBy(h => h.City)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.LICOVerified)
                    .ThenByDescending(h => h.City)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.HouseholdStatus.Name)
                    .ThenBy(h => h.City)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.HouseholdStatus.Name)
                    .ThenByDescending(h => h.City)
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
        public async Task<IActionResult> Create([Bind("ID,StreetNumber,StreetName,AptNumber,City,PostalCode,HouseholdCode,YearlyIncome,NumberOfMembers,LICOVerified,JoinedDate,ProvinceID,HouseholdStatusID")] Household household, List<IFormFile> theFiles)
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
                h => h.StreetName, h => h.StreetNumber, h => h.AptNumber, h => h.City, h => h.PostalCode,
                h => h.HouseholdCode, h => h.YearlyIncome, h => h.LICOVerified, h => h.NumberOfMembers, h => h.JoinedDate,
                h => h.ProvinceID, h => h.HouseholdStatusID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { householdToUpdate.ID });
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

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.FileContent.MimeType, theFile.FileName);
        }

        public IActionResult HouseholdIncomeInformation()
        {
            var sumQ = _context.Members
                .Include(m => m.Household).ThenInclude(m => m.Province)
                .Include(m => m.Gender)
                .Include( m=> m.IncomeSituation)
                .AsEnumerable()
                .OrderBy(m => m.Household.HCode).ThenByDescending(m => m.FirstName)
                .GroupBy(a => new { a.Household.HCode, a.FirstName, a.LastName, a.FullName, a.Gender.Name, a.Age, a.IncomeSituation.Situation, a.Household.YearlyIncome })
                .Select(grp => new HouseholdInformation
                {
                    Code = grp.Key.HCode,
                    Name = grp.Key.FullName,
                    Age = grp.Key.Age,
                    Gender = grp.Key.Name,
                    IncomeSource = grp.Key.Situation,
                    TotalIncome = (int)grp.Key.YearlyIncome
                });

            return View(sumQ.ToList());
        }

        public IActionResult YearlyAssessment()
        {
            var sumQ = _context.Members
                .Include(m => m.Household).ThenInclude(m => m.Province)
                .AsEnumerable()
                .OrderBy(m => m.Household.HCode).ThenByDescending(m => m.FirstName)
                .GroupBy(a => new { a.Household })
                .Select(grp => new YearlyReportVM
                {
                    Code = grp.Key.Household.HCode,
                    Members = grp.Key.Household.NumberOfMembers.ToString(),
                    Address = grp.Key.Household.FullAddress,
                    PendingReassessment = grp.Key.Household.JoinedDate.Equals(DateTime.Now).ToString()
                });

            return View(sumQ.ToList());
        }

        public IActionResult DownloadIncomes()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .Include(p => p.Household).ThenInclude(p => p.Province)
                        .Include(p => p.Gender)
                        .Include(p => p.IncomeSituation)
                        .AsEnumerable()
                        .OrderBy(m => m.Household.HCode).ThenByDescending(m => m.FirstName)
                        .GroupBy(a => new { a.Household, a.Household.HCode, a.FullName, a.Gender.Name, a.Age, a.IncomeSituation.Situation, a.Household.YearlyIncome })
                        .Select(grp => new
                        {
                            Code = grp.Key.HCode,
                            Name = grp.Key.FullName,
                            Age = grp.Key.Age,
                            Gender = grp.Key.Name,
                            Income_Source = grp.Key.Situation,
                            Total_Income = (int)grp.Key.YearlyIncome
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

                    var workSheet = excel.Workbook.Worksheets.Add("Household Income Information");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(householdInfo, true);

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Athlete Bold
                    workSheet.Cells[4, 1, numRows + 2, 1].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    //using (ExcelRange totals = workSheet.Cells[numRows + 6, 2])
                    //{
                    //    totals.Formula = "Sum(" + workSheet.Cells[4, 6].Address + ":" + workSheet.Cells[numRows + 3, 6].Address + ")";
                    //    totals.Style.Font.Bold = true;
                    //}
                    //workSheet.Cells[numRows + 5, 1].Value = "Total Number of Athletes";
                    //workSheet.Cells[numRows + 5, 1].Style.Font.Bold = true;
                    //workSheet.Cells[numRows + 5, 2].Value = numRows;
                    //workSheet.Cells[numRows + 5, 2].Style.Font.Bold = true;
                    //workSheet.Cells[numRows + 6, 1].Value = "Total Number of Events";
                    //workSheet.Cells[numRows + 6, 1].Style.Font.Bold = true;

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 6])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    ////Boy those notes are BIG!
                    ////Lets put them in comments instead.
                    //for (int i = 4; i < numRows + 4; i++)
                    //{
                    //    using (ExcelRange Rng = workSheet.Cells[i, 7])
                    //    {
                    //        string[] commentWords = Rng.Value.ToString().Split(' ');
                    //        Rng.Value = commentWords[0] + "...";
                    //        //This LINQ adds a newline every 7 words
                    //        string comment = string.Join(Environment.NewLine, commentWords
                    //            .Select((word, index) => new { word, index })
                    //            .GroupBy(x => x.index / 7)
                    //            .Select(grp => string.Join(" ", grp.Select(x => x.word))));
                    //        ExcelComment cmd = Rng.AddComment(comment, "Dietary Restrictions");
                    //        cmd.AutoFit = true;
                    //    }
                    //}

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Household Income Report";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 6])
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
                    using (ExcelRange Rng = workSheet.Cells[2, 6])
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
                            Response.Headers["content-disposition"] = "attachment;  filename=HouseholdIncomeReport.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "HouseholdIncomeReport.xlsx";
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

        public IActionResult SalesReceipt()
        {
            var sumQ = _context.Members
                .Include(m => m.Household)
                .Include(m => m.Order).ThenInclude(m => m.OrderItem)
                .AsEnumerable()
                .OrderBy(m => m.Order.Date) //order by purchase date
                .GroupBy(a => new { a.Household, a.Order, a.Order.OrderItem, a.Order.OrderItem.Item })
                .Select(grp => new Sales
                {
                    Household = grp.Key.Household.HCode,
                    PurchaseDate = grp.Key.Order.Date, //purchase date
                    Purchases = grp.Key.Item.Name + " (" + grp.Key.OrderItem.Quantity.ToString() + ")", //purchases and quantity
                    Taxes = grp.Key.Order.Taxes,
                    Total = grp.Key.Order.Total, //purchase total
                    Volunteer = grp.Key.Household.Members.ToString() //volunteer
                });

            return View(sumQ.ToList());
        }

        public IActionResult DownloadSalesReceipt()
        {
            //Get the placements
            var householdInfo = _context.Members
                .Include(m => m.Household)
                .Include(m => m.Order).ThenInclude(m => m.OrderItem)
                .AsEnumerable()
                .OrderBy(m => m.Order.Date) //order by purchase date
                .GroupBy(a => new { a.Household, a.Order, a.Order.OrderItem, a.Order.OrderItem.Item })
                .Select(grp => new Sales
                {
                    Household = grp.Key.Household.HCode,
                    PurchaseDate = grp.Key.Order.Date, //purchase date
                    Purchases = grp.Key.Item.Name + " (" + grp.Key.OrderItem.Quantity.ToString() + ")", //purchases and quantity
                    Taxes = grp.Key.Order.Taxes,
                    Total = grp.Key.Order.Total, //purchase total
                    Volunteer = grp.Key.Household.Members.ToString() //volunteer
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

                    var workSheet = excel.Workbook.Worksheets.Add("Household Income Information");

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
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 6])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    ////Boy those notes are BIG!
                    ////Lets put them in comments instead.
                    ///use for product and quantity
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
                    workSheet.Cells[1, 1].Value = "Household Income Report";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 6])
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
                    using (ExcelRange Rng = workSheet.Cells[2, 6])
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
                            Response.Headers["content-disposition"] = "attachment;  filename=HouseholdIncomeReport.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "HouseholdIncomeReport.xlsx";
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


        private async Task AddDocumentsAsync(Household household, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    //Note: you could filter for mime types if you only want to allow
                    //certain types of files.  I am allowing everything.
                    if (!(fileName == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        HouseholdDocument d = new HouseholdDocument();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.FileContent.MimeType = mimeType;
                        d.FileName = fileName;
                        household.HouseholdDocuments.Add(d);
                    };
                }
            }
        }
        private SelectList ProvinceSelectList(int? selectedId)
        {
            return new SelectList(_context.Provinces
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private SelectList HouseholdStatusSelectList(int? selectedId)
        {
            return new SelectList(_context.HouseholdStatuses
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(Household household = null)
        {
            ViewData["ProvinceID"] = ProvinceSelectList(household?.ProvinceID);
            ViewData["HouseholdStatusID"] = HouseholdStatusSelectList(household?.HouseholdStatusID);
           
        }
    }
}
