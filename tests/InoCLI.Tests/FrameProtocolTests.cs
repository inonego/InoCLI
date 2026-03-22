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

   // Uses InoCLI.MemoryTransport from Transport/MemoryTransport.cs

   }
}
