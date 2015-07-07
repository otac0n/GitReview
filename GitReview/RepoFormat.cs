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
        public static readonly string DestinationRef = "refs/heads/reviews/{prefix}/{revision}/destination";

        /// <summary>
        /// Gets a string describing the format of source refs in the repository.
        /// </summary>
        public static readonly string SourceRef = "refs/heads/reviews/{prefix}/{revision}/source";

        /// <summary>
        /// Gets the canonical name of the specified ref.
        /// </summary>
        /// <param name="refPrefix">The ref prefix.</param>
        /// <param name="revision">The revision ID.</param>
        /// <returns>The canonical ref name.</returns>
        public static string FormatDestinationRef(string refPrefix, int revision)
        {
            return DestinationRef.Replace("{prefix}", refPrefix).Replace("{revision}", revision.ToString());
        }

        /// <summary>
        /// Gets the canonical name of the specified ref.
        /// </summary>
        /// <param name="refPrefix">The ref prefix.</param>
        /// <param name="revision">The revision ID.</param>
        /// <returns>The canonical ref name.</returns>
        public static string FormatSourceRef(string refPrefix, int revision)
        {
            return SourceRef.Replace("{prefix}", refPrefix).Replace("{revision}", revision.ToString());
        }

        /// <summary>
        /// Gets all of the revisions in a review.
        /// </summary>
        /// <param name="repo">The repository.</param>
        /// <param name="refPrefix">The review's ref prefix.</param>
        /// <returns>An enumerable collection of revisions.</returns>
        public static IEnumerable<Revision> GetRevisions(this Repository repo, string refPrefix)
        {
            var revision = 1;
            while (true)
            {
                var sourceRef = repo.Refs[FormatSourceRef(refPrefix, revision)];
                var destinationRef = repo.Refs[FormatDestinationRef(refPrefix, revision)];

                if (sourceRef == null || destinationRef == null)
                {
                    yield break;
                }

                yield return new Revision
                {
                    Id = revision,
                    Source = sourceRef.ResolveToDirectReference(),
                    Destination = destinationRef.ResolveToDirectReference(),
                };

                revision++;
            }
        }
    }
}
