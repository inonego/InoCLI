using System;
using System.Text;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// High-level frame send/receive over an ITransport.
   /// Encodes strings as UTF-8 byte frames.
   /// </summary>
   // ============================================================
   public static class FrameProtocol
   {

   #region Send / Receive

      // ------------------------------------------------------------
      /// <summary>
      /// Encodes a string as UTF-8 and writes it as a frame.
      /// </summary>
      // ------------------------------------------------------------
      public static void Send(ITransport transport, string json)
      {
         byte[] data = Encoding.UTF8.GetBytes(json);
         transport.WriteFrame(data);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Reads a frame and decodes it as a UTF-8 string.
      /// </summary>
      // ------------------------------------------------------------
      public static string Receive(ITransport transport)
      {
         byte[] data = transport.ReadFrame();
         return Encoding.UTF8.GetString(data);
      }

   #endregion

   }
}
