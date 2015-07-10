// -----------------------------------------------------------------------
// <copyright file="ReviewController.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using GitReview.Models;
    using LibGit2Sharp;

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

                List<Revision> revisions;
                var commits = new HashSet<Commit>();
                using (var repo = new Repository(GitReviewApplication.RepositoryPath))
                {
                    revisions = repo.GetRevisions(review.RefPrefix).ToList();
                    foreach (var rev in revisions)
                    {
                        var sourceCommit = (Commit)rev.Source.Target;
                        var destinationCommit = (Commit)rev.Destination.Target;

                        commits.Add(sourceCommit);
                        commits.Add(destinationCommit);

                        var mergeBase = repo.Commits.FindMergeBase(sourceCommit, destinationCommit);
                        if (mergeBase == null)
                        {
                            continue;
                        }

                        commits.Add(mergeBase);

                        var between = repo.Commits.QueryBy(new CommitFilter
                        {
                            Since = new[] { sourceCommit, destinationCommit },
                            Until = mergeBase,
                        });

                        if (between != null)
                        {
                            commits.UnionWith(between);
                        }
                    }

                    return new
                    {
                        Review = new
                        {
                            Id = review.Id,
                            Revisions = revisions.Select(r => new
                            {
                                Id = r.Id,
                                Source = r.Source.TargetIdentifier,
                                Destination = r.Destination.TargetIdentifier,
                            }).ToList(),
                            Commits = commits.Select(c => new
                            {
                                Id = c.Sha,
                                c.Author,
                                c.Committer,
                                c.Message,
                            }).ToList(),
                        },
                    };
                }
            }
        }
    }
}
