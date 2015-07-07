// -----------------------------------------------------------------------
// <copyright file="ReviewController.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.Controllers
{
    using System.Web.Http;
    using GitReview.Models;

    /// <summary>
    /// Provides an API for working with reviews.
    /// </summary>
    public class ReviewController : ApiController
    {
        /// <summary>
        /// Gets the specified review.
        /// </summary>
        /// <param name="id">The ID of the review to find.</param>
        /// <returns>The specified review.</returns>
        [Route("reviews/{id}")]
        public object Get(string id)
        {
            return new
            {
                Reviews = new[]
                {
                    new Review { Id = id },
                }
            };
        }
    }
}
