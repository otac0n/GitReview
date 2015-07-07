// -----------------------------------------------------------------------
// <copyright file="RepoFormat.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview
{
    using System.Collections.Generic;
    using GitReview.Models;
    using LibGit2Sharp;

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

        /// <summary>
        /// Gets all of the revisions in a review.
        /// </summary>
        /// <param name="repo">The repository.</param>
        /// <param name="refPrefix">The review's ref prefix.</param>
        /// <returns>An enumerable collection of revisions.</returns>
        public static IEnumerable<Revision> GetRevisions(this Repository repo, string refPrefix)
        {
            var version = 1;
            while (true)
            {
                var source = SourceRef.Replace("{id}", refPrefix).Replace("{version}", version.ToString());
                var destination = DestinationRef.Replace("{id}", refPrefix).Replace("{version}", version.ToString());

                var sourceRef = repo.Refs[source];
                var destinationRef = repo.Refs[destination];

                if (sourceRef == null || destinationRef == null)
                {
                    yield break;
                }

                yield return new Revision
                {
                    Id = version,
                    Source = sourceRef.ResolveToDirectReference(),
                    Destination = destinationRef.ResolveToDirectReference(),
                };

                version++;
            }
        }
    }
}
