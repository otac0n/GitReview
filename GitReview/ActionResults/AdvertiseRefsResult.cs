// -----------------------------------------------------------------------
// <copyright file="AdvertiseRefsResult.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.ActionResults
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web.Mvc;
    using LibGit2Sharp;

    /// <summary>
    /// Advertises an empty set of refs to smart http clients.
    /// </summary>
    public class AdvertiseRefsResult : ActionResult, IDisposable
    {
        private readonly Repository repo;
        private readonly string service;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvertiseRefsResult"/> class.
        /// </summary>
        /// <param name="service">The service being advertised.</param>
        /// <param name="repo">The <see cref="Repository"/> whose refs are being advertised.</param>
        public AdvertiseRefsResult(string service, Repository repo)
        {
            this.service = service;
            this.repo = repo;
        }

        ~AdvertiseRefsResult()
        {
            this.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.repo.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;

            response.StatusCode = 200;
            response.ContentType = "application/x-" + this.service + "-advertisement";
            response.BinaryWrite(ProtocolUtils.PacketLine("# service=" + this.service + "\n"));
            response.BinaryWrite(ProtocolUtils.EndMarker);

            var ids = new SortedSet<string>(this.repo.Refs.Select(r => r.TargetIdentifier));

            var first = true;
            foreach (var id in ids)
            {
                var line = first
                    ? string.Format("{0} refs/anonymous/{0}\0{1}\n", id, this.GetCapabilities())
                    : string.Format("{0} refs/anonymous/{0}\n", id);

                response.BinaryWrite(ProtocolUtils.PacketLine(line));

                first = false;
            }

            if (first)
            {
                var line = string.Format("{0} capabilities^{}\0{1}\n", ProtocolUtils.ZeroId, this.GetCapabilities());

                response.BinaryWrite(ProtocolUtils.PacketLine(line));
            }

            response.BinaryWrite(ProtocolUtils.EndMarker);
            response.End();
        }

        private string GetCapabilities()
        {
            var c = new StringBuilder();

            switch (this.service)
            {
                case "git-receive-pack":
                    c.Append(ReceivePackResult.Capabilities);
                    break;
            }

            if (c.Length > 0)
            {
                c.Append(' ');
            }

            var n = Assembly.GetExecutingAssembly().GetName();
            c.Append("agent=").Append(n.Name).Append('/').Append(n.Version);

            return c.ToString();
        }
    }
}
