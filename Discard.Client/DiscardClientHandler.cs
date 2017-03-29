// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Discard.Client
{
    using System;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;
    using Examples.Common;
    using System.Threading;

    public class DiscardClientHandler : SimpleChannelInboundHandler<object>
    {
        IChannelHandlerContext ctx;

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            this.ctx = ctx;

            // Send the initial messages.
            this.GenerateTraffic();
        }

        protected override void ChannelRead0(IChannelHandlerContext context, object message)
        {
            // Server is supposed to send nothing, but if it sends something, discard it.
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.Error.WriteLine("{0}", e.ToString());
            this.ctx.CloseAsync();
        }

        async void GenerateTraffic()
        {
            try
            {
                var pkgLen = ServerSettings.Size;
                for (int i = 0; i < 100; ++i)
                {
                    Console.WriteLine("PreSend: " + i);
                    IByteBuffer buffer = Unpooled.WrappedBuffer(new byte[pkgLen]);
                    buffer.SetInt(0, i);
                    // Flush the outbound buffer to the socket.
                    // Once flushed, generate the same amount of traffic again.
                    await this.ctx.WriteAndFlushAsync(buffer);//.ConfigureAwait(false)
                    Console.WriteLine("PostSend: " + i);
                }
                Console.WriteLine("Test success.");
            }
            catch(Exception e)
            {
                Console.Error.WriteLine("{0}", e.ToString());
                await this.ctx.CloseAsync();
            }
        }
    }
}