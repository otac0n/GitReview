// -----------------------------------------------------------------------
// <copyright file="BundleConfig.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview
{
    using System.Web.Optimization;

    /// <summary>
    /// Contains bundle registration.
    /// </summary>
    public static class BundleConfig
    {
        /// <summary>
        /// Registers the application's bundles against the specified <see cref="BundleCollection"/>.
        /// </summary>
        /// <param name="bundles">The bundle collection to update.</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/ember")
                .Include("~/scripts/handlebars.js")
                .IncludeDirectory("~/scripts", "ember.js", true)
                .IncludeDirectory("~/scripts", "ember-template-compiler.js", true)
                .IncludeDirectory("~/scripts", "ember-data.js", true));

            bundles.Add(new ScriptBundle("~/bundles/moment")
                .Include("~/scripts/moment-with-locales.js"));

            bundles.Add(new ScriptBundle("~/bundles/app")
                .Include("~/app/app.js")
                .Include("~/app/router.js")
                .IncludeDirectory("~/app/controllers", "*.js")
                .IncludeDirectory("~/app/models", "*.js")
                .IncludeDirectory("~/app/routes", "*.js")
                .IncludeDirectory("~/app/views", "*.js"));
        }
    }
}
