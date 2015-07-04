// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview
{
    using System.Configuration;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// The GitReview application.
    /// </summary>
    public class GitReviewApplication : HttpApplication
    {
        public static string RepositoriesPath
        {
            get { return ConfigurationManager.AppSettings["RepositoriesPath"]; }
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        protected virtual void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
