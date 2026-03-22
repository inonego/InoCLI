using System;
using System.IO;
using System.Threading;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// High-level client that sends requests and receives responses
   /// over an ITransport using the frame protocol.
   /// </summary>
   // ============================================================
   public class CliClient
   {

   #region Fields

      private readonly ITransport transport;

   #endregion

   #region Constructor

      public CliClient(ITransport transport)
      {
         this.transport = transport;
      }

   #endregion

   #region Send

      // ------------------------------------------------------------
      /// <summary>
      /// Sends a request and returns the parsed response.
      /// </summary>
      // ------------------------------------------------------------
      public CliResponse Send(CliRequest request)
      {
         if (!transport.IsConnected)
         {
            transport.Connect();
         }

         FrameProtocol.Send(transport, request.ToJson());

         string json = FrameProtocol.Receive(transport);

         return CliResponse.Parse(json);
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Sends a request with retry on connection failure.
      /// <br/> If the frame was sent but the response is lost (e.g. domain
      /// <br/> reload), returns a synthetic success response.
      /// </summary>
      // ----------------------------------------------------------------------
      public CliResponse SendWithRetry
      (
         CliRequest request,
         int timeoutSeconds = -1,
         Action onFrameSent = null
      )
      {
         var  start   = DateTime.Now;
         bool hasSent = false;

         while (true)
         {
            try
            {
               if (!transport.IsConnected)
               {
                  transport.Connect();
               }

               FrameProtocol.Send(transport, request.ToJson());
               hasSent = true;

               onFrameSent?.Invoke();

               string json = FrameProtocol.Receive(transport);

               return CliResponse.Parse(json);
            }
            catch (Exception)
            {
               // Request sent but response lost (domain reload) — treat as success
               if (hasSent)
               {
                  return CliResponse.SyntheticSuccess();
               }

               if (timeoutSeconds >= 0 && (DateTime.Now - start).TotalSeconds >= timeoutSeconds)
               {
                  throw new IOException($"Failed to connect after {timeoutSeconds}s.");
               }

               Thread.Sleep(500);

               // Reset for next attempt
               hasSent = false;
            }
         }
      }

   #endregion

   }
}
