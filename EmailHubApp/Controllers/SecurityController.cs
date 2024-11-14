using AutoMapper;
using EmailHubApp.Manager.Contract;
using EmailHubApp.Model.Models;
using EmailHubApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using Microsoft.CodeAnalysis;
using EmailHubApp.Database;
using System.Dynamic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using EmailHubApp.Manager;

namespace EmailHubApp.Controllers
{
    #region Admin Users Only
    public class SecurityController : Controller
    {
        #region Initialization
        private readonly EmailHubDB _db;
        private readonly IMapper _iMapper;    
        private readonly IUserManager _iUserManager;
        private readonly IUserTypeManager _iUserTypeManager;
        private readonly ISearchQueryManager _iSearchQueryManager;
        private readonly ISearchedDataManager _iSearchedDataManager;
        private readonly ISearchRequirementsManager _iSearchRequirementsManager;
        public SecurityController(IMapper iMapper, IUserManager iUserManager, IUserTypeManager iUserTypeManager, ISearchQueryManager iSearchQueryManager, ISearchedDataManager iSearchedDataManager, ISearchRequirementsManager iSearchRequirementsManager)
            {
                _db = new EmailHubDB();
                _iMapper = iMapper;
                _iUserManager = iUserManager;
                _iUserTypeManager = iUserTypeManager;
                _iSearchQueryManager = iSearchQueryManager;
                _iSearchedDataManager = iSearchedDataManager;
                _iSearchRequirementsManager = iSearchRequirementsManager;
            }
        #endregion

        #region User

        #region User List
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserOperation>>> Index()
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You don not have the permission to access this resource");
            #endregion

                if (TempData["ErrorMessage"] != null)
                {
                    string? message = TempData["ErrorMessage"] as string;
                    if (message != null)
                    {
                        ViewBag.ErrorMessage = message;
                        TempData["ErrorMessage"] = null;
                    }
                }

            IEnumerable<UserOperation> users = _iMapper.Map<IEnumerable<UserOperation>>(await _iUserManager.GetAll());

                ViewBag.userType = _iMapper.Map<IEnumerable<UserTypeOperation>>(await _iUserTypeManager.GetAll());

