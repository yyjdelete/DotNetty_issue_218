using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace Discard.Server
{
    public class FixedLengthDecoder : ByteToMessageDecoder
    {
        private readonly int pkgLen;

        public FixedLengthDecoder(int pkgLen)
        {
            this.pkgLen = pkgLen;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            while (input.ReadableBytes >= this.pkgLen)
            {
                output.Add(input.ReadBytes(this.pkgLen));
            }
        }
    }
}