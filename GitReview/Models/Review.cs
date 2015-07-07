// -----------------------------------------------------------------------
// <copyright file="Review.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a code review.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Gets or sets the unique identifier of the code review.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the prefix for all refs that are contained in the code review.
        /// </summary>
        [JsonIgnore]
        public string RefPrefix { get; set; }
    }
}
