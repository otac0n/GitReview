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
    using System.Diagnostics;
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
                    var mergeBases = new Dictionary<Revision, HashSet<Commit>>(revisions.Count);
                    var ignored = new Dictionary<Revision, HashSet<Commit>>(revisions.Count);

                    foreach (var rev in revisions)
                    {
                        var revReachable = new HashSet<Commit>();
                        var revMergeBases = new HashSet<Commit>();
                        var revIgnored = new HashSet<Commit>();

                        var sourceCommit = (Commit)rev.Source.Target;
                        var destinationCommit = (Commit)rev.Destination.Target;
                        GetMergeDetails(repo, sourceCommit, destinationCommit, out revReachable, out revMergeBases, out revIgnored);

                        ignored[rev] = revIgnored;
                        mergeBases[rev] = revMergeBases;
                        commits.UnionWith(revReachable);
                    }

                    return new
                    {
                        Review = new
                        {
                            Id = review.Id,
                            Revisions = revisions.Select(r => review.Id + ":" + r.Id)
                        },
                        Revisions = revisions.Select(r => new
                        {
                            Id = review.Id + ":" + r.Id,
                            Source = r.Source.TargetIdentifier,
                            Destination = r.Destination.TargetIdentifier,
                            MergeBases = mergeBases[r].Select(b => b.Sha).ToList(),
                            Ignored = ignored[r].Select(i => i.Sha).ToList(),
                        }).ToList(),
                        Commits = commits.Select(c => new
                        {
                            Id = c.Sha,
                            Author = c.Author.Email,
                            AuthoredAt = c.Author.When,
                            Committer = c.Committer.Email,
                            CommittedAt = c.Committer.When,
                            Message = c.Message,
                            Parents = c.Parents.Select(p => p.Sha).ToList(),
                        }).ToList(),
                    };
                }
            }
        }

        private static void GetMergeDetails(Repository repo, Commit source, Commit destination, out HashSet<Commit> allReachable, out HashSet<Commit> mergeBases, out HashSet<Commit> ignored)
        {
            allReachable = new HashSet<Commit>();
            allReachable.Add(source);
            allReachable.Add(destination);

            ignored = new HashSet<Commit>();

            mergeBases = GitMergeBaseAll(repo, source, destination);
            allReachable.UnionWith(mergeBases);

            if (mergeBases.Count > 0)
            {
                var ancestryPath = GitLogAncestryPath(repo, source, destination, mergeBases);
                allReachable.UnionWith(ancestryPath);
            }

            var mergeBasesClosure = mergeBases;
            var allReachableClosure = allReachable;
            ignored.UnionWith(allReachable
                .Where(r => !mergeBasesClosure.Contains(r))
                .SelectMany(r => r.Parents)
                .Distinct()
                .Where(p => !allReachableClosure.Contains(p)));

            allReachable.UnionWith(ignored);
        }

        private static HashSet<Commit> GitLogAncestryPath(Repository repo, Commit source, Commit destination, HashSet<Commit> mergeBases)
        {
            var startInfo = new ProcessStartInfo(GitReviewApplication.GitPath, string.Format("log --format=%H --ancestry-path {0} {1} {2}", source.Sha, destination.Sha, string.Join(" ", mergeBases.Select(m => "--not " + m.Sha))))
            {
                WorkingDirectory = GitReviewApplication.RepositoryPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            using (var git = Process.Start(startInfo))
            {
                var lines = git.StandardOutput.ReadToEnd().TrimEnd('\n').Split('\n');
                return new HashSet<Commit>(lines.Select(s => (Commit)repo.Lookup(s)));
            }
        }

        private static HashSet<Commit> GitMergeBaseAll(Repository repo, Commit source, Commit destination)
        {
            var startInfo = new ProcessStartInfo(GitReviewApplication.GitPath, string.Format("merge-base --all {0} {1}", source.Sha, destination.Sha))
            {
                WorkingDirectory = GitReviewApplication.RepositoryPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            using (var git = Process.Start(startInfo))
            {
                var lines = git.StandardOutput.ReadToEnd().TrimEnd('\n').Split('\n');
                return new HashSet<Commit>(lines.Select(s => (Commit)repo.Lookup(s)));
            }
        }
    }
}
