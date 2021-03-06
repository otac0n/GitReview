// -----------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Contains route registrations.
    /// </summary>
    public static class RouteConfig
    {
        /// <summary>
        /// Registers the application's routes against the specified <see cref="RouteCollection"/>.
        /// </summary>
        /// <param name="routes">The route collection to update.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Home", "", new { controller = "Home", action = "Index" });
            routes.MapRoute("Upload InfoRefs", "new/info/refs", new { controller = "Upload", action = "InfoRefs" });
            routes.MapRoute("Upload ReceivePack", "new/git-receive-pack", new { controller = "Upload", action = "ReceivePack" });
        }
    }
}
