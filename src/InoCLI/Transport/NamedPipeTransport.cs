using System;
using System.IO;
using System.IO.Pipes;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Named Pipe transport using length-prefixed frames.
   /// Frame format: [4-byte BE uint32 length][body]
   /// </summary>
   // ============================================================
   public class NamedPipeTransport : ITransport
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Whether the underlying pipe stream is connected.
      /// </summary>
      // ------------------------------------------------------------
      public bool IsConnected => stream != null && stream.IsConnected;

      private readonly string pipeName;

      private PipeStream stream = null;

   #endregion

   #region Constructors

      // ------------------------------------------------------------
      /// <summary>
      /// Creates a transport that will connect to a named pipe.
      /// </summary>
      // ------------------------------------------------------------
      public NamedPipeTransport(string pipeName)
      {
         this.pipeName = pipeName;
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Wraps an already-connected pipe stream (e.g. from server Accept).
      /// <br/> Connect() becomes a no-op.
      /// </summary>
      // ----------------------------------------------------------------------
      public NamedPipeTransport(PipeStream acceptedStream)
      {
         this.pipeName = null;
         this.stream   = acceptedStream;
      }

   #endregion

   #region ITransport

      // ------------------------------------------------------------
      /// <summary>
      /// Connects to the named pipe server.
      /// </summary>
      // ------------------------------------------------------------
      public void Connect()
      {
         if (stream != null)
         {
            return;
         }

         var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
         client.Connect(5000);

         stream = client;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Writes a length-prefixed frame to the pipe.
      /// </summary>
      // ------------------------------------------------------------
      public void WriteFrame(byte[] data)
      {
         byte[] lengthBuf = new byte[4];

         lengthBuf[0] = (byte)((data.Length >> 24) & 0xFF);
         lengthBuf[1] = (byte)((data.Length >> 16) & 0xFF);
         lengthBuf[2] = (byte)((data.Length >> 8) & 0xFF);
         lengthBuf[3] = (byte)(data.Length & 0xFF);

         stream.Write(lengthBuf, 0, 4);
         stream.Write(data, 0, data.Length);
         stream.Flush();
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Reads a length-prefixed frame from the pipe.
      /// </summary>
      // ------------------------------------------------------------
      public byte[] ReadFrame()
      {
         byte[] lengthBuf = new byte[4];
         ReadExact(lengthBuf, 4);

         int length = (lengthBuf[0] << 24) | (lengthBuf[1] << 16) | (lengthBuf[2] << 8) | lengthBuf[3];

         byte[] body = new byte[length];
         ReadExact(body, length);

         return body;
      }

   #endregion

   #region IDisposable

      // ------------------------------------------------------------
      /// <summary>
      /// Disposes the pipe stream.
      /// </summary>
      // ------------------------------------------------------------
      public void Dispose()
      {
         if (stream != null)
         {
            stream.Close();
            stream.Dispose();
         }
      }

   #endregion

   #region Helpers

      // ------------------------------------------------------------
      /// <summary>
      /// Reads exactly count bytes from the pipe stream.
      /// </summary>
      // ------------------------------------------------------------
      private void ReadExact(byte[] buffer, int count)
      {
         int offset = 0;

         while (offset < count)
         {
            int read = stream.Read(buffer, offset, count - offset);

            if (read == 0)
            {
               throw new IOException("Pipe closed.");
            }

            offset += read;
         }
      }

   #endregion

   }
}
