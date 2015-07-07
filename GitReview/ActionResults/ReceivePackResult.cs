// -----------------------------------------------------------------------
// <copyright file="ReceivePackResult.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.ActionResults
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using GitReview.Models;
    using LibGit2Sharp;

    /// <summary>
    /// Accepts objects from the client over the current HTTP connection.
    /// </summary>
    public class ReceivePackResult : ActionResult
    {
        private const string DestinationRefActual = "refs/heads/reviews/{id}/{version}/destination";
        private const string DestinationRefName = "refs/heads/destination";
        private const string Service = "git-receive-pack";
        private const string SourceRefActual = "refs/heads/reviews/{id}/{version}/source";
        private const string SourceRefName = "refs/heads/source";
        private Repository repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivePackResult"/> class.
        /// </summary>
        /// <param name="repo">The repository that will receive the packs.</param>
        public ReceivePackResult(Repository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Gets the capabilities of this service.
        /// </summary>
        public static string Capabilities
        {
            get { return "atomic report-status side-band-64k"; }
        }

        /// <inheritdoc />
        public override void ExecuteResult(ControllerContext context)
        {
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            response.ContentType = "application/x-" + Service + "-result";
            response.BufferOutput = false;

            using (var input = request.GetBufferlessInputStream(disableMaxRequestLength: true))
            {
                var stream = request.Headers["Content-Encoding"] == "gzip"
                    ? new GZipStream(input, CompressionMode.Decompress)
                    : input;

                this.ReceivePack(context, stream, response);
                response.End();
            }
        }

        private static Dictionary<ProtocolUtils.UpdateRequest, string> ReadRequests(List<ProtocolUtils.UpdateRequest> requests, out ProtocolUtils.UpdateRequest source, out ProtocolUtils.UpdateRequest destination)
        {
            var errors = requests.ToDictionary(r => r, r =>
            {
                if (r.TargetIdentifier == null)
                {
                    return "delete unsupported";
                }
                else if (r.SourceIdentifier != null)
                {
                    return "update unsupported";
                }
                else if (r.CanonicalName != SourceRefName && r.CanonicalName != DestinationRefName)
                {
                    return "ref unsupported";
                }

                return null;
            });

            if (requests.Count == 2)
            {
                source = requests.FirstOrDefault(r => r.CanonicalName == SourceRefName);
                destination = requests.FirstOrDefault(r => r.CanonicalName == DestinationRefName);
            }
            else
            {
                source = null;
                destination = null;
            }

            return errors;
        }

        private static void ReportFailure(HttpResponseBase response, int? reportBand, Dictionary<ProtocolUtils.UpdateRequest, string> errors, string message)
        {
            if (reportBand == ProtocolUtils.ErrorBand)
            {
                response.BinaryWrite(ProtocolUtils.Band(ProtocolUtils.MessageBand,
                    ProtocolUtils.DefaultEncoding.GetBytes(string.Format("{0}\n", message))));

                foreach (var e in errors)
                {
                    response.BinaryWrite(ProtocolUtils.Band(ProtocolUtils.MessageBand,
                        ProtocolUtils.DefaultEncoding.GetBytes(string.Format("{0} ({1})\n", e.Key.CanonicalName, e.Value ?? "not created, see other errors"))));
                }

                response.BinaryWrite(ProtocolUtils.Band(ProtocolUtils.ErrorBand, ProtocolUtils.DefaultEncoding.GetBytes("code review creation aborted\n")));
            }
            else
            {
                var status = new List<byte[]>();

                status.Add(ProtocolUtils.PacketLine(string.Format("unpack {0}\n", message)));

                foreach (var e in errors)
                {
                    status.Add(ProtocolUtils.PacketLine(string.Format("ng {0} {1}\n", e.Key.CanonicalName, e.Value ?? "not created, see other errors")));
                }

                status.Add(ProtocolUtils.EndMarker);

                response.BinaryWrite(ProtocolUtils.Band(reportBand, status));
            }
        }

        private static void ReportSuccess(HttpResponseBase response, int? reportBand)
        {
            response.BinaryWrite(ProtocolUtils.Band(reportBand,
                ProtocolUtils.PacketLine("unpack ok\n"),
                ProtocolUtils.PacketLine("ok " + DestinationRefName + "\n"),
                ProtocolUtils.PacketLine("ok " + SourceRefName + "\n"),
                ProtocolUtils.EndMarker));
        }

        private MemoryStream ReadPack(IList<ProtocolUtils.UpdateRequest> commands, HashSet<string> capabilities, Stream input)
        {
            var startInfo = new ProcessStartInfo(GitReviewApplication.GitPath, "receive-pack --stateless-rpc .")
            {
                WorkingDirectory = GitReviewApplication.RepositoryPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = ProtocolUtils.DefaultEncoding,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var output = new MemoryStream();
            using (var git = Process.Start(startInfo))
            {
                var t = Task.Factory.StartNew(() =>
                {
                    git.StandardOutput.BaseStream.CopyTo(output);
                });

                var first = true;
                foreach (var c in commands)
                {
                    var message = string.Format("{0} {1} {2}",
                        c.SourceIdentifier ?? ProtocolUtils.ZeroId,
                        c.TargetIdentifier ?? ProtocolUtils.ZeroId,
                        c.CanonicalName);

                    if (first)
                    {
                        message += "\0atomic report-status";
                    }

                    var data = ProtocolUtils.PacketLine(message);
                    git.StandardInput.BaseStream.Write(data, 0, data.Length);

                    first = false;
                }

                var marker = ProtocolUtils.EndMarker;
                git.StandardInput.BaseStream.Write(marker, 0, marker.Length);

                input.CopyTo(git.StandardInput.BaseStream);
                git.StandardInput.Close();
                git.WaitForExit();
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        private void ReceivePack(ControllerContext context, Stream input, HttpResponseBase response)
        {
            var capabilities = new HashSet<string>(Capabilities.Split(' '));
            var requests = ProtocolUtils.ParseUpdateRequests(input, capabilities).ToList();
            if (requests.Count == 0)
            {
                response.BinaryWrite(ProtocolUtils.EndMarker);
                return;
            }

            var reportStatus = capabilities.Contains("report-status");
            var useSideBand = capabilities.Contains("side-band-64k");
            var reportBand = useSideBand ? ProtocolUtils.PrimaryBand : (int?)null;

            try
            {
                ProtocolUtils.UpdateRequest source;
                ProtocolUtils.UpdateRequest destination;
                var errors = ReadRequests(requests, out source, out destination);
                if (errors.Any(e => e.Value != null) || source == null || destination == null)
                {
                    if (reportStatus || useSideBand)
                    {
                        ReportFailure(response, reportStatus ? reportBand : ProtocolUtils.ErrorBand, errors, "expected source and destination branches to be pushed");
                    }

                    return;
                }

                var id = Guid.NewGuid().ToString();
                source = new ProtocolUtils.UpdateRequest(
                    source.SourceIdentifier,
                    source.TargetIdentifier,
                    SourceRefActual.Replace("{id}", id).Replace("{version}", "1"));
                destination = new ProtocolUtils.UpdateRequest(
                    destination.SourceIdentifier,
                    destination.TargetIdentifier,
                    DestinationRefActual.Replace("{id}", id).Replace("{version}", "1"));

                var output = this.ReadPack(new[] { source, destination }, capabilities, input);
                var line = ProtocolUtils.ReadPacketLine(output).TrimEnd('\n');
                if (line != "unpack ok")
                {
                    line = line.Substring("unpack ".Length);

                    if (reportStatus || useSideBand)
                    {
                        ReportFailure(response, reportStatus ? reportBand : ProtocolUtils.ErrorBand, errors, line);
                    }

                    return;
                }

                string name;
                using (var ctx = new ReviewContext())
                {
                    name = Task.Factory.StartNew(() => ctx.GetNextReviewId().Result).Result;

                    ctx.Reviews.Add(new Review
                    {
                        Id = name,
                        RefPrefix = id,
                    });
                    ctx.SaveChanges();
                }

                if (useSideBand)
                {
                    var url = new UrlHelper(context.RequestContext).Action("Index", "Home", null, context.HttpContext.Request.Url.Scheme) + "#/" + name;
                    var message = string.Format("code review created:\n\n\t{0}\n\n", url);
                    response.BinaryWrite(ProtocolUtils.Band(ProtocolUtils.MessageBand, Encoding.UTF8.GetBytes(message)));
                }

                if (reportStatus)
                {
                    ReportSuccess(response, reportBand);
                }
            }
            finally
            {
                if (useSideBand)
                {
                    response.BinaryWrite(ProtocolUtils.EndMarker);
                }
            }
        }
    }
}
