﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coderr.Server.WebSite.Areas.Installation.Controllers
{
    /// <summary>
    /// Purpose is to be able to launch installation area and be able to use dependencies in the home controller
    /// </summary>
    
    public class BootController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous/*, Route("installation/{*url}")*/]
        public ActionResult NoInstallation()
        {
            if (Request.Path.Value.EndsWith("/setup/activate", StringComparison.OrdinalIgnoreCase))
                return Redirect("~/");
            return View();
        }

        [AllowAnonymous]
        public ActionResult ToInstall()
        {
            return RedirectToRoute(new { Controller = "Setup", Area = "Installation" });
        }


    }
}