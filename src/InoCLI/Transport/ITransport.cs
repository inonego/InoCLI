using System;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Transport abstraction for length-prefixed frame communication.
   /// </summary>
   // ============================================================
   public interface ITransport : IDisposable
   {

      bool IsConnected { get; }

      void Connect();

      void WriteFrame(byte[] data);

      byte[] ReadFrame();

   }
}
