// -----------------------------------------------------------------------
// <copyright file="ProtocolUtils.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview.ActionResults
{
    using System.Text;

    /// <summary>
    /// Provides utilities for implementing the Git Smart HTTP protocol.
    /// </summary>
    public static class ProtocolUtils
    {
        /// <summary>
        /// Gets the Windows 28591 (iso-8859-1) encoding.
        /// </summary>
        public static readonly Encoding DefaultEncoding = Encoding.GetEncoding(28591);

        private const int HeaderSize = 4;

        /// <summary>
        /// Formats a string according to the pkt-line format.
        /// </summary>
        /// <param name="packet">The data to format.</param>
        /// <param name="encoding">The encoding to use. If none is specified, the default encoding will be used.</param>
        /// <returns>The data in the pkt-line format.</returns>
        public static byte[] PacketLine(string packet, Encoding encoding = null)
        {
            encoding = encoding ?? ProtocolUtils.DefaultEncoding;

            var payload = new byte[encoding.GetByteCount(packet) + HeaderSize];
            encoding.GetBytes(packet, 0, packet.Length, payload, HeaderSize);

            var header = payload.Length.ToString("x");
            var offset = HeaderSize - header.Length;

            for (var i = 0; i < offset; i++)
            {
                payload[i] = (byte)'0';
            }

            for (var i = 0; i < header.Length; i++)
            {
                payload[offset + i] = (byte)header[i];
            }

            return payload;
        }

        /// <summary>
        /// Gets the magic "0000" end pkt-line marker.
        /// </summary>
        /// <returns>The end pkt-line marker.</returns>
        public static byte[] PacketLineEndMarker()
        {
            return new[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0' };
        }
    }
}
