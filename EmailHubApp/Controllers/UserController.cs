using AutoMapper;
using EmailHubApp.Database;
using EmailHubApp.Manager;
using EmailHubApp.Manager.Contract;
using EmailHubApp.Model.Models;
using EmailHubApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EmailHubApp.Controllers
{
    public class UserController : Controller
    {
        private readonly EmailHubDB _db;
        private readonly IMapper _iMapper;    
        private readonly IUserManager _iUserManager;
        private readonly IUserTypeManager _iUserTypeManager;
        private readonly ISearchedDataManager _iSearchedDataManager;
        private readonly ISearchRequirementsManager _iSearchRequirementsManager;
        public UserController(IMapper iMapper, IUserManager iUserManager, IUserTypeManager iUserTypeManager, ISearchedDataManager iSearchedDataManager,ISearchRequirementsManager iSearchRequirementsManager)
        {
            _db = new EmailHubDB();
            _iMapper = iMapper;
            _iUserManager = iUserManager;
            _iUserTypeManager = iUserTypeManager;
            _iSearchedDataManager = iSearchedDataManager;
            _iSearchRequirementsManager = iSearchRequirementsManager;
        }

        #region User Login
        public async Task<IActionResult> Index()
        {
            #region Login Check
            if (HttpContext.Session.GetString("UserID") == "" || HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login", "User");
            #endregion

            if (HttpContext.Session.GetString("Membership") == "Admin")
                ViewBag.searchedData = await _iSearchedDataManager.GetAll();                
            else
                ViewBag.searchedData = _db.SearchedData.Where(u => u.UserID == Convert.ToInt32(HttpContext.Session.GetString("UserID"))).ToList();

            ViewBag.User = _iMapper.Map<IEnumerable<UserOperation>>(await _iUserManager.GetAll());

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            #region Not Login Check
            if (HttpContext.Session.GetString("UserID") == "" || HttpContext.Session.GetString("UserID") == null)
                return View();
            #endregion
            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserOperation user)
        {
            try
            {                
                int userCount = _db.User.Count();

                if (userCount <= 0 && user.UserName == "xyz" && user.Password == "123456")                    
                {
                    //TypeID
                    int typeCount = _db.UserType.Where(u => u.Name == "Admin").Select(u => u.ID).FirstOrDefault();
                    
                    if(typeCount <= 0) 
                    {
                        UserType userType = new UserType();
                        userType.Name = "Admin";
                        userType.Limit = 0;
                        userType.IsActive = true;

                        bool saveType = await _iUserTypeManager.Create(userType);
                        if (!saveType) 
                        {
                            return BadRequest("Initial User-Type Creation Failed");
                        }
                    }

                    //create user
                    User createUser = _iMapper.Map<User>(user);
                    createUser.Name = "XYZ";
                    createUser.Email = "no-reply@gmail.com";
                    createUser.EntryDate = DateTime.Today;
                    createUser.IsActive = true;
                    createUser.TypeID = _db.UserType.Where(u => u.Name == "Admin").Select(u => u.ID).FirstOrDefault();

                    bool save = await _iUserManager.Create(createUser);
                    if(!save)
                    {
                        return BadRequest("Initial User Failed");
                    }
                    HttpContext.Session.SetString("UserID", "1");
                    HttpContext.Session.SetString("Membership", "Admin");
                    return RedirectToAction("Index");
                }
                int id = _db.User.Where(u => u.IsActive ==true && u.UserName == user.UserName && u.UserName!= "xyz").Select(u => u.ID).FirstOrDefault();
                if (id != 0)
                {                        
                    UserOperation LoginUser = _iMapper.Map<UserOperation>(await _iUserManager.GetById(id));
                    if(LoginUser.Password == user.Password)
                    {
                        //Login Successful
                        #region Set Session Variable

                        int TypeID = _db.UserType.Where(u => u.ID == LoginUser.TypeID).Select(u => u.ID).FirstOrDefault();

                        if (TypeID != 0)
                        {
                            UserTypeOperation type = _iMapper.Map<UserTypeOperation>(await _iUserTypeManager.GetById(TypeID));

                            //User
                            HttpContext.Session.SetString("UserID", LoginUser.ID.ToString());
                            HttpContext.Session.SetString("UserName", LoginUser.Name.ToString());
                            HttpContext.Session.SetString("Membership", type.Name.ToString());
                            HttpContext.Session.SetString("Limit", type.Limit.ToString());
                            HttpContext.Session.SetString("Searched", LoginUser.TotalSearched.ToString());
                        }

                        #region Search Engine Session Variable
                        int RequirementsID = _db.SearchRequirements.Where(s => s.SearchCount < 100 || s.LastUpdatedDay != DateTime.Today).Select(s => s.ID).FirstOrDefault();
                        
                        if (RequirementsID != 0) 
                        {
                            SearchRequirements searchRequirements = await _iSearchRequirementsManager.GetById(RequirementsID);

                            if (searchRequirements != null)
                            {                                
                                if(searchRequirements.LastUpdatedDay != DateTime.Today)
                                {
                                    searchRequirements.LastUpdatedDay = DateTime.Today;
                                    searchRequirements.SearchCount = 0;
                                    bool save = await _iSearchRequirementsManager.Update(searchRequirements);
                                    if (!save)
                                    {
                                        ViewBag.ErrorMessage = "Search Engine not Updating.";
                                        return View();
                                    }                                   
                                }
                                HttpContext.Session.SetString("SearchID", searchRequirements.ID.ToString());
                                HttpContext.Session.SetString("ApiKey", searchRequirements.ApiKey);
                                HttpContext.Session.SetString("CX", searchRequirements.CX);
                                HttpContext.Session.SetString("Date", searchRequirements.LastUpdatedDay.ToString());
                                HttpContext.Session.SetString("SearchCount", searchRequirements.SearchCount.ToString());
                            }
                        }
                        #endregion

                        #endregion
                       
                        return RedirectToAction("Index");
                    }
                    ViewBag.WrongPassword = "Incorrect Password!";
                    return View();
                }
                ViewBag.UserNameExists = "User doesn't Exists!";
                return View();
            }
            catch (Exception ex)
            {
                return ViewBag.ErrorMessage = "Failed to Login. Error: " +  ex.Message;
            }          
        }

        public IActionResult Logout()
        {
            HttpContext.Session.SetString("UserID", "");
            HttpContext.Session.SetString("UserName", "");
            HttpContext.Session.SetString("Membership", "");
            HttpContext.Session.SetString("Limit", "");
            HttpContext.Session.SetString("Searched", "");

            HttpContext.Session.SetString("SearchID", "");
            HttpContext.Session.SetString("ApiKey", "");
            HttpContext.Session.SetString("CX", "");
            HttpContext.Session.SetString("Date", "");
            HttpContext.Session.SetString("SearchCount", "");

            return RedirectToAction("Login", "User");
        }
        #endregion

        #region SignUp
        [HttpGet]
        public IActionResult Signup()
        {
            #region Not Login Check
            if (HttpContext.Session.GetString("UserID") == "" || HttpContext.Session.GetString("UserID") == null)
                return View();
            #endregion

            return RedirectToAction("Index", "User");          
        }

        [HttpPost]
        public async Task<IActionResult> SignupAsync(UserOperation user, string confirm)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    #region Validation Check
                    bool notValid = false;
                    EmailHubDB _db = new EmailHubDB();
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
                    if (user.Password != confirm)
                    {
                        ViewBag.PasswordMismatched = "Password is not matching!";
                        notValid = true;
                    }
                    if(notValid)
                    {
                        return View(user);
                    }                       
                    #endregion
                    
                    int typeID = _db.UserType.Where(t => t.Name == "Free").Select(t => t.ID).FirstOrDefault();
                    UserType type = await _iUserTypeManager.GetById(typeID);
                    if(typeID == 0) 
                    {
                        return BadRequest("Failed to select User-Type.");
                    }
                    user.TypeID = typeID;

                    //Varify Email Here
                    user.IsActive = true;

                    user.Password = EDPassword(user.Password, true);
                    User newUser = _iMapper.Map<User>(user);
                    bool isAdded = await _iUserManager.Create(newUser);
                    if (isAdded)
                    {
                        HttpContext.Session.SetString("UserID", user.ID.ToString());
                        HttpContext.Session.SetString("UserName", user.Name.ToString());
                        HttpContext.Session.SetString("Membership", type.Name);
                        HttpContext.Session.SetString("Limit", type.Limit.ToString());
                        HttpContext.Session.SetString("Searched", "0");

                        #region Search Engine Session Variable
                        int RequirementsID = _db.SearchRequirements.Where(s => s.SearchCount < 100 || s.LastUpdatedDay != DateTime.Today).Select(s => s.ID).FirstOrDefault();

                        if (RequirementsID != 0)
                        {
                            SearchRequirements searchRequirements = await _iSearchRequirementsManager.GetById(RequirementsID);

                            if (searchRequirements != null)
                            {
                                if (searchRequirements.LastUpdatedDay != DateTime.Today)
                                {
                                    searchRequirements.LastUpdatedDay = DateTime.Today;
                                    searchRequirements.SearchCount = 0;
                                    bool save = await _iSearchRequirementsManager.Update(searchRequirements);
                                    if (!save)
                                    {
                                        ViewBag.ErrorMessage = "Search Engine not Updating.";
                                        return View();
                                    }
                                }
                                HttpContext.Session.SetString("SearchID", searchRequirements.ID.ToString());
                                HttpContext.Session.SetString("ApiKey", searchRequirements.ApiKey);
                                HttpContext.Session.SetString("CX", searchRequirements.CX);
                                HttpContext.Session.SetString("Date", searchRequirements.LastUpdatedDay.ToString());
                                HttpContext.Session.SetString("SearchCount", searchRequirements.SearchCount.ToString());
                            }
                        }
                        #endregion


                        return RedirectToAction("Index");
                    }
                        
                    else
                        ViewBag.ErrorMessage = "Failed to Sign Up";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Operation Failed. \nReason: " + ex.Message;
            }
            return View();
        }
        #endregion

        #region Other Tasks
        public IActionResult EmailConfirmation()
        {
            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

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

        #endregion
    }
}
