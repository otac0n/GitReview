// -----------------------------------------------------------------------
// <copyright file="ReviewContext.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview
{
    using System.Data.Entity;
    using System.Threading.Tasks;
    using GitReview.Models;

    /// <summary>
    /// Repository containing review data.
    /// </summary>
    public class ReviewContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewContext"/> class.
        /// </summary>
        public ReviewContext()
            : base("GitReview.Database")
        {
        }

        /// <summary>
        /// Gets or sets the collection of reviews.
        /// </summary>
        public DbSet<Review> Reviews { get; set; }

        /// <summary>
        /// Gets the next available review ID.
        /// </summary>
        /// <returns>A string containing a unique review ID.</returns>
        public async Task<string> GetNextReviewId()
        {
            var seq = await this.Database.SqlQuery<long>("SELECT NEXT VALUE FOR [ReviewId]").SingleAsync();
            return seq.ToString(@"\c\r0000");
        }
    }
}