                return View(users);
            }
            catch(Exception ex)
            {
                return ViewBag.ErrorMessage = ex.Message;
            }
        }
        #endregion

        #region Create and Update User
        [HttpGet]
        public async Task<IActionResult> AddUpdateUser(int? id)
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
                #endregion

                ViewBag.userType = _iMapper.Map<IEnumerable<UserTypeOperation>>(await _iUserTypeManager.GetAll());

                if (id == null)
                {
                    HttpContext.Session.SetString("Title", "Create");
                    return View();
                }

                HttpContext.Session.SetString("Title", "Update");

                UserOperation user = _iMapper.Map<UserOperation>(await _iUserManager.GetById(id));

                return View(user);
            }
            catch (Exception ex) 
            {
                return ViewBag.ErrorMessage(ex.Message);
            }            
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateUser(UserOperation user, string? confirm)
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
                #endregion

                ViewBag.userType = _iMapper.Map<IEnumerable<UserTypeOperation>>(await _iUserTypeManager.GetAll());
                if (ModelState.IsValid) 
                {
                    User addupdateUser = _iMapper.Map<User>(user);                   
                    bool notValid = false;

                    #region New User Add
                    if (user.ID == 0)
                    {
                        #region Validation Check                       
                        bool usernameExists = _db.User.Any(u => u.UserName == user.UserName);
                        if (usernameExists)
                        {
                            ViewBag.UserNameExists = "UserName already exists!";
                            notValid = true;
                        }
                        bool emailExists = _db.User.Any(e => e.Email == user.Email);
                        if (emailExists)
                        {
                            ViewBag.EmailExists = "Email already exists!";
                            notValid = true;
                        }
                        if(addupdateUser.TypeID == 0)
                        {
                            ViewBag.NoUserTypeSelected = "Please Select a User Type!";
                            notValid = true;
                        }
                        if (user.Password != confirm)
                        {
                            ViewBag.PasswordMismatched = "Password is not matching!";
                            notValid = true;
                        }
                        if (notValid)
                        {
                            return View(user);
                        }
                        #endregion

                        addupdateUser.Password = EDPassword(user.Password, true);

                        addupdateUser.EntryDate = DateTime.Now;
                        addupdateUser.TotalSearched = 0;

                        bool isAdded = await _iUserManager.Create(addupdateUser);
                        if (isAdded)
                            return RedirectToAction("Index");
                        else
                            ViewBag.ErrorMessage = "Failed to Create New User";
                    }
                    #endregion

                    #region Old User Update
                    else
                    {
                        #region Validation Check                       
                        User? OldInfo = _db.User.Find(user.ID);
                            
                        if(OldInfo != null && OldInfo.UserName == "admin" && OldInfo.UserName != user.UserName)
                        {
                            ViewBag.UserNameExists = "\'admin\' UserID can't be Changed!";
                            notValid = true;
                        }
                        if (OldInfo != null && Convert.ToInt32(HttpContext.Session.GetString("UserID")) == user.ID && OldInfo.Name != user.Name)
                        {
                            HttpContext.Session.SetString("UserName", user.Name);
                        }
                    if (OldInfo!= null && OldInfo.UserName!= user.UserName)
                        {
                            int userNameCount = _db.User.Count(u => u.UserName == user.UserName);
                            if (userNameCount > 0) 
                            {
                                ViewBag.UserNameExists = "UserName already exists!";
                                notValid = true;
                            }
                        }
                        if (OldInfo != null && OldInfo.Email != user.Email)
                        {
                            int emailCount = _db.User.Count(u => u.Email == user.Email);
                            if (emailCount > 0)
                            {
                                ViewBag.EmailExists = "Email already exists!";
                                notValid = true;
                            }
                        }
                        if(OldInfo != null && Convert.ToInt32(HttpContext.Session.GetString("UserID")) == user.ID && OldInfo.TypeID != user.TypeID)
                        {
                            ViewBag.NoUserTypeSelected = "User Type of Logged in User can't be changed!";
                            notValid = true;
                        }
                        if (addupdateUser.TypeID == 0)
                        {
                            ViewBag.NoUserTypeSelected = "Please Select a User Type!";
                            notValid = true;
                        }                            
                        if(OldInfo != null && (confirm == "" || confirm == null))
                        {
                            user.Password = OldInfo.Password;
                        }
                        else if (user.Password != confirm)
                        {
                            ViewBag.ErrorMessage = "Password is not matching";
                            notValid = true;
                        }
                        if (notValid)
                        {
                            return View(user);
                        }
                        #endregion

                        addupdateUser.Password = EDPassword(user.Password, true);

                        bool isUpdated = await _iUserManager.Update(addupdateUser);
                        if (isUpdated)
                            return RedirectToAction("Index");
                        else
                            ViewBag.ErrorMessage = "Failed to Update User";
                    }
                    #endregion
                }
                return View(user);
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage(ex.Message);
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Delete User
        [HttpGet]
        public async Task<IActionResult> RemoveUser(int? id)
        {
            #region Admin Check
            if (HttpContext.Session.GetString("Membership") != "Admin")
                return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
            #endregion

            if (id == null)
                return NotFound();

        if (Convert.ToInt32(HttpContext.Session.GetString("UserID")) == id)
        {
            TempData["ErrorMessage"] = "Logged In User Can't be Deleted!";
            return RedirectToAction("Index");
        }

        User user = await _iUserManager.GetById(id);

            if (user == null)
                return NotFound();

            bool remove = await _iUserManager.Remove(user);

            if (remove)
                return RedirectToAction("Index");
                                
            return BadRequest();
        }
        #endregion

        #endregion

        #region UserType

        #region User Type List
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTypeOperation>>> UserTypeList()
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You don not have the permission to access this resource");
            #endregion

                if (TempData["ErrorMessage"] != null)
                {
                    string? message = TempData["ErrorMessage"] as string;
                    if (message != null)
                    {
                        ViewBag.ErrorMessage = message;
                        TempData["ErrorMessage"] = null;
                    }                      
                }

                IEnumerable<UserTypeOperation> userTypes = _iMapper.Map<IEnumerable<UserTypeOperation>>(await _iUserTypeManager.GetAll());

                return View(userTypes);
            }
            catch (Exception ex)
            {
                return ViewBag.ErrorMessage = ex.Message;
            }
        }
        #endregion

        #region Create and Update User Type
        [HttpGet]
        public async Task<IActionResult> AddUpdateUserType(int? id)
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
                #endregion

                if (id == null)
                {
                    HttpContext.Session.SetString("Title", "Create");
                    return View();
                }

                HttpContext.Session.SetString("Title", "Update");

                UserTypeOperation userType = _iMapper.Map<UserTypeOperation>(await _iUserTypeManager.GetById(id));

                return View(userType);
            }
            catch (Exception ex)
            {
                return ViewBag.ErrorMessage(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateUserType(UserTypeOperation userType)
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
                #endregion

                if (ModelState.IsValid)
                {
                    UserType addupdateUserType = _iMapper.Map<UserType>(userType);
                    bool notValid = false;

                    #region New User Type Add
                    if (userType.ID == 0)
                    {
                        #region Validation Check                       
                        bool userTypeExists = _db.UserType.Any(u => u.Name == userType.Name);
                        if (userTypeExists)
                        {
                            ViewBag.userTypeExists = "User Type already exists!";
                            notValid = true;
                        }
                        if (notValid)
                        {
                            return View(userType);
                        }
                        #endregion

                        bool isAdded = await _iUserTypeManager.Create(addupdateUserType);
                        if (isAdded)
                            return RedirectToAction("UserTypeList");
                        else
                            ViewBag.ErrorMessage = "Failed to Create New User Type";
                    }
                    #endregion

                    #region Old User Update
                    else
                    {
                        #region Validation Check                       
                        UserType? OldTypeInfo = _db.UserType.Find(userType.ID);
                        if (OldTypeInfo != null && OldTypeInfo.Name != userType.Name)
                        {
                            int userTypeCount = _db.UserType.Count(u => u.Name == userType.Name);
                            if (OldTypeInfo.Name == "Admin")
                            {
                                ViewBag.userTypeExists = "Admin User Type Can't be Changed!";
                                notValid = true;
                            }                                
                            else if (userTypeCount > 0)
                            {
                                ViewBag.userTypeExists = "User Type already exists!";
                                notValid = true;
                            }
                        }
                        if(userType.Name == "Admin" && userType.IsActive == false)
                        {
                            ViewBag.userTypeActive = "Admin User Type can't be Inactive!";
                            notValid = true;
                        }
                        if (notValid)
                        {
                            return View(userType);
                        }
                        #endregion

                        bool isUpdated = await _iUserTypeManager.Update(addupdateUserType);
                        if (isUpdated)
                            return RedirectToAction("UserTypeList");
                        else
                            ViewBag.ErrorMessage = "Failed to Update User Type";
                    }
                    #endregion
                }
                return View(userType);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage(ex.Message);
                return RedirectToAction("UserTypeList");
            }
        }
        #endregion

        #region Delete User Type
        [HttpGet]
        public async Task<IActionResult> RemoveUserType(int? id)
        {
            #region Admin Check
            if (HttpContext.Session.GetString("Membership") != "Admin")
                return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
            #endregion

            if (id == null)
                return NotFound();

            UserType userType = await _iUserTypeManager.GetById(id);

            if (userType.Name == "Admin")
            {
                TempData["ErrorMessage"] = "Admin User Type Can't be Deleted!";
                return RedirectToAction("UserTypeList");
            }

            //Replace Deleted UserType in Users
            UserTypeReplaceInUsers(userType);

            if (userType == null)
                return NotFound();

            bool remove = await _iUserTypeManager.Remove(userType);

            if (remove)
                return RedirectToAction("UserTypeList");

            return BadRequest();
        }
        #endregion

        #region User Type Replace
        private void UserTypeReplaceInUsers(UserType userType)
        {
            //replace the user type
        }
        #endregion

        #endregion

        #region Search Query

        #region Search Query List
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTypeOperation>>> SearchQueryList()
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You don not have the permission to access this resource");
                #endregion

                if (TempData["ErrorMessage"] != null)
                {
                    string? message = TempData["ErrorMessage"] as string;
                    if (message != null)
                    {
                        ViewBag.ErrorMessage = message;
                        TempData["ErrorMessage"] = null;
                    }
                }

                IEnumerable<SearchQueryOperation> searchQuery = _iMapper.Map<IEnumerable<SearchQueryOperation>>(await _iSearchQueryManager.GetAll());

                return View(searchQuery);
            }
            catch (Exception ex)
            {
                return ViewBag.ErrorMessage = ex.Message;
            }
        }
        #endregion

        #region Create and Update Search Query
        [HttpGet]
        public async Task<IActionResult> AddUpdateSearchQuery(int? id)
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
                #endregion

                if (id == null)
                {
                    HttpContext.Session.SetString("Title", "Create");
                    return View();
                }

                HttpContext.Session.SetString("Title", "Update");

                SearchQueryOperation searchQuery = _iMapper.Map<SearchQueryOperation>(await _iSearchQueryManager.GetById(id));

                return View(searchQuery);
            }
            catch (Exception ex)
            {
                return ViewBag.ErrorMessage(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateSearchQuery(SearchQueryOperation searchQuery)
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
                #endregion

                if (ModelState.IsValid)
                {
                    SearchQuery addupdateSearchQuery = _iMapper.Map<SearchQuery>(searchQuery);
                    bool notValid = false;

                    #region New Search Query Add
                    if (searchQuery.ID == 0)
                    {
                        #region Validation Check                       
                        bool searchQueryExists = _db.SearchQuery.Any(u => u.Start == searchQuery.Start && u.End == searchQuery.End);
                        if (searchQueryExists)
                        {
                            ViewBag.ErrorMessage = "Search Query already exists!";
                            notValid = true;
                        }
                        if (notValid)
                        {
                            return View(searchQuery);
                        }
                        #endregion

                        bool isAdded = await _iSearchQueryManager.Create(addupdateSearchQuery);
                        if (isAdded)
                        {
                            if(addupdateSearchQuery.IsActive)
                            {
                                IEnumerable<SearchQuery> SQueryList = _db.SearchQuery.Where(s => s.Start != addupdateSearchQuery.Start && s.End != addupdateSearchQuery.End && s.IsActive == true).ToList();
                                foreach (SearchQuery SQuery in SQueryList)
                                {
                                    SQuery.IsActive = false;
                                }
                                _db.SearchQuery.UpdateRange(SQueryList);
                                int save2 = await _db.SaveChangesAsync();
                                if (save2 <= 0)
                                    ViewBag.ErrorMessage = "Failed to Update Status.";
                            }                           
                            return RedirectToAction("SearchQueryList");
                        }
                            
                        else
                            ViewBag.ErrorMessage = "Failed to Create New Search Query.";
                    }
                    #endregion

                    #region Old Search Query Update
                    else
                    {
                        #region Validation Check  
                        
                        int searchQueryCount = _db.SearchQuery.Count(u => u.Start == searchQuery.Start && u.End == searchQuery.End);
                        if(searchQueryCount > 1) 
                        {
                            ViewBag.ErrorMessage = "Search Query already exists!";
                            notValid = true;
                        }

                        if (notValid)
                        {
                            return View(searchQuery);
                        }
                        #endregion

                        bool isUpdated = await _iSearchQueryManager.Update(addupdateSearchQuery);
                        if (isUpdated)
                        {
                            if (addupdateSearchQuery.IsActive)
                            {
                                IEnumerable<SearchQuery> SQueryList = _db.SearchQuery.Where(s => s.Start != addupdateSearchQuery.Start && s.End != addupdateSearchQuery.End && s.IsActive == true).ToList();
                                foreach (SearchQuery SQuery in SQueryList)
                                {
                                    SQuery.IsActive = false;
                                }
                                _db.SearchQuery.UpdateRange(SQueryList);
                                int save2 = await _db.SaveChangesAsync();
                                if (save2 <= 0)
                                    ViewBag.ErrorMessage = "Failed to Update Status.";
                            }
                            return RedirectToAction("SearchQueryList");
                        }                            
                        else
                            ViewBag.ErrorMessage = "Failed to Update Search Query";
                    }
                    #endregion
                }
                return View(searchQuery);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage(ex.Message);
                return RedirectToAction("UserTypeList");
            }
        }
        #endregion

        #region Update Search Query Status
        [HttpGet]
        public async Task<IActionResult> UpdateSearchQueryStatus(int? id)
        {
            try 
            {
                if(id == null || id == 0)
                {
                    return NotFound();
                }                
                else
                {
                    SearchQuery sr = await _iSearchQueryManager.GetById(id);
                    sr.IsActive = true;
                    bool save = await _iSearchQueryManager.Update(sr);
                    //int save = await _db.SaveChangesAsync();
                    if (save) 
                    {
                        IEnumerable<SearchQuery> SQueryList = _db.SearchQuery.Where(s => s.ID != sr.ID && s.IsActive == true).ToList();
                        foreach (SearchQuery SQuery in SQueryList)
                        {
                            SQuery.IsActive = false;
                        }
                        _db.SearchQuery.UpdateRange(SQueryList);
                        int save2 = await _db.SaveChangesAsync();
                        if (save2 > 0)
                            return RedirectToAction("SearchQueryList");
                        else
                            return BadRequest();
                    }
                    else
                        return BadRequest();
                }
            }
            catch (Exception ex) 
            {
                ViewBag.ErrorMessage(ex);
            }
            return BadRequest();
        }
        #endregion

        #region Delete Search Query
        [HttpGet]
        public async Task<IActionResult> RemoveSearchQuery(int? id)
        {
            #region Admin Check
            if (HttpContext.Session.GetString("Membership") != "Admin")
                return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
            #endregion

            if (id == null)
                return NotFound();

            SearchQuery searchQuery = await _iSearchQueryManager.GetById(id);

            if (searchQuery == null)
                return NotFound();

            bool remove = await _iSearchQueryManager.Remove(searchQuery);

            if (remove)
                return RedirectToAction("SearchQueryList");

            return BadRequest();
        }
        #endregion

        #endregion

        #region Search Engine

        #region Search Requirements List
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SearchRequirementsOperation>>> SearchRequirementsList()
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You don not have the permission to access this resource");
                #endregion

                IEnumerable<SearchRequirementsOperation> search = _iMapper.Map<IEnumerable<SearchRequirementsOperation>>(await _iSearchRequirementsManager.GetAll());

                return View(search);
            }
            catch (Exception ex)
            {
                return ViewBag.ErrorMessage = ex.Message;
            }
        }
        #endregion

        #region Create and Update Search Requirements
        [HttpGet]
        public async Task<IActionResult> AddUpdateSearchRequirements(int? id)
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
                #endregion

                if (id == null)
                {
                    HttpContext.Session.SetString("Title", "Create");
                    return View();
                }

                HttpContext.Session.SetString("Title", "Update");

                SearchRequirementsOperation search = _iMapper.Map<SearchRequirementsOperation>(await _iSearchRequirementsManager.GetById(id));

                return View(search);
            }
            catch (Exception ex)
            {
                return ViewBag.ErrorMessage(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateSearchRequirements(SearchRequirementsOperation search)
        {
            try
            {
                #region Admin Check
                if (HttpContext.Session.GetString("Membership") != "Admin")
                    return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
                #endregion

                if (ModelState.IsValid)
                {
                    SearchRequirements searchRequirement = _iMapper.Map<SearchRequirements>(search);
                    bool notValid = false;

                    #region New Search Add
                    if (search.ID == 0)
                    {
                        #region Validation Check                       
                        bool ApiKeyExists = _db.SearchRequirements.Any(u => u.ApiKey == search.ApiKey);
                        bool CxExists = _db.SearchRequirements.Any(u => u.CX == search.CX);
                        if (ApiKeyExists)
                        {
                            ViewBag.ApiKeyExists = "API Key already exists!";
                            notValid = true;
                        }
                        if (CxExists)
                        {
                            ViewBag.CxExists = "CX already exists!";
                            notValid = true;
                        }
                        if (notValid)
                        {
                            return View(search);
                        }
                        #endregion

                        bool isAdded = await _iSearchRequirementsManager.Create(searchRequirement);
                        if (isAdded)
                            return RedirectToAction("SearchRequirementsList");
                        else
                            ViewBag.ErrorMessage = "Failed to Create New Search Requirements";
                    }
                    #endregion

                    #region Old User Update
                    else
                    {
                    #region Validation Check                       
                        SearchRequirements? OldsearchRequirements = _db.SearchRequirements.Find(search.ID);
                        if (OldsearchRequirements != null && OldsearchRequirements.ApiKey != search.ApiKey)
                        {
                            int ApiKeyCount = _db.SearchRequirements.Count(u => u.ApiKey == search.ApiKey);

                            if (ApiKeyCount > 0)
                            {
                                ViewBag.ApiKeyExists = "API Key already exists!";
                                notValid = true;
                            }
                        }
                        if (OldsearchRequirements != null && OldsearchRequirements.CX != search.CX)
                        {
                            int CxCount = _db.SearchRequirements.Count(u => u.CX == search.CX);

                            if (CxCount > 0)
                            {
                                ViewBag.CxExists = "CX already exists!";
                                notValid = true;
                            }
                        }
                        if (notValid)
                        {
                            return View(search);
                        }
                        #endregion

                        bool isUpdated = await _iSearchRequirementsManager.Update(searchRequirement);
                        if (isUpdated)
                            return RedirectToAction("SearchRequirementsList");
                        else
                            ViewBag.ErrorMessage = "Failed to Update Search Requirements";
                    }
                    #endregion
                }
                return View(search);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage(ex.Message);
                return RedirectToAction("SearchRequirementsList");
            }
        }
        #endregion

        #region Delete Search Requirements
        [HttpGet]
        public async Task<IActionResult> RemoveSearchRequirements(int? id)
        {
            #region Admin Check
            if (HttpContext.Session.GetString("Membership") != "Admin")
                return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
            #endregion

            if (id == null)
                return NotFound();

            SearchRequirements search = await _iSearchRequirementsManager.GetById(id);

            if (search == null)
                return NotFound();

            bool remove = await _iSearchRequirementsManager.Remove(search);

            if (remove)
                return RedirectToAction("SearchRequirementsList");

            return BadRequest();
        }
        #endregion

        #endregion

        #region Encryption and Decryption
        private string EDPassword(string pass, bool en)
        {
            string password = "";
            try
            {
                
                if (pass != null)
                {
                    if (en)
                    {
                        #region Encrypt the password
                        password = pass;

                        #endregion
                    }
                    else
                    {
                        #region Decrypt the password
                        password = pass;

                        #endregion
                    }                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return password;
        }
        #endregion

        #region Session
        private IActionResult GetSetSession()
        {
            #region Admin Check
            if (HttpContext.Session.GetString("Membership") != "Admin")
                return StatusCode(403, "Access Denied: You do not have the permission to access this resource");
            #endregion

            #region User
            HttpContext.Session.SetString("UserID", "");
            HttpContext.Session.SetString("UserName", "");
            HttpContext.Session.SetString("Membership", "");
            HttpContext.Session.SetString("Limit", "");
            HttpContext.Session.SetString("Searched", "");
            HttpContext.Session.SetString("Title", "");
            #endregion

            #region Search
            HttpContext.Session.SetString("SearchID", "");
            HttpContext.Session.SetString("ApiKey", "");
            HttpContext.Session.SetString("CX", "");
            HttpContext.Session.SetString("SearchCount", "");
            HttpContext.Session.SetString("Date", "");
            #endregion

            return View();
        }
        #endregion
    }
    #endregion
}
