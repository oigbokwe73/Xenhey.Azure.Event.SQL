using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Xenhey.BPM.Core;
using Xenhey.BPM.Core.Implementation;

namespace Xenhey.Azure.Function.Event.SQL
{
    public static class EventHubToSQL
    {
        private static NameValueCollection nvc = new NameValueCollection();

        [FunctionName("EventHubToSQL")]
        public static async Task Run([EventHubTrigger("samples-workitems", Connection = "EventHubConnectionAppSetting")] EventData[] events, ILogger log)
        {

            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    var results = orchrestatorService.Run(messageBody);
                    log.LogInformation($"EnqueuedTimeUtc={eventData.SystemProperties.EnqueuedTimeUtc}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
        private static  IOrchrestatorService orchrestatorService
        {
            get
            {
                //The name,value pair should be changed. Sees documentation
                nvc.Add("x-api-key", "0F1AE83D158143AC84F6150F62B29712");
                return new ManagedOrchestratorService(nvc);
            }
        }
    }
}
