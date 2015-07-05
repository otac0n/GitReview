// -----------------------------------------------------------------------
// <copyright file="UploadController.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.Controllers
{
    using System.Net;
    using System.Web.Mvc;
    using GitReview.ActionResults;
    using LibGit2Sharp;

    /// <summary>
    /// Provides git client support.
    /// </summary>
    public class UploadController : Controller
    {
        /// <summary>
        /// List refs contained in the repository.
        /// </summary>
        /// <param name="service">The service being requested. Must be "git-receive-pack".</param>
        /// <returns>A <see cref="AdvertiseRefsResult"/></returns>
        public ActionResult InfoRefs(string service)
        {
            if (service != "git-receive-pack")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var repo = new Repository(GitReviewApplication.RepositoryPath);

            return new AdvertiseRefsResult(service, repo);
        }

        /// <summary>
        /// Accepts objects from the client over the current HTTP connection.
        /// </summary>
        /// <returns>A <see cref="ReceivePackResult"/>.</returns>
        [HttpPost]
        public ActionResult ReceivePack()
        {
            var repo = new Repository(GitReviewApplication.RepositoryPath);

            return new ReceivePackResult(repo);
        }
    }
}
