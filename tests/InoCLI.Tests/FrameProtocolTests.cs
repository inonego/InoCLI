using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace InoCLI.Tests
{
   // ============================================================
   /// <summary>
   /// Tests for frame encode/decode round-trip via a mock transport.
   /// </summary>
   // ============================================================
   public class FrameProtocolTests
   {

   #region Round-trip

      [Fact]
      public void SendReceive_RoundTrip()
      {
         var transport = new MemoryTransport();

         FrameProtocol.Send(transport, "{\"group\":\"ping\"}");

         string result = FrameProtocol.Receive(transport);

         Assert.Equal("{\"group\":\"ping\"}", result);
      }

      [Fact]
      public void SendReceive_Unicode()
      {
         var transport = new MemoryTransport();
         string input  = "{\"message\":\"한글 테스트 🎉\"}";

         FrameProtocol.Send(transport, input);

         string result = FrameProtocol.Receive(transport);

         Assert.Equal(input, result);
      }

      [Fact]
      public void SendReceive_EmptyBody()
      {
         var transport = new MemoryTransport();

         FrameProtocol.Send(transport, "");

         string result = FrameProtocol.Receive(transport);

         Assert.Equal("", result);
      }

      [Fact]
      public void SendReceive_LargePayload()
      {
         var transport = new MemoryTransport();
         string input  = new string('x', 100_000);

         FrameProtocol.Send(transport, input);

         string result = FrameProtocol.Receive(transport);

         Assert.Equal(input, result);
      }

   #endregion

   #region MemoryTransport

      // ============================================================
      /// <summary>
      /// In-memory ITransport for testing frame protocol round-trips.
      /// </summary>
      // ============================================================
      private class MemoryTransport : ITransport
      {

         public bool IsConnected => true;

         private readonly List<byte> buffer = new List<byte>();
         private int readPos = 0;

         public void Connect() {}

         public void WriteFrame(byte[] data)
         {
            // Write 4-byte BE length prefix + body (same as real transports)
            byte[] lenBuf = new byte[4];

            lenBuf[0] = (byte)((data.Length >> 24) & 0xFF);
            lenBuf[1] = (byte)((data.Length >> 16) & 0xFF);
            lenBuf[2] = (byte)((data.Length >> 8) & 0xFF);
            lenBuf[3] = (byte)(data.Length & 0xFF);

            buffer.AddRange(lenBuf);
            buffer.AddRange(data);
         }

         public byte[] ReadFrame()
         {
            byte[] lenBuf = new byte[4];

            for (int i = 0; i < 4; i++)
            {
               lenBuf[i] = buffer[readPos++];
            }

            int length = (lenBuf[0] << 24) | (lenBuf[1] << 16) | (lenBuf[2] << 8) | lenBuf[3];

            byte[] body = new byte[length];

            for (int i = 0; i < length; i++)
            {
               body[i] = buffer[readPos++];
            }

            return body;
         }

         public void Dispose() {}

      }

   #endregion

   }
}
