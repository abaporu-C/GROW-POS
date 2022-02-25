using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.ViewModels.ReportsViewModels;
using GROW_CRM.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.AspNetCore.Http.Features;
using System.IO;
using GROW_CRM.Controllers.Helpers;

namespace GROW_CRM.Controllers
{
    public class ReportsController : Controller
    {
        private readonly GROWContext _context;

        public ReportsController(GROWContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            GetReportsDDLItems();

            return View();
        }

        public async Task<IActionResult> SelectedReport(string Reports)
        {
            switch(Reports){
                case "0":
                    GetRenewalReport();
                    break;
                case "1":
                    GetNewAdditions();
                    break;
                case "2":
                    GetDemographics();
                    break;
                case "3":
                    GetMapping();
                    break;
                case "4":
                    GetIncomeInfo();
                    break;
                default:
                    return View("Index");                    
            }
            return View();
        }
        
        //Get Renewal Report
        public async void GetRenewalReport()
        {
            DateTime now = DateTime.Now;

            var renewalReports =  await _context.Households
                                .Include(h => h.Members)                                
                                .Select(rr => new RenewalReport
                                {
                                    ID = rr.ID,
                                    Members = rr.Members.Count,
                                    Income = rr.YearlyIncome,
                                    LastVerified = rr.LastVerification
                                }).ToListAsync();

            List<RenewalReport> renewalReportsFiltered = new List<RenewalReport>();

            foreach(RenewalReport r in renewalReports)
            {
                int diff = (now - r.LastVerified).Days;
                if (diff < 365) continue;
                renewalReportsFiltered.Add(r);
            }

            string[] headers = new string[] { "Household ID", "Number of Members", "Yearly Income", "Last Verification"};

            ViewData["ReportType"] = "Renewal Report";
            ViewData["Count"] = renewalReportsFiltered.Count();
            ViewData["Name"] = $"Households up for Reassessment";
            ViewBag.Headers = headers;
            ViewBag.Report = renewalReportsFiltered;

            GetReportsDDLItems();
        }

        public IActionResult DownloadRenewal()
        {
            DateTime now = DateTime.Now;

            var renewalReports = _context.Households
                                .Include(h => h.Members)
                                .Select(rr => new RenewalReport
                                {
                                    ID = rr.ID,
                                    Members = rr.Members.Count,
                                    Income = rr.YearlyIncome,
                                    LastVerified = rr.LastVerification
                                }).ToList();

            List<RenewalReport> renewalReportsFiltered = new List<RenewalReport>();

            foreach (RenewalReport r in renewalReports)
            {
                int diff = (now - r.LastVerified).Days;
                if (diff < 365) continue;
                renewalReportsFiltered.Add(r);
            }
            
            //How many rows?
            int reportRows = renewalReportsFiltered.Count();

            if (reportRows > 0) //We have data
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

                    var workSheet = excel.Workbook.Worksheets.Add("Households Up For Renewal");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(renewalReportsFiltered, true);

                    //Style fee column for currency
                    workSheet.Column(3).Style.Numberformat.Format = "###,##0.00";

                    //Style first column for dates
                    workSheet.Column(4).Style.Numberformat.Format = "yyyy-mm-dd";                    

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Patient Bold
                    workSheet.Cells[4, 1, reportRows + 3, 2].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totalfees = workSheet.Cells[reportRows + 4, 4])//
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[4, 3].Address + ":" + workSheet.Cells[reportRows + 3, 3].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "$###,##0.00";
                    }

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
                    //        ExcelComment cmd = Rng.AddComment(comment, "Apt. Notes");
                    //        cmd.AutoFit = true;
                    //    }
                    //}

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Renewal Report";
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
                            Response.Headers["content-disposition"] = "attachment;  filename=Appointments.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "Appointments.xlsx";
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

