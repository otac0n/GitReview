// -----------------------------------------------------------------------
// <copyright file="Revision.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.Models
{
    using LibGit2Sharp;

    /// <summary>
    /// Encapsulates the refs for a specific revision of a review.
    /// </summary>
    public class Revision
    {
        /// <summary>
        /// Gets or sets the destination ref.
        /// </summary>
        public DirectReference Destination { get; set; }

        /// <summary>
        /// Gets or sets the ID of the revision.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the source ref.
        /// </summary>
        public DirectReference Source { get; set; }
    }
}
