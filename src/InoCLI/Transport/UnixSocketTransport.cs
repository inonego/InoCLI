using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Unix Domain Socket transport using length-prefixed frames.
   /// Frame format: [4-byte BE uint32 length][body]
   /// </summary>
   // ============================================================
   public class UnixSocketTransport : ITransport
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Whether the underlying socket is connected.
      /// </summary>
      // ------------------------------------------------------------
      public bool IsConnected => socket != null && socket.Connected;

      private readonly string socketPath;

      private Socket socket = null;

   #endregion

   #region Constructors

      // ------------------------------------------------------------
      /// <summary>
      /// Creates a transport that will connect to a socket path.
      /// </summary>
      // ------------------------------------------------------------
      public UnixSocketTransport(string socketPath)
      {
         this.socketPath = socketPath;
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Wraps an already-connected socket (e.g. from server Accept).
      /// <br/> Connect() becomes a no-op.
      /// </summary>
      // ----------------------------------------------------------------------
      public UnixSocketTransport(Socket acceptedSocket)
      {
         this.socketPath = null;
         this.socket     = acceptedSocket;
      }

   #endregion

   #region ITransport

      // ------------------------------------------------------------
      /// <summary>
      /// Connects to the Unix Domain Socket at the configured path.
      /// </summary>
      // ------------------------------------------------------------
      public void Connect()
      {
         var endpoint = new UnixDomainSocketEndPoint(socketPath);

         socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
         socket.Connect(endpoint);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Writes a length-prefixed frame to the socket.
      /// </summary>
      // ------------------------------------------------------------
      public void WriteFrame(byte[] data)
      {
         byte[] lengthBuf = new byte[4];

         lengthBuf[0] = (byte)((data.Length >> 24) & 0xFF);
         lengthBuf[1] = (byte)((data.Length >> 16) & 0xFF);
         lengthBuf[2] = (byte)((data.Length >> 8) & 0xFF);
         lengthBuf[3] = (byte)(data.Length & 0xFF);

         socket.Send(lengthBuf);
         socket.Send(data);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Reads a length-prefixed frame from the socket.
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

      public void Dispose()
      {
         if (socket != null)
         {
            socket.Close();
            socket.Dispose();
         }
      }

   #endregion

   #region Helpers

      private void ReadExact(byte[] buffer, int count)
      {
         int offset = 0;

         while (offset < count)
         {
            int read = socket.Receive(buffer, offset, count - offset, SocketFlags.None);

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