        public async void GetNewAdditions()
        {
            var newAdditions = await _context.Households
                                .Include(h => h.Members)
                                .Select(na => new NewAdditionsReport
                                {
                                    ID = na.ID,
                                    Members = na.Members.Count,
                                    Income = na.YearlyIncome,
                                    CreatedOn = na.CreatedOn,
                                    CreatedBy = na.CreatedBy
                                }).ToListAsync();

            List<NewAdditionsReport> newAdditionsfiltered = new List<NewAdditionsReport>();
            
            DateTime lastWeek = DateTime.Now.AddDays(-7);

            foreach(NewAdditionsReport na in newAdditions)
            {
                TimeSpan diff = (TimeSpan)(na.CreatedOn - lastWeek);
                double tds = diff.TotalDays;
                if (tds > 7) continue;
                newAdditionsfiltered.Add(na);
            }

            string[] headers = new string[] { "Household ID", "Number of Members", "Yearly Income", "Created On", "Created By" };

            ViewData["ReportType"] = "New Households Report";
            ViewData["Count"] = newAdditionsfiltered.Count();
            ViewData["Name"] = $"New Additions - From: {lastWeek.Month}/{lastWeek.Day}/{lastWeek.Year} To: {DateTime.Now.Month}/{DateTime.Now.Day}/{DateTime.Now.Year}";
            ViewBag.Headers = headers;
            ViewBag.Report = newAdditionsfiltered;

            GetReportsDDLItems();
        }

