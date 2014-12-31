using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.EventHubWithHost
{
    public class SimpleEventProcessor : IEventProcessor
    {
        private readonly IDictionary<string, int> map;
        private Stopwatch _checkpointStopWatch;
        private PartitionContext _partitionContext;

        public SimpleEventProcessor()
        {
            map = new Dictionary<string, int>();
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine(
                string.Format("SimpleEventProcessor initialize.  Partition: '{0}', Offset: '{1}'",
                    context.Lease.PartitionId, context.Lease.Offset));
            _partitionContext = context;
            _checkpointStopWatch = new Stopwatch();
            _checkpointStopWatch.Start();
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            try
            {
                foreach (EventData eventData in events)
                {
                    int data;
                    Message msg = DeserializeEventData(eventData);
                    string key = eventData.PartitionKey;

                    //// Name of device generating the event acts as hash key to retrieve average computed for it so far
                    //if (!map.TryGetValue(key, out data))
                    //    // If this is the first time we got data for this device then generate new state
                    //    map.Add(key, -1);

                    //// Update data
                    ////data = newData.Type;
                    //map[key] = data;

                    Console.WriteLine(
                        string.Format(
                            "Message received.  Partition: '{0}', Message.PartitionId: {3}, Message.Type: '{1}', msg.Body: '{2}'",
                            _partitionContext.Lease.PartitionId, msg.Type, Encoding.UTF8.GetString(msg.Body), key));
                }

                //Call checkpoint every 5 minutes, so that worker can resume processing from the 5 minutes back if it restarts.
                if (_checkpointStopWatch.Elapsed > TimeSpan.FromSeconds(5))
                {
                    await context.CheckpointAsync();
                    lock (this)
                    {
                        _checkpointStopWatch.Reset();
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error in processing: " + exp.Message);
            }
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine(
                string.Format("SimpleEventProcessor closing.  Partition '{0}', Reason: '{1}'.",
                    _partitionContext.Lease.PartitionId, reason.ToString()));
            //if (reason == CloseReason.Shutdown)
            await context.CheckpointAsync();
        }

        private Message DeserializeEventData(EventData eventData)
        {
            return
                JsonConvert.DeserializeObject<Message>(
                    Encoding.UTF8.GetString(eventData.GetBytes()));
        }
    }
}