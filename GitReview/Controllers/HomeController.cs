// -----------------------------------------------------------------------
// <copyright file="HomeController.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// The controller for the landing page for the application.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Gets the landing page of the application.
        /// </summary>
        /// <returns>A <see cref="ViewResult"/>.</returns>
        public ActionResult Index()
        {
            return this.View();
        }
    }
}
