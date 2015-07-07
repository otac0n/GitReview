// -----------------------------------------------------------------------
// <copyright file="RepoFormat.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview
{
    /// <summary>
    /// Provides utility methods for working with the repository.
    /// </summary>
    public static class RepoFormat
    {
        /// <summary>
        /// Gets a string describing the format of destination refs in the repository.
        /// </summary>
        public static readonly string DestinationRef = "refs/heads/reviews/{id}/{version}/destination";

        /// <summary>
        /// Gets a string describing the format of source refs in the repository.
        /// </summary>
        public static readonly string SourceRef = "refs/heads/reviews/{id}/{version}/source";
    }
}
