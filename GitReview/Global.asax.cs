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
    using System.IO;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using LibGit2Sharp;

    /// <summary>
    /// The GitReview application.
    /// </summary>
    public class GitReviewApplication : HttpApplication
    {
        /// <summary>
        /// Gets the path to the git command line executable.
        /// </summary>
        public static string GitPath
        {
            get { return ConfigurationManager.AppSettings["GitPath"]; }
        }

        /// <summary>
        /// Gets the path of the shared repository.
        /// </summary>
        public static string RepositoryPath
        {
            get { return HostingEnvironment.MapPath(ConfigurationManager.AppSettings["RepositoryPath"]); }
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        protected virtual void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var path = GitReviewApplication.RepositoryPath;
            if (!Directory.Exists(path))
            {
                Repository.Init(path, isBare: true);
            }
        }
    }
}
