using System;
using System.Collections;
using System.Collections.Generic;

namespace InoCLI
{
   // ==================================================================
   /// <summary>
   /// In-memory ITransport for testing. Stores frames in a buffer.
   /// </summary>
   // ==================================================================
   public class MemoryTransport : ITransport
   {

   #region Fields

      public bool IsConnected => true;

      private readonly List<byte> buffer = new List<byte>();
      private int readPos = 0;

   #endregion

   #region ITransport

      public void Connect() {}

      // ------------------------------------------------------------
      /// <summary>
      /// Writes a length-prefixed frame to the buffer.
      /// </summary>
      // ------------------------------------------------------------
      public void WriteFrame(byte[] data)
      {
         byte[] lenBuf = new byte[4];

         lenBuf[0] = (byte)((data.Length >> 24) & 0xFF);
         lenBuf[1] = (byte)((data.Length >> 16) & 0xFF);
         lenBuf[2] = (byte)((data.Length >> 8) & 0xFF);
         lenBuf[3] = (byte)(data.Length & 0xFF);

         buffer.AddRange(lenBuf);
         buffer.AddRange(data);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Reads a length-prefixed frame from the buffer.
      /// </summary>
      // ------------------------------------------------------------
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

   #endregion

   #region Helpers

      // ------------------------------------------------------------
      /// <summary>
      /// Resets the buffer and read position.
      /// </summary>
      // ------------------------------------------------------------
      public void Reset()
      {
         buffer.Clear();
         readPos = 0;
      }

   #endregion

   #region IDisposable

      public void Dispose() {}

   #endregion

   }
}
