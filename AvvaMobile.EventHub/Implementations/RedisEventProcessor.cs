// using Microsoft.Azure.EventHubs;
// using StackExchange.Redis;
// using System.Text.Json;
// using Azure.Messaging.EventHubs;
// using Azure.Messaging.EventHubs.Consumer;
// using Azure.Messaging.EventHubs.Primitives;
//
// namespace Mlink.AdDistribution.Shared.EventHubs.Implementations;
//
// public class CheckpointInfo
// {
//     public string Offset { get; set; }
//     public long SequenceNumber { get; set; }
//     public DateTime LastUpdated { get; set; }
// }
//
// public class RedisCheckpointManager
// {
//     private readonly ConnectionMultiplexer _redis;
//     private readonly string _checkpointPrefix;
//
//     public RedisCheckpointManager(string redisConnectionString, string eventHubName)
//     {
//         _redis = ConnectionMultiplexer.Connect(redisConnectionString);
//         _checkpointPrefix = $"eventhub:{eventHubName}:checkpoint:";
//     }
//
//     private string GetCheckpointKey(string partitionId, string consumerGroup)
//     {
//         return $"{_checkpointPrefix}{consumerGroup}:{partitionId}";
//     }
//
//     public async Task SaveCheckpointAsync(string partitionId, string consumerGroup, string offset, long sequenceNumber)
//     {
//         var db = _redis.GetDatabase();
//         var checkpointInfo = new CheckpointInfo
//         {
//             Offset = offset,
//             SequenceNumber = sequenceNumber,
//             LastUpdated = DateTime.UtcNow
//         };
//
//         var serializedCheckpoint = JsonSerializer.Serialize(checkpointInfo);
//         await db.StringSetAsync(
//             GetCheckpointKey(partitionId, consumerGroup),
//             serializedCheckpoint
//         );
//     }
//
//     public async Task<CheckpointInfo> GetCheckpointAsync(string partitionId, string consumerGroup)
//     {
//         var db = _redis.GetDatabase();
//         var value = await db.StringGetAsync(GetCheckpointKey(partitionId, consumerGroup));
//
//         if (value.HasValue)
//         {
//             return JsonSerializer.Deserialize<CheckpointInfo>(value);
//         }
//
//         return null;
//     }
// }
//
// public class RedisEventProcessor
// {
//     private readonly EventHubClient _eventHubClient;
//     private readonly RedisCheckpointManager _checkpointManager;
//     private readonly Dictionary<string, PartitionReceiver> _receivers;
//     private readonly string _consumerGroup;
//     private bool _isRunning;
//     private readonly int _maxBatchSize;
//     private readonly TimeSpan _receiveTimeout;
//
//     public RedisEventProcessor(
//         string eventHubConnectionString,
//         string eventHubName,
//         string redisConnectionString,
//         string consumerGroup = null,
//         int maxBatchSize = 100,
//         int receiveTimeoutSeconds = 30)
//     {
//         _eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString);
//         _checkpointManager = new RedisCheckpointManager(redisConnectionString, eventHubName);
//         _receivers = new Dictionary<string, PartitionReceiver>();
//         _consumerGroup = consumerGroup ?? PartitionReceiver.DefaultConsumerGroupName;
//         _maxBatchSize = maxBatchSize;
//         _receiveTimeout = TimeSpan.FromSeconds(receiveTimeoutSeconds);
//     }
//
//     public async Task StartProcessingAsync(Func<EventData, Task> processEventHandler)
//     {
//         _isRunning = true;
//         var runtimeInfo = await _eventHubClient.GetRuntimeInformationAsync();
//         var tasks = new List<Task>();
//
//         foreach (string partitionId in runtimeInfo.PartitionIds)
//         {
//             tasks.Add(ProcessPartitionAsync(partitionId, processEventHandler));
//         }
//
//         await Task.WhenAll(tasks);
//     }
//
//     private async Task ProcessPartitionAsync(string partitionId, Func<EventData, Task> processEventHandler)
//     {
//         try
//         {
//             var checkpoint = await _checkpointManager.GetCheckpointAsync(partitionId, _consumerGroup);
//             
//             var receiver = _eventHubClient.CreateReceiver(
//                 _consumerGroup,
//                 partitionId,
//                 checkpoint != null 
//                     ? EventPosition.FromOffset(checkpoint.Offset) 
//                     : EventPosition.FromStart());
//
//             _receivers[partitionId] = receiver;
//
//             while (_isRunning)
//             {
//                 try
//                 {
//                     var events = await receiver.ReceiveAsync(_maxBatchSize, _receiveTimeout);
//                     
//                     if (events != null)
//                     {
//                         foreach (var eventData in events)
//                         {
//                             await processEventHandler(eventData);
//
//                             // Her event sonrası checkpoint kaydetme
//                             await _checkpointManager.SaveCheckpointAsync(
//                                 partitionId,
//                                 _consumerGroup,
//                                 eventData.SystemProperties.Offset,
//                                 eventData.SystemProperties.SequenceNumber);
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"Error processing partition {partitionId}: {ex.Message}");
//                     await Task.Delay(1000); // Hata durumunda kısa bir bekleme
//                 }
//             }
//         }
//         finally
//         {
//             if (_receivers.TryGetValue(partitionId, out var receiver))
//             {
//                 await receiver.CloseAsync();
//                 _receivers.Remove(partitionId);
//             }
//         }
//     }
//
//     public async Task StopProcessingAsync()
//     {
//         _isRunning = false;
//         foreach (var receiver in _receivers.Values)
//         {
//             await receiver.CloseAsync();
//         }
//         await _eventHubClient.CloseAsync();
//     }
// }
//
// // Kullanım örneği
// public class Program
// {
//     static async Task Main(string[] args)
//     {
//         var processor = new RedisEventProcessor(
//             eventHubConnectionString: "YOUR_EVENTHUB_CONNECTION_STRING",
//             eventHubName: "YOUR_EVENTHUB_NAME",
//             redisConnectionString: "YOUR_REDIS_CONNECTION_STRING",
//             maxBatchSize: 100,
//             receiveTimeoutSeconds: 30
//         );
//
//         try
//         {
//             await processor.StartProcessingAsync(async (eventData) =>
//             {
//                 // Event işleme
//                 var message = System.Text.Encoding.UTF8.GetString(eventData.Body.Array);
//                 Console.WriteLine($"Received message: {message}");
//                 await Task.CompletedTask;
//             });
//
//             Console.WriteLine("Processing started. Press any key to stop...");
//             Console.ReadKey();
//         }
//         finally
//         {
//             await processor.StopProcessingAsync();
//         }
//     }
// }