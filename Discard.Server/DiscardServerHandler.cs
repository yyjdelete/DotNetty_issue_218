// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Discard.Server
{
    using System;
    using DotNetty.Transport.Channels;
    using DotNetty.Buffers;
    using Examples.Common;
    using System.Threading;

    public class DiscardServerHandler : SimpleChannelInboundHandler<IByteBuffer>
    {
        int index = 0;
        private readonly int pkgLen;

        public DiscardServerHandler(int pkgLen)
        {
            this.pkgLen = pkgLen;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            ToggleAutoRead(context.Channel);
        }

        private void ToggleAutoRead(IChannel channel)
        {
            if (!channel.Active)
                return;

            //1. Disable autoRead to fill up the sendBuf of client/recvBuf of server, so latter call use IOCP;
            //2. Enable autoRead to make IOCP callback
            var autoRead = !channel.Configuration.AutoRead;
            channel.Configuration.AutoRead = autoRead;
            if (autoRead)
                channel.Read();
            channel.EventLoop.Schedule(
                (state) => ToggleAutoRead((IChannel)state),
                channel,
                TimeSpan.FromMilliseconds(100));
        }

        protected override void ChannelRead0(IChannelHandlerContext context, IByteBuffer message)
        {
            if (message.ReadableBytes != pkgLen)
            {
                Console.Error.WriteLine($"Pkg length mismatch, expected {pkgLen}, actual {message.ReadableBytes}");
                return;
            }
            var val = message.GetInt(0);
            if (val != index)
            {
                Console.Error.WriteLine($"Pkg seq mismatch, expected {index}, actual {val}");
                index = val + 1;
                return;
            }
            Console.WriteLine("Recv: " + index);

            ++index;
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.Error.WriteLine("{0}", e.ToString());
            ctx.CloseAsync();
        }
    }
}