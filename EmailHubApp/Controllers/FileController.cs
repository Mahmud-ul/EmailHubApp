using AutoMapper;
using Azure;
using Azure.Core;
using EmailHubApp.Database;
using EmailHubApp.Manager;
using EmailHubApp.Manager.Contract;
using EmailHubApp.Model.Models;
using EmailHubApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OfficeOpenXml;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
//using Excel = Microsoft.Office.Interop.Excel;

namespace EmailHubApp.Controllers
{
    public class FileController : Controller
    {
        #region Initialization
        private readonly EmailHubDB _db;
        private readonly IMapper _iMapper;
        private readonly IUserManager _iUserManager;
        private readonly ISearchQueryManager _iSearchQueryManager;
        private readonly ISearchedDataManager _iSearchedDataManager;
        private readonly ISearchRequirementsManager _iSearchRequirementsManager;
        public FileController(IMapper iMapper, IUserManager iUserManager, ISearchQueryManager iSearchQueryManager, ISearchedDataManager iSearchedDataManager, ISearchRequirementsManager iSearchRequirementsManager)
        {
            _db = new EmailHubDB();
            _iMapper = iMapper;
            _iUserManager = iUserManager;
            _iSearchQueryManager = iSearchQueryManager;
            _iSearchedDataManager = iSearchedDataManager;
            _iSearchRequirementsManager = iSearchRequirementsManager;
        }
        #endregion

        public IActionResult Index()
        { 
            try
            {
                #region Login Check
                if (HttpContext.Session.GetString("UserID") == "" || HttpContext.Session.GetString("UserID") == null)
                    return RedirectToAction("Login", "User");
                #endregion

                if(TempData["LimitEnd"]!= null)
                {
                    ViewBag.ErrorMessage = "You have reached your search limit. Please Upgrade your membership!";

                    if (TempData.TryGetValue("ResultData", out object? dataListObj))
                    {
                        if(dataListObj != null && dataListObj is string[] dataArray)
                        {
                            List<string> dataList = dataArray.ToList();
                            ViewBag.DataList = dataList;
                        }
                    }
                }
                else if (TempData["ResultData"] == null)
                {
                    ViewBag.ErrorMessage = "No Result Data Found!";
                }
                else if(TempData.TryGetValue("ResultData", out object? dataListObj))
                {
                    if (dataListObj != null && dataListObj is string[] dataArray)
                    {
                        List<string> dataList = dataArray.ToList();
                        ViewBag.DataList = dataList;
                    }
                    else
                    {
                        return BadRequest("No Data Found");
                    }
                }
                return View();
            }
            catch (Exception ex) 
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        public IActionResult DownloadResult(List<string> Result)
        {
            try
            {
                #region Login Check
                if (HttpContext.Session.GetString("UserID") == "" || HttpContext.Session.GetString("UserID") == null)
                    return RedirectToAction("Login", "User");
                #endregion

                if (Result != null && Result.Count > 0)
                {
                    // Create a new Excel package
                    using (var package = new ExcelPackage())
                    {
                        // Add a new worksheet to the Excel package
                        var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                        // Populate the Excel worksheet with data from the list
                        int i, j = 0;
                        for (i = 0; i <= Result.Count/2; i++)
                        {
                            if (i == 0)
                            {
                                worksheet.Cells[i + 1, 1].Value = "Search";
                                worksheet.Cells[i + 1, 2].Value = "Result";                              
                                continue;
                            }
                            worksheet.Cells[i + 1, 1].Value = Result[j];
                            worksheet.Cells[i + 1, 2].Value = Result[j+1];
                            j+=2;
                        }

                        // Set the content type and file name
                        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        var fileName = $"ResultFile_{DateTime.Today.ToString("yyyyMMdd")}.xlsx";

                        // Convert the Excel package to a byte array
                        var fileBytes = package.GetAsByteArray();

                        // Return the Excel file as a file result
                        return File(fileBytes, contentType, fileName);
                    }
                }
                ViewBag.ErrorMessage = "Failed to Download!";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        #region Load Excel File
        public IActionResult LoadFile()
        {
            #region Login Check
            if (HttpContext.Session.GetString("UserID") == "" || HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login", "User");
            #endregion

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadFile(IFormFile file)
        {
            #region Login Check
            if (HttpContext.Session.GetString("UserID") == "" || HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login", "User");
            #endregion
        
            try
            {
                List<string> excelData = new List<string>();
                List<SearchedData> searchedDataList = new List<SearchedData>();

                if (file == null || file.Length == 0)
                {
                    ViewBag.ErrorMessage = "No File Uploaded";
                }
                else
                {
                    using (var stream = new MemoryStream()) 
                    {
                        await file.CopyToAsync(stream);

                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                            if (worksheet == null) 
                            {
                                return BadRequest("The Excel file does not contain any worksheets.");
                            }

                            #region Fetch All data in a List String
                            string result = string.Empty;
                            int UsersearchedCount = Convert.ToInt32(HttpContext.Session.GetString("Searched"));

                            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                            {
                                if(HttpContext.Session.GetString("Membership") != "Admin" && Convert.ToInt32(HttpContext.Session.GetString("Limit")) <= UsersearchedCount)
                                {
                                    TempData["LimitEnd"] = "Limit End";                                    
                                    break;
                                }

                                var cellValue = worksheet.Cells[row, 1].Text;

/*Replace White Spaces*/
                                if (cellValue.Contains(' '))
                                {
                                    var cellValue2 = cellValue.Replace(" ", "");
                                    cellValue = cellValue2;
                                }

                                if (cellValue == null || cellValue == string.Empty)
                                {
                                    continue;
                                }

                                #region Get the result Data And add to the list String
                                result = string.Empty;

/*Clean Search site*/
                                cellValue = cellValue.ToLower();
                                cellValue = SiteCleaning(cellValue);

                                result = GetDataFromWeb(cellValue);

                                if (result == string.Empty || result == null)
                                {
                                    if (result == null)
                                        result = string.Empty;

                                    excelData.Add(cellValue);
                                    excelData.Add(result);
                                    continue;
                                }

                                SearchedData searchedData = new SearchedData();
                                searchedData.SearchedKey = cellValue;
                                searchedData.SearchedValue = result;
                                searchedData.UserID = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                                searchedData.SearchDate = DateTime.Today;

/*Do The check and counting*/
                                if (row % 50 == 0 || row >= worksheet.Dimension.Rows)
                                {
                                    SearchRequirements sr = await _iSearchRequirementsManager.GetById(Convert.ToInt32(HttpContext.Session.GetString("SearchID")));
                                    
                                    if (sr.LastUpdatedDay == DateTime.Today)
                                    {
                                        sr.SearchCount = sr.SearchCount + ((row % 50) - 1);
                                    }
                                    else
                                    {
                                        sr.LastUpdatedDay = DateTime.Today;
                                        sr.SearchCount = row - 1;
                                        HttpContext.Session.SetString("Date", DateTime.Today.ToString());
                                    }
                                    
                                    HttpContext.Session.SetString("SearchCount", sr.SearchCount.ToString());

                                    bool save = await _iSearchRequirementsManager.Update(sr);
                                    if (save)
                                    {
                                        HttpContext.Session.SetString("SearchCount", (sr.SearchCount).ToString());                                       
                                    }
                                    else
                                    {
                                        return BadRequest("Search Engine Data not Updating.");
                                    }
                                }

                                searchedDataList.Add(searchedData);
                                excelData.Add(cellValue);
                                excelData.Add(result);
                                UsersearchedCount++;

                                #endregion
                            }

                            User user = await _iUserManager.GetById(Convert.ToInt32(HttpContext.Session.GetString("UserID")));
                            if(user.TotalSearched != UsersearchedCount)
                            {
                                user.TotalSearched = UsersearchedCount;
                                bool save2 = await _iUserManager.Update(user);
                                if (save2)
                                {
                                    HttpContext.Session.SetString("Searched", UsersearchedCount.ToString());
                                }
                                else
                                {
                                    return BadRequest("Search Engine Data not Updating.");
                                }
                            }         
                            #endregion

/*To Display data and make Downloadable excel "excelData"*/
                            if (searchedDataList.Count > 0)
                            {
                                await _db.SearchedData.AddRangeAsync(searchedDataList);
                                int IsSaved = _db.SaveChanges();

/*Update SearchCount*/
                                if(IsSaved <= 0)
                                {
                                    ViewBag.ErrorMessage = "No File uploaded in server";
                                    return View();
                                }
                            }
                            if(excelData.Count>0)
                                TempData["ResultData"] = excelData;
                            return RedirectToAction("Index");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;               
            }
            return View();
        }


        private string SiteCleaning(string website)
        {
            string result = website;
            int start = 0;
            if (website.Contains("www."))
            {
                start = website.IndexOf("www.") + 4;
            }
            else if (website.Contains("//"))
            {
                start = website.IndexOf("//") + 2;
            }

            result = result.Substring(start, (result.Length - start));

            if (result.Contains('/'))
            {
                result = result[..(result.IndexOf("/"))];
            }

            if (!char.IsLetter(result[result.Length - 1]))
            {
                int i, end = result.Length - 1;
                for (i = end; i > 0; i--)
                {
                    if (char.IsLetter(result[i]))
                    {
                        end = i + 1;
                        break;
                    }
                }
                result = result.Substring(0, end);
            }
            return result;
        }
        #endregion

        #region Search Related Operations

        #region Search Data on Google
        public string GetDataFromWeb(string cellValue)
        {
            return "result";
        }
        #endregion

        #region Create Search Query
        private string SearchQueryBuild(string cellValue)
        {
            if(cellValue!=string.Empty)
            {
                string SQuery = string.Empty;

                IEnumerable<SearchQuery> searchQuery = _db.SearchQuery.Where(s => s.IsActive == true).ToList();
                foreach(SearchQuery i in searchQuery)
                {
                    SQuery = i.Start + cellValue + i.End;
                    return SQuery;
                }              
            }
            return string.Empty;
        }
        #endregion

        #region Filer Searched Data
        private string FilterSearchedData(string responseText)
        {
            string Result = string.Empty;
            string str, str1 = responseText;
            int start = str1.IndexOf("\"snippet\"");
            int end = str1.LastIndexOf("\"htmlSnippet\"");
            str = string.Empty;
            str = str1.Substring(start);
            str1 = string.Empty;
            str1 = str.Remove(end - start);
            str = string.Empty;

/*Replace special characters that may exist after mail end*/
            str = str1.Replace(',', ' ');

            int i, emailStart, emailEnd;
            string email;
            bool IsContain, IsFirst = true;

            while (true)
            {
                start = str.IndexOf("\"snippet\"");
                end = str.IndexOf("\"htmlSnippet\"");

                for (i = start; i < end; i++)
                {
/*Search for mail here and store*/
                    if (str[i] == '@')
                    {
/*Mail start point*/
                        emailStart = i - 1;
                        while (true)
                        {
                            if (str[emailStart] != ' ')
                            {
                                emailStart--;
                            }
                            else
                            {
                                while (!char.IsLetter(str[emailStart + 1]))
                                {
                                    emailStart++;
                                }
                                emailStart++;
                                break;
                            }
                        }
/*Mail end point*/
                        emailEnd = i + 1;
                        while (true)
                        {
                            if (str[emailEnd] != ' ')
                            {
                                emailEnd++;
                            }
                            else
                            {
                                while (!char.IsLetter(str[emailEnd-1]))
                                {
                                    emailEnd--;
                                }
                                break;
                            }
                        }

                        email = str.Substring(emailStart, emailEnd - emailStart);

                        if (email.Contains('/'))
                        {
                            email = email[..(email.IndexOf("/"))];
                        }
                        else if (email.Contains('\\'))
                        {
                            email = email[..(email.IndexOf('\\'))];
                        }

                        if (!email.Contains('@') || Result.Contains(email) || !email.Contains('.') || email.Contains('*'))
                            continue;

                        if (IsFirst == false)
                        {
                            Result += ", ";
                        }
/*Add to Result*/
                        Result += email;

                        i = emailEnd + 1;

                        IsFirst = false;
                    }
                }
                str1 = str.Substring(end + 5);
                str = string.Empty;
                str = str1;
                str1 = string.Empty;
                IsContain = str.Contains("snippet");

                if (!IsContain)
                {
                    break;
                }
            }
            return Result;
        }
        private string FilterSearchedData2(string responseText)
        {
            string Result = string.Empty;
            string str = string.Empty;
            string str1 = responseText;

            int i, start, end, emailStart, emailEnd;
            start = str1.IndexOf("<div class=\"BNeawe");
            end = str1.IndexOf("<footer>");
            if (start < 1) 
            {
                //handle proxy here
                return string.Empty;
            }
            
            if(end>start)
                str = str1.Substring(start, end - start);
            
            string email = string.Empty;
            bool IsFirst = true;

            while (str.Contains("<div class=\"BNeawe"))
            {                
                end = str.IndexOf("</div>");

                string s = str.Substring(19, end-19);

                if(!s.Contains('@'))
                {
                    str1 = string.Empty;
                    str1 = str.Substring(end + 6);
                    str = string.Empty;
                    if(str1.Contains("<div class=\"BNeawe"))
                        str = str1.Substring(str1.IndexOf("<div class=\"BNeawe"));
                    continue;
                }

                for (i = 0; i < end; i++)
                {
                    /*Search for mail here and store*/
                    if (str[i] == '@')
                    {
                        /*Mail start point*/
                        emailStart = i - 1;
                        while (true)
                        {
                            if (str[emailStart] != ' ')
                            {
                                emailStart--;
                            }
                            else
                            {
                                while (!char.IsLetter(str[emailStart + 1]))
                                {
                                    emailStart++;
                                }
                                emailStart++;
                                break;
                            }
                        }
                        /*Mail end point*/
                        emailEnd = i + 1;
                        while (true)
                        {
                            if (str[emailEnd] != ' ')
                            {
                                emailEnd++;
                            }
                            else
                            {
                                while (!char.IsLetter(str[emailEnd - 1]))
                                {
                                    emailEnd--;
                                }
                                break;
                            }
                        }

                        email = str.Substring(emailStart, emailEnd - emailStart);

                        if (email.Contains('/'))
                        {
                            email = email[..(email.IndexOf("/"))];
                        }
                        else if (email.Contains('\\'))
                        {
                            email = email[..(email.IndexOf('\\'))];
                        }

                        if (!IsValidEmail(email))
                            continue;
                        else if (!email.Contains('@') || Result.Contains(email) || !email.Contains('.') || email.Contains('*'))
                            continue;

                        if (IsFirst == false)
                        {
                            Result += ", ";
                        }
                        /*Add to Result*/
                        Result += email;

                        i = emailEnd + 1;

                        IsFirst = false;
                    }
                }
                str1 = string.Empty;
                str1 = str.Substring(end + 6);
                str = string.Empty;
                if (str1.Contains("<div class=\"BNeawe"))
                    str = str1.Substring(str1.IndexOf("<div class=\"BNeawe"));
            }
            return Result;
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }
        #endregion

        #endregion

        #region Search Engine Check and Update
        public async void SearchEngineUpdate()
        {
            try
            {
                if (Convert.ToInt32(HttpContext.Session.GetString("SearchCount")) >= 100 || HttpContext.Session.GetString("SearchID") == string.Empty || HttpContext.Session.GetString("SearchID") == null)
                {
                    bool IsNew = false;
                    if (HttpContext.Session.GetString("SearchID") == string.Empty || HttpContext.Session.GetString("SearchID") == null)
                    {
                        IsNew = true;
                    }
                    if (!IsNew)
                    {
                        int searchID = Convert.ToInt32(HttpContext.Session.GetString("SearchID"));
                        SearchRequirements Oldsearch = await _iSearchRequirementsManager.GetById(searchID);
                        Oldsearch.LastUpdatedDay = DateTime.Today;
                        bool isAdded = await _iSearchRequirementsManager.Update(Oldsearch);
                    }

                    bool SearchEngineFound = false;

                    #region Setup New Search Engine
                    IEnumerable<SearchRequirements> searchRequirements = _db.SearchRequirements.Where(e => e.LastUpdatedDay != DateTime.Today).ToList();
                    if (searchRequirements != null)
                    {
                        foreach (SearchRequirements i in searchRequirements)
                        {
                            if (i.IsActive == true)
                            {
                                HttpContext.Session.SetString("SearchID", i.ID.ToString());
                                HttpContext.Session.SetString("ApiKey", i.ApiKey);
                                HttpContext.Session.SetString("CX", i.CX);
                                HttpContext.Session.SetString("SearchCount", "0");
                                HttpContext.Session.SetString("Date", DateTime.Today.ToString());

                                SearchRequirements sr = await _iSearchRequirementsManager.GetById(i.ID);
                                sr.LastUpdatedDay = DateTime.Today;
                                bool Save = await _iSearchRequirementsManager.Update(sr);

                                if (!Save)
                                {
                                    //Can Debug to check error
                                }
                                SearchEngineFound = true;
                                break;
                            }
                        }
                    }
                    if (SearchEngineFound == false)
                    {
                        ViewBag.ErrorMessage = "No more Search Engine available Today. Sorry for the trouble.";
                    }
                    #endregion
                }
                else if (Convert.ToDateTime(HttpContext.Session.GetString("Date")) != DateTime.Today)
                {
                    SearchRequirements sr = await _iSearchRequirementsManager.GetById(Convert.ToInt32(HttpContext.Session.GetString("SearchID"))) ;
                    sr.LastUpdatedDay = DateTime.Today;
                    sr.SearchCount = 0;
                    bool Save = await _iSearchRequirementsManager.Update(sr);

                    if (!Save)
                    {
                        //check by debugging
                    }
                    HttpContext.Session.SetString("SearchCount", "0");
                    HttpContext.Session.SetString("Date", DateTime.Today.ToString());
                }
            }
            catch(Exception ex) 
            {
                ViewBag.Error = ex.Message;
            }
        }
        #endregion
    }
}