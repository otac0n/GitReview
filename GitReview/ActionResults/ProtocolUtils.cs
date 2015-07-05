// -----------------------------------------------------------------------
// <copyright file="ProtocolUtils.cs" company="(none)">
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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Provides utilities for implementing the Git Smart HTTP protocol.
    /// </summary>
    public static class ProtocolUtils
    {
        /// <summary>
        /// The band reserved for sending errors.
        /// </summary>
        public const int ErrorBand = 3;

        /// <summary>
        /// The band reserved for sending messages.
        /// </summary>
        public const int MessageBand = 2;

        /// <summary>
        /// The band reserved as the primary communication band.
        /// </summary>
        public const int PrimaryBand = 1;

        /// <summary>
        /// Gets the Windows 28591 (iso-8859-1) encoding.
        /// </summary>
        public static readonly Encoding DefaultEncoding = Encoding.GetEncoding(28591);

        /// <summary>
        /// Gets the SHA1 hash of an empty tree object.
        /// </summary>
        public static readonly string EmptyTreeId = "4b825dc642cb6eb9a060e54bf8d69288fbee4904";

        /// <summary>
        /// Gets an all-zero hash.
        /// </summary>
        public static readonly string ZeroId = "0000000000000000000000000000000000000000";

        private const int HeaderSize = 4;
        private const int MaxBandPayloadSize = 0xFFFF - (HeaderSize + 1);
        private const int MaxPayloadSize = 0xFFFF - HeaderSize;

        /// <summary>
        /// Gets the magic "0000" end pkt-line marker.
        /// </summary>
        /// <returns>The end pkt-line marker.</returns>
        public static byte[] EndMarker
        {
            get { return new[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0' }; }
        }

        /// <summary>
        /// Formats pkt-line data for transmission in a side-band.
        /// </summary>
        /// <param name="band">The band the packet will be sent on. If none is specified, no band will be used.</param>
        /// <param name="packets">The packets that will be sent.</param>
        /// <returns>The side-band formatted data.</returns>
        public static byte[] Band(int? band, params byte[][] packets)
        {
            return Band(band, packets.AsEnumerable());
        }

        /// <summary>
        /// Formats pkt-line data for transmission in a side-band.
        /// </summary>
        /// <param name="band">The band the packet will be sent on. If none is specified, no band will be used.</param>
        /// <param name="packets">The packets that will be sent.</param>
        /// <returns>The side-band formatted data.</returns>
        public static byte[] Band(int? band, IEnumerable<byte[]> packets)
        {
            var totalSize = 0;
            var packetList = new List<byte[]>();

            foreach (var p in packets)
            {
                totalSize += p.Length;
                packetList.Add(p);
            }

            if (band == null)
            {
                var data = new byte[totalSize];
                var dataOffset = 0;

                foreach (var p in packets)
                {
                    Array.Copy(p, 0, data, dataOffset, p.Length);
                    dataOffset += p.Length;
                }

                return data;
            }
            else
            {
                var chunks = (totalSize + MaxBandPayloadSize - 1) / MaxBandPayloadSize;
                var data = new byte[(chunks * (HeaderSize + 1)) + totalSize];
                var dataOffset = 0;
                var packet = 0;
                var packetOffset = 0;

                var remaining = totalSize;
                while (remaining > 0)
                {
                    var toWrite = Math.Min(remaining, MaxBandPayloadSize);
                    WriteHeader(data, dataOffset, toWrite + HeaderSize + 1);
                    dataOffset += HeaderSize;
                    data[dataOffset++] = (byte)band;

                    while (toWrite > 0)
                    {
                        var p = packetList[packet];
                        var n = Math.Min(p.Length - packetOffset, toWrite);

                        Array.Copy(p, packetOffset, data, dataOffset, n);

                        toWrite -= n;
                        remaining -= n;
                        packetOffset += n;
                        dataOffset += n;
                        if (packetOffset >= p.Length)
                        {
                            packet++;
                            packetOffset = 0;
                        }
                    }
                }

                return data;
            }
        }

        /// <summary>
        /// Formats a string according to the pkt-line format.
        /// </summary>
        /// <param name="packet">The data to format.</param>
        /// <param name="encoding">The encoding to use. If none is specified, the default encoding will be used.</param>
        /// <returns>The data in the pkt-line format.</returns>
        public static byte[] PacketLine(string packet, Encoding encoding = null)
        {
            encoding = encoding ?? ProtocolUtils.DefaultEncoding;

            var data = new byte[encoding.GetByteCount(packet) + HeaderSize];
            encoding.GetBytes(packet, 0, packet.Length, data, HeaderSize);
            WriteHeader(data, 0, data.Length);

            return data;
        }

        /// <summary>
        /// Parses a list of reference update requests from the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> that contains the requests.</param>
        /// <param name="capabilities">The capabilities supported by the requester. This will be modified before yielding the first <see cref="UpdateRequest"/> to contain only capabilities that are shared by the client.</param>
        /// <returns>An enumerable collection of <see cref="UpdateRequest">UpdateRequests</see>.</returns>
        public static IEnumerable<UpdateRequest> ParseUpdateRequests(Stream stream, HashSet<string> capabilities)
        {
            var requests = new List<UpdateRequest>();

            var first = true;
            while (true)
            {
                var line = ProtocolUtils.ReadPacketLine(stream);
                if (line == null)
                {
                    break;
                }

                line = line.TrimEnd('\n');

                if (first)
                {
                    var parts = line.Split(new[] { '\0' }, 2);
                    if (parts.Length < 2)
                    {
                        throw new ProtocolException("Capabilities not specified.");
                    }

                    line = parts[0];
                    var specified = new HashSet<string>(parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    capabilities.IntersectWith(specified);

                    specified.ExceptWith(capabilities);
                    specified.RemoveWhere(s => s.StartsWith("agent="));
                    if (specified.Count != 0)
                    {
                        throw new ProtocolException("Unrecognized capability.");
                    }
                }

                var requestParts = line.Split(new[] { ' ' }, 3);
                var source = requestParts[0];
                var target = requestParts[1];
                var name = requestParts[2];

                yield return new UpdateRequest(source == ProtocolUtils.ZeroId ? null : source, target == ProtocolUtils.ZeroId ? null : target, name);
                first = false;
            }

            if (first)
            {
                capabilities.Clear();
            }
        }

        /// <summary>
        /// Reads a single pkt-line formatted packet from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use. If none is specified, the default encoding will be used.</param>
        /// <returns>The next packet available in the specified <see cref="Stream"/>.</returns>
        public static string ReadPacketLine(Stream stream, Encoding encoding = null)
        {
            encoding = encoding ?? ProtocolUtils.DefaultEncoding;

            var header = new byte[HeaderSize];
            stream.Read(header, 0, HeaderSize);

            int length;
            if (!int.TryParse(
                ProtocolUtils.DefaultEncoding.GetString(header),
                NumberStyles.AllowHexSpecifier, // This throws in all of our error cases: negative sign, leading spaces, and even the '0x' modifier.
                CultureInfo.InvariantCulture,
                out length))
            {
                throw new ProtocolException("Invalid pkt-line.");
            }

            if (length == 0)
            {
                return null;
            }
            else if (length < HeaderSize)
            {
                throw new ProtocolException("Invalid pkt-line.");
            }
            else if (length == HeaderSize)
            {
                return "";
            }

            length -= HeaderSize;
            var data = new byte[length];

            stream.Read(data, 0, length);
            return encoding.GetString(data);
        }

        private static void WriteHeader(byte[] data, int offset, int value)
        {
            var header = value.ToString("x").PadLeft(HeaderSize, '0');
            for (var i = 0; i < HeaderSize; i++)
            {
                data[offset + i] = (byte)header[i];
            }
        }

        /// <summary>
        /// Thrown when an error is detected in the interpretation of the protocol.
        /// </summary>
        public class ProtocolException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProtocolException"/> class with a specified error message.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public ProtocolException(string message)
                : base(message)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ProtocolException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
            public ProtocolException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        /// <summary>
        /// Represents a request to update a <see cref="Reference"/>.
        /// </summary>
        [DebuggerDisplay("{DebuggerDisplay,nq}")]
        public class UpdateRequest
        {
            private readonly string canonicalName;
            private readonly string sourceIdentifier;
            private readonly string targetIdentifier;

            /// <summary>
            /// Initializes a new instance of the <see cref="UpdateRequest"/> class.
            /// </summary>
            /// <param name="sourceIdentifier">The currently declared target of the reference.</param>
            /// <param name="targetIdentifier">The target that the reference should be updated to refer to.</param>
            /// <param name="canonicalName">The full name of the reference to update.</param>
            public UpdateRequest(string sourceIdentifier, string targetIdentifier, string canonicalName)
            {
                this.sourceIdentifier = sourceIdentifier;
                this.targetIdentifier = targetIdentifier;
                this.canonicalName = canonicalName;
            }

            /// <summary>
            /// Gets the full name of the reference to update.
            /// </summary>
            public string CanonicalName
            {
                get { return this.canonicalName; }
            }

            /// <summary>
            /// Gets the currently declared target of the reference.
            /// </summary>
            public string SourceIdentifier
            {
                get { return this.sourceIdentifier; }
            }

            /// <summary>
            /// Gets the target that the reference should be updated to refer to.
            /// </summary>
            public string TargetIdentifier
            {
                get { return this.targetIdentifier; }
            }

            private string DebuggerDisplay
            {
                get
                {
                    return
                        this.sourceIdentifier == null ? "create " + this.canonicalName + " => " + this.targetIdentifier :
                        this.targetIdentifier == null ? "delete " + this.canonicalName + " (was " + this.sourceIdentifier + ")" :
                        "delete " + this.canonicalName + " => " + this.targetIdentifier + " (was " + this.sourceIdentifier + ")";
                }
            }
        }
    }
}
