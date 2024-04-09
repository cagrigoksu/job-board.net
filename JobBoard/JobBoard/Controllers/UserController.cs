﻿using System.Net;
using Microsoft.AspNetCore.Mvc;
using JobBoard.Models.Data;
using JobBoard.Models.View;
using JobBoard.Models.Classes;
using JobBoard.Repositories.Interfaces;
using JobBoard.Services;
using JobBoard.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JobBoard.Controllers
{
    public class UserController : Controller
    {
        private readonly IJobPosterRepository? _jobPosterRepository;
        private readonly IUserService? _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ISecurityService _securityService;

        public UserController(IJobPosterRepository jobPosterRepository, IUserService? userService, IWebHostEnvironment webHostEnvironment, ISecurityService securityService)
        {
            _jobPosterRepository = jobPosterRepository;
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
            _securityService = securityService;
        }
        
        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogIn(string email, string pwd)
        {
            var user = _userService.GetUser(email);

            if (user != null)
            {
                var hashedPwd = _securityService.Hasher(pwd, user.PasswordSalt, Globals.HashIter);

                if (user.PasswordHash == hashedPwd)
                {
                    Globals.UserId = user.Id;
                    Globals.CompanyUser = user.CompanyUser;
                    HttpContext.Session.SetInt32("Id", user.Id);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetInt32("CompanyUser", Convert.ToInt32(user.CompanyUser));

                    var jobPosts = _jobPosterRepository.GetAllJobPosts();
                    return View("Index", new IndexViewModel() { UserId = user.Id, CompanyUser = user.CompanyUser, JobPosts = jobPosts });
                }
               
            }
            // TODO: user not found and incorrect password message
            return NotFound();
        }

        public IActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogOn(IFormCollection formCollection)
        {
            // user log on

            var isUserExist = _userService.IsUserExist(formCollection["email"]);

            if (!isUserExist)
            {
                var pwd = formCollection["pwd"];
                var pwdConf = formCollection["pwdConf"];
                if (pwd == pwdConf)
                {
                    // generate salt and hash
                    var userSalt = _securityService.GenerateSalt();
                    var userHashedPwd = _securityService.Hasher(pwd, userSalt, Globals.HashIter);

                    var user = new UserDataModel()
                    {
                        Email = formCollection["email"],
                        CompanyUser = formCollection["hiring"] == "on",
                        PasswordSalt = userSalt,
                        PasswordHash = userHashedPwd
                    };

                    // save user
                    _userService.AddUser(user);

                    var userResult = _userService.GetUser(user.Email);

                    if (userResult != null)
                    {
                        Globals.UserId = user.Id;
                        HttpContext.Session.SetInt32("Id", user.Id);
                        HttpContext.Session.SetString("Email", user.Email);
                        HttpContext.Session.SetInt32("CompanyUser", Convert.ToInt32(user.CompanyUser));

                        var jobPosts = _jobPosterRepository.GetAllJobPosts();
                        return View("Index", new IndexViewModel() { JobPosts = jobPosts });
                    }
                    // TODO: user not found.
                    return NotFound();
                }

                return View("LogOn");
            }

            // TODO: return user exist message.
            return View("LogOn");
        }

        public IActionResult UserProfile()
        {
            if (new SessionUtils().EmptySession())
            {
                return View("LogIn");
            }

            var result = new UserProfileViewModel();

            var profile = _userService.GetUserProfile(Globals.UserId);

            if (profile != null)
            {
                result.Name = profile.Name;
                result.Surname = profile.Surname;
                result.PhoneNumber = profile.PhoneNumber;
                result.Email = profile.Email;
            };

            result.CompanyUser = Convert.ToBoolean(HttpContext.Session.GetInt32("CompanyUser"));
            return View(result);
        }

        [HttpPost]
        public IActionResult UserProfile(UserProfileViewModel model)
        {
            if (new SessionUtils().EmptySession())
            {
                return View("LogIn");
            }

            var profile = _userService.GetUserProfile(Globals.UserId);
            var _UrlResume = "";
            var _UrlMotivationLetter = "";
            
            // Copy CV and ML files to server
            if (model.CV != null)
            {
                string root = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                string pathCV = Path.Combine(root, "Resumes\\Profile");

                var filenameCV = Globals.UserId.ToString() + ".pdf";

                using FileStream streamCV = new FileStream(Path.Combine(pathCV, filenameCV), FileMode.Create);

                model.CV.CopyTo(streamCV);

                _UrlResume = Path.Combine(pathCV, filenameCV);
            }
            if (model.MotivationLetter != null)
            {
                string root = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                string pathML = Path.Combine(root, "MotivationLetters\\Profile");

                var filenameMotivation = Globals.UserId.ToString() + ".pdf";

                using FileStream streamML = new FileStream(Path.Combine(pathML, filenameMotivation), FileMode.Create);

                model.MotivationLetter.CopyTo(streamML);

                _UrlMotivationLetter = Path.Combine(pathML, filenameMotivation);
            }

            if (profile != null)
            {
                // Update existing user profile

                profile.Name = model.Name;
                profile.Surname = model.Surname;
                profile.PhoneNumber = model.PhoneNumber;
                profile.Email = model.Email;

                if (_UrlResume != "")
                    profile.UrlResume = _UrlResume;
                if (_UrlMotivationLetter != "")
                    profile.UrlMotivationLetter = _UrlMotivationLetter;
                _userService.EditUserProfile(profile);
            }
            else
            {
                // Add new user profile
                profile = new UserProfileDataModel();
                profile.UserId = Globals.UserId;
                profile.Name = model.Name;
                profile.Surname = model.Surname;
                profile.PhoneNumber = model.PhoneNumber;
                profile.Email = model.Email;

                if (_UrlResume != "")
                    profile.UrlResume = _UrlResume;
                if (_UrlMotivationLetter != "")
                    profile.UrlMotivationLetter = _UrlMotivationLetter;
                _userService.AddUserProfile(profile);
            }

            ModelState.Clear();
            ViewBag.Message = "Successfully saved.";
            model.CompanyUser = Convert.ToBoolean(HttpContext.Session.GetInt32("CompanyUser"));

            return View(model);
        }

        public IActionResult LogOut()
        {
            // HttpContext.Session.Remove("Id");
            // HttpContext.Session.Remove("Email");
            HttpContext.Session.Clear();

            return View("LogIn");
        }
       
    }
}
