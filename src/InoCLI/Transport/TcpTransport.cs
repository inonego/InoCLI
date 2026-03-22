using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// TCP transport using length-prefixed frames.
   /// Frame format: [4-byte BE uint32 length][body]
   /// </summary>
   // ============================================================
   public class TcpTransport : ITransport
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Whether the underlying TCP connection is alive.
      /// </summary>
      // ------------------------------------------------------------
      public bool IsConnected => client != null && client.Connected;

      private readonly string host;
      private readonly int    port;

      private TcpClient     client = null;
      private NetworkStream stream = null;

   #endregion

   #region Constructor

      public TcpTransport(int port, string host = "127.0.0.1")
      {
         this.host = host;
         this.port = port;
      }

   #endregion

   #region ITransport

      // ------------------------------------------------------------
      /// <summary>
      /// Opens a TCP connection to the target host and port.
      /// </summary>
      // ------------------------------------------------------------
      public void Connect()
      {
         client = new TcpClient();
         client.Connect(host, port);

         stream = client.GetStream();
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Writes a length-prefixed frame to the TCP stream.
      /// </summary>
      // ------------------------------------------------------------
      public void WriteFrame(byte[] data)
      {
         byte[] length = BitConverter.GetBytes((uint)data.Length);

         if (BitConverter.IsLittleEndian)
         {
            Array.Reverse(length);
         }

         stream.Write(length, 0, 4);
         stream.Write(data,   0, data.Length);
         stream.Flush();
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Reads a length-prefixed frame from the TCP stream.
      /// </summary>
      // ------------------------------------------------------------
      public byte[] ReadFrame()
      {
         byte[] lengthBuf = new byte[4];
         ReadExact(lengthBuf, 4);

         if (BitConverter.IsLittleEndian)
         {
            Array.Reverse(lengthBuf);
         }

         uint length = BitConverter.ToUInt32(lengthBuf, 0);

         byte[] body = new byte[length];
         ReadExact(body, (int)length);

         return body;
      }

   #endregion

   #region IDisposable

      public void Dispose()
      {
         stream?.Dispose();
         client?.Dispose();
      }

   #endregion

   #region Helpers

      private void ReadExact(byte[] buffer, int count)
      {
         int offset = 0;

         while (offset < count)
         {
            int read = stream.Read(buffer, offset, count - offset);

            if (read == 0)
            {
               throw new IOException("Connection closed.");
            }

            offset += read;
         }
      }

   #endregion

   }
}
