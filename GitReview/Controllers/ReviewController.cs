// -----------------------------------------------------------------------
// <copyright file="ReviewController.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

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
        public async Task<object> Get(string id)
        {
            using (var ctx = new ReviewContext())
            {
                var review = await ctx.Reviews.FindAsync(id);
                if (review == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                return new
                {
                    Reviews = new[] { review },
                };
            }
        }
    }
}