        public IActionResult DownloadNewMembers()
        {
            var newAdditions = _context.Households
                                .Include(h => h.Members)
                                .Select(na => new NewAdditionsReport
                                {
                                    ID = na.ID,
                                    Members = na.Members.Count,
                                    Income = na.YearlyIncome,
                                    CreatedOn = na.CreatedOn,
                                    CreatedBy = na.CreatedBy
                                }).ToList();

            List<NewAdditionsReport> newAdditionsfiltered = new List<NewAdditionsReport>();

            DateTime lastWeek = DateTime.Now.AddDays(-7);

            foreach (NewAdditionsReport na in newAdditions)
            {
                TimeSpan diff = (TimeSpan)(na.CreatedOn - lastWeek);

                if (diff.TotalDays > 7) continue;
                newAdditionsfiltered.Add(na);
            }
            //How many rows?
            int numRows = newAdditionsfiltered.Count();

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

                    var workSheet = excel.Workbook.Worksheets.Add("NewAdditionsReport");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(newAdditionsfiltered, true);

                    //Style first column for dates
                    workSheet.Column(3).Style.Numberformat.Format = "###,##0.00";

                    //Style fee column for currency
                    workSheet.Column(4).Style.Numberformat.Format = "yyyy-mm-dd";
                    workSheet.Column(5).Style.Numberformat.Format = "yyyy-mm-dd";

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Patient Bold
                    workSheet.Cells[4, 1, numRows + 3, 2].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totalfees = workSheet.Cells[numRows + 4, 4])//
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[4, 3].Address + ":" + workSheet.Cells[numRows + 3, 3].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "$###,##0.00";
                    }

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
                    //        ExcelComment cmd = Rng.AddComment(comment, "Apt. Notes");
                    //        cmd.AutoFit = true;
                    //    }
                    //}

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "New Aditions Report";
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
                            Response.Headers["content-disposition"] = "attachment;  filename=NewAdditionsReport.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "NewAdditionsReport.xlsx";
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

        public void GetDemographics()
        {
            var members = from m in _context.Members
                               .Include(m => m.Gender)
                                select m;

            int memberCount = members.Count();

            var data = ReportsHelper.GetDemoData(_context);

            var genderReport = data.ElementAt(0);
            var ageReport = data.ElementAt(1);
            var dietaryReport = data.ElementAt(2);

            ViewData["ReportType"] = "Demographics Report";
            ViewData["Count"] = memberCount;
            ViewData["Name"] = "Demographics";            
            ViewBag.GenderReport = genderReport;
            ViewBag.AgeReport = ageReport;
            ViewBag.DietaryReport = dietaryReport;

            GetReportsDDLItems();
        }

        public IActionResult DownloadDemographics()
        {
            //Get the appointments
            var members = from m in _context.Members
                               .Include(m => m.Gender)
                          select m;

            int memberCount = members.Count();

            var data = ReportsHelper.GetDemoData(_context);

            var genderReport = (List<GenderReport>)data.ElementAt(0);
            var ageReport = (List<AgeReport>)data.ElementAt(1);
            var dietaryReport = (List<DietaryReport>)data.ElementAt(2);

            //How many rows?
            int genderRows = genderReport.Count();
            int ageRows = ageReport.Count();
            int restrictionsRows = dietaryReport.Count();

            if (memberCount > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {
                    var workSheet = excel.Workbook.Worksheets.Add("Demographics");

                    workSheet.Cells[3, 1].Value = "GENDER REPORT";

                    //Note: Cells[row, column]
                    workSheet.Cells[4, 1].LoadFromCollection(genderReport, true);

                    //Style first column for dates
                    workSheet.Column(2).Style.Numberformat.Format = "0%";

                    //Style fee column for currency
                    workSheet.Column(3).Style.Numberformat.Format = "0";

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Patient Bold
                    workSheet.Cells[5, 1, genderRows + 4, 2].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totalfees = workSheet.Cells[genderRows + 5, 3])//
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[5, 3].Address + ":" + workSheet.Cells[genderRows + 4, 3].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "0";
                    }

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[4, 1, 4, 3])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    workSheet.Cells[genderRows + 7, 1].Value = "AGE REPORT";

                    workSheet.Cells[genderRows + 8, 1].LoadFromCollection(ageReport, true);

                    //workSheet.Cells[genderRows + 7, 1, ageRows + 3, 2].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totalfees = workSheet.Cells[genderRows + 9 + ageRows, 3])//
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[genderRows + 8, 3].Address + ":" + workSheet.Cells[genderRows + 8 + ageRows, 3].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "0";
                    }

                    using (ExcelRange headings = workSheet.Cells[genderRows + 7, 1, genderRows + 7, 3])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    workSheet.Cells[genderRows + 8 + ageRows + restrictionsRows, 1].Value = "DIETARY RESTRICTIONS REPORT";

                    workSheet.Cells[genderRows + 9 + ageRows + restrictionsRows, 1].LoadFromCollection(dietaryReport, true);

                    using (ExcelRange totalfees = workSheet.Cells[genderRows + 14 + ageRows + restrictionsRows, 3])//
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[genderRows + 13 + ageRows, 3].Address + ":" + workSheet.Cells[genderRows + 13 + ageRows + restrictionsRows, 3].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "0";
                    }

                    using (ExcelRange headings = workSheet.Cells[genderRows + 11 + ageRows, 1, genderRows + 11 + ageRows, 3])
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
                    workSheet.Cells[1, 1].Value = "Demographics Report";
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
                            Response.Headers["content-disposition"] = "attachment;  filename=Demographics.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "Demographics.xlsx";
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

        public void GetMapping()
        {
            var householdCount = _context.Households.Count();

            var citiesReport = _context.Households
                        .Include(h => h.City)
                        .GroupBy(h => new { h.City.Name })
                        .Select(grp => new CitiesReport
                        {
                            Name = grp.Key.Name,
                            Percentage = grp.Count(),
                            Total = grp.Count()
                        }).ToList();

            for(int i = 0; i < citiesReport.Count(); i++)
            {
                citiesReport[i].Percentage /= householdCount;
            }

            List<List<CityReport>> cityReports = new List<List<CityReport>>();

            var cities = _context.Cities.ToList();

            //This can get better
            //Tripple Loops are not a good idea
            foreach(City c in cities)
            {
                var h = _context.Members
                        .Include(h => h.MemberIncomeSituations)
                        .Include(h => h.Household).ThenInclude(hh => hh.City)
                        .Where(h => h.Household.City.Name == c.Name)
                        .GroupBy(h => new { h.Household.PostalCode, h.Household.City.Name })
                        .Select(grp => new CityReport
                        {
                            Name = grp.Key.Name,
                            PostalCode = grp.Key.PostalCode,
                            NumberOfMembers = grp.Count(),
                            TotalIncome = 0//grp.Sum(h => )
                        }).ToList();
                
                for(int i = 0; i < h.Count(); i++)
                {
                    CityReport cr = h.ElementAt(i);

                    var members = _context.Members
                                  .Include(m => m.MemberIncomeSituations)
                                  .Include(m => m.Household).ThenInclude(h => h.City)
                                  .Where(m => m.Household.City.Name == cr.Name && m.Household.PostalCode == cr.PostalCode)
                                  .Select(m => m).ToList();

                    double inc = 0;

                    foreach(Member m in members)
                    {
                        inc += m.YearlyIncome;
                    }

                    cr.TotalIncome = inc;
                }
                
                cityReports.Add(h);
            }

            ViewData["ReportType"] = "Mapping Report";
            ViewData["Count"] = citiesReport.Count();
            ViewData["Name"] = "Mapping";
            ViewBag.CitiesReport = citiesReport;
            ViewBag.CityReports = cityReports;

            GetReportsDDLItems();
        }

        public IActionResult DownloadMapping()
        {
            //Get the appointments
            var householdCount = _context.Households.Count();

            var citiesReport = _context.Households
                        .Include(h => h.City)
                        .GroupBy(h => new { h.City.Name })
                        .Select(grp => new CitiesReport
                        {
                            Name = grp.Key.Name,
                            Percentage = grp.Count(),
                            Total = grp.Count()
                        }).ToList();

            for (int i = 0; i < citiesReport.Count(); i++)
            {
                citiesReport[i].Percentage /= householdCount;
            }

            List<List<CityReport>> cityReports = new List<List<CityReport>>();

            var cities = _context.Cities.ToList();

            //This can get better
            //Tripple Loops are not a good idea
            foreach (City c in cities)
            {
                var h = _context.Members
                        .Include(h => h.MemberIncomeSituations)
                        .Include(h => h.Household).ThenInclude(hh => hh.City)
                        .Where(h => h.Household.City.Name == c.Name)
                        .GroupBy(h => new { h.Household.PostalCode, h.Household.City.Name })
                        .Select(grp => new CityReport
                        {
                            Name = grp.Key.Name,
                            PostalCode = grp.Key.PostalCode,
                            NumberOfMembers = grp.Count(),
                            TotalIncome = 0//grp.Sum(h => )
                        }).ToList();

                for (int i = 0; i < h.Count(); i++)
                {
                    CityReport cr = h.ElementAt(i);

                    var members = _context.Members
                                  .Include(m => m.MemberIncomeSituations)
                                  .Include(m => m.Household).ThenInclude(h => h.City)
                                  .Where(m => m.Household.City.Name == cr.Name && m.Household.PostalCode == cr.PostalCode)
                                  .Select(m => m).ToList();

                    double inc = 0;

                    foreach (Member m in members)
                    {
                        inc += m.YearlyIncome;
                    }

                    cr.TotalIncome = inc;
                }

                cityReports.Add(h);
            }
            //How many rows?
            int citiesRows = citiesReport.Count();

            if (householdCount > 0) //We have data
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

                    var workSheet = excel.Workbook.Worksheets.Add("Mapping");

                    workSheet.Cells[3, 1].Value = "CITIES REPORT";
                    //Note: Cells[row, column]
                    workSheet.Cells[4, 1].LoadFromCollection(citiesReport, true);

                    //Style first column for dates
                    workSheet.Cells[4,2,citiesRows + 4, 2].Style.Numberformat.Format = "0%";                                        

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Patient Bold
                    workSheet.Cells[4, 1, citiesRows + 4, 2].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange totalfees = workSheet.Cells[citiesRows + 5, 3])//
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[4, 3].Address + ":" + workSheet.Cells[citiesRows + 4, 3].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "0";
                    }

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[4, 1, 4, 7])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    int rows = citiesRows + 7;

                    foreach(List<CityReport> cr in cityReports)
                    {
                        int count = cr.Count();

                        if(count > 0)
                        {
                            workSheet.Cells[rows, 1].Value = $"{cr[0]?.Name.ToUpper()} REPORT";
                            //Note: Cells[row, column]
                            workSheet.Cells[rows + 1, 1].LoadFromCollection(cr, true);

                            //Style first column for dates
                            workSheet.Cells[rows + 1, 3, count + rows + 1, 3].Style.Numberformat.Format = "0";
                            workSheet.Cells[rows + 1, 4, count + rows + 1, 4].Style.Numberformat.Format = "$#,##0.00";

                            //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                            //Make Date and Patient Bold
                            workSheet.Cells[rows + 1, 1, count + rows + 1, 2].Style.Font.Bold = true;

                            using (ExcelRange totalfees = workSheet.Cells[count + rows + 2, 3])//
                            {
                                totalfees.Formula = "Sum(" + workSheet.Cells[rows + 1, 3].Address + ":" + workSheet.Cells[count + rows + 1, 3].Address + ")";
                                totalfees.Style.Font.Bold = true;
                                totalfees.Style.Numberformat.Format = "0";
                            }

                            using (ExcelRange totalfees = workSheet.Cells[count + rows + 2, 4])//
                            {
                                totalfees.Formula = "Sum(" + workSheet.Cells[rows + 1, 4].Address + ":" + workSheet.Cells[count + rows + 1, 4].Address + ")";
                                totalfees.Style.Font.Bold = true;
                                totalfees.Style.Numberformat.Format = "$#,##0.00";
                            }

                            //Set Style and backgound colour of headings
                            using (ExcelRange headings = workSheet.Cells[rows + 1, 1, rows + 1, 7])
                            {
                                headings.Style.Font.Bold = true;
                                var fill = headings.Style.Fill;
                                fill.PatternType = ExcelFillStyle.Solid;
                                fill.BackgroundColor.SetColor(Color.LightBlue);
                            }

                            rows += count + 5;
                        }                        
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
                    //        ExcelComment cmd = Rng.AddComment(comment, "Apt. Notes");
                    //        cmd.AutoFit = true;
                    //    }
                    //}

                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Mapping Report";
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
                            Response.Headers["content-disposition"] = "attachment;  filename=MappingReport.xlsx";
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.Body);
                        }
                    }
                    else
                    {
                        try
                        {
                            Byte[] theData = excel.GetAsByteArray();
                            string filename = "MappingReport.xlsx";
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

        public void GetIncomeInfo()
        {
            var sumQ = _context.Members
                .Include(m => m.Household).ThenInclude(m => m.Province)
                .Include(m => m.Gender)
                .AsEnumerable()
                .GroupBy(a => new { a.Household.ID, a.FirstName, a.LastName, a.FullName, a.Gender.Name, a.Age, a.Household.YearlyIncome })
                .Select(grp => new HouseholdInformation
                {
                    Code = grp.Key.ID,
                    Name = grp.Key.FullName,
                    Age = grp.Key.Age,
                    Gender = grp.Key.Name,
                    TotalIncome = (int)grp.Key.YearlyIncome
                });

            string[] headers = new string[] { "Household ID", "Member", "Age", "Gender", "Total Income" };

            ViewData["ReportType"] = "Income Report";
            ViewData["Count"] = sumQ.Count();
            ViewData["Name"] = $"Income Information Report";
            ViewBag.Headers = headers;
            ViewBag.Report = sumQ;

            GetReportsDDLItems();
        }

        public IActionResult DownloadIncomes()
        {
            //Get the placements
            var householdInfo = _context.Members
                        .Include(p => p.Household).ThenInclude(p => p.Province)
                        .Include(p => p.Gender)
                        .AsEnumerable()
                        .GroupBy(a => new { a.Household, a.Household.ID, a.FullName, a.Gender.Name, a.Age, a.Household.YearlyIncome })
                        .Select(grp => new
                        {
                            Code = grp.Key.ID,
                            Name = grp.Key.FullName,
                            Age = grp.Key.Age,
                            Gender = grp.Key.Name,
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

        public void GetReportsDDLItems()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "Renewal Report", Value = "0" });
            items.Add(new SelectListItem { Text = "Weekly Additions", Value = "1" });
            items.Add(new SelectListItem { Text = "Demographics", Value = "2" });
            items.Add(new SelectListItem { Text = "Mapping", Value = "3" });
            items.Add(new SelectListItem { Text = "Income Information", Value = "4" });

            ViewBag.Reports = items;
        }

        private bool HouseholdExists(int id)
        {
            return _context.Households.Any(e => e.ID == id);
        }

        [HttpGet]
        public async Task<ActionResult> GetDemoJson()
        {
            var demoData = ReportsHelper.GetDemoData(_context);

            return Json(demoData);
        }

        [HttpGet]
        public async Task<ActionResult> GetMapJson()
        {
            var mapData = ReportsHelper.GetMapData(_context);

            return Json(mapData);
        }
    }
}
