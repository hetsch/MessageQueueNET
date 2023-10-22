using MessageQueueNET.Models;
using MessageQueueNET.Services.Abstraction;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageQueueNET.Services
{
    public class QueuePersistFileSystem : IQueuesPersistService
    {
        private const string QueuePropertiesFilename = "_queue.properties.json";
        private readonly QueuePersistFileSystemOptions _options;

        public QueuePersistFileSystem(IOptionsMonitor<QueuePersistFileSystemOptions> optionsMonitor)
        {
            _options = optionsMonitor.CurrentValue;
        }

        #region IQueuePersist

        async public Task<bool> PersistQueueProperties(Queue queue)
        {
            try
            {
                if (queue?.Properties != null)
                {
                    FileInfo fi = new FileInfo(Path.Combine(_options.RootPath, queue.Name, QueuePropertiesFilename));

                    if (!fi.Directory!.Exists)
                    {
                        fi.Directory.Create();
                    }

                    var jsonString = JsonSerializer.Serialize(queue.Properties);
                    await File.WriteAllTextAsync(fi.FullName, jsonString);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        async public Task<QueueProperties?> GetQueueProperties(string queueName)
        {
            try
            {
                FileInfo fi = new FileInfo(Path.Combine(_options.RootPath, queueName, QueuePropertiesFilename));
                if (fi.Exists)
                {
                    var jsonString = await File.ReadAllTextAsync(fi.FullName);
                    return JsonSerializer.Deserialize<QueueProperties>(jsonString);
                }
            }
            catch { }
            return null;
        }

        async public Task<IEnumerable<QueueItem>> GetAllItems(string queueName)
        {
            var result = new List<QueueItem>();
            DirectoryInfo di = new DirectoryInfo(Path.Combine(_options.RootPath, queueName));
  
            if (di.Exists)
            {
                foreach (var fi in di.GetFiles("*.json"))
                {
                    if (fi.Name.StartsWith("_"))
                    {
                        continue;
                    }

                    var jsonString = await File.ReadAllTextAsync(fi.FullName);
                    result.Add(JsonSerializer.Deserialize<QueueItem>(jsonString)!);
                }
            }
            return result;
        }

        async public Task<IEnumerable<QueueItem>> GetAllUnconfirmedItems(string queueName)
        {
            var result = new List<QueueItem>();
            DirectoryInfo di = new DirectoryInfo(Path.Combine(_options.RootPath, queueName, "_unconfirmed"));
            
            if (di.Exists)
            {
                foreach (var fi in di.GetFiles("*.json"))
                {
                    if (fi.Name.StartsWith("_"))
                    {
                        continue;
                    }

                    var jsonString = await File.ReadAllTextAsync(fi.FullName);
                    result.Add(JsonSerializer.Deserialize<QueueItem>(jsonString)!);
                }
            }

            return result;
        }

        async public Task<bool> PersistQueueItem(string queueName, QueueItem item)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(Path.Combine(_options.RootPath, queueName));

                if (!di.Exists)
                {
                    di.Create();
                }

                var jsonString = JsonSerializer.Serialize(item);
                await File.WriteAllTextAsync(Path.Combine(di.FullName, $"{ item.Id.ToString().ToLower() }.json"), jsonString);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<IEnumerable<string>> QueueNames()
        {
            List<string> queueNames = new List<string>();

            try
            {
                DirectoryInfo di = new DirectoryInfo(_options.RootPath);
                foreach (var queueDirectory in di.GetDirectories())
                {
                    queueNames.Add(queueDirectory.Name);
                }
            }
            catch { }

            return Task.FromResult<IEnumerable<string>>(queueNames);
        }

        public Task<bool> RemoveQueue(string queueName)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(Path.Combine(_options.RootPath, queueName));
                if (di.Exists)
                {
                    di.Delete(true);
                }

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> RemoveQueueItem(string queueName, Guid itemId)
        {
            try
            {
                FileInfo fi = new FileInfo(Path.Combine(_options.RootPath, queueName, $"{ itemId.ToString().ToLower() }.json"));
                if (fi.Exists)
                {
                    fi.Delete();
                }

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        async public Task<bool> PersistUnconfirmedQueueItem(string queueName, QueueItem item)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(Path.Combine(_options.RootPath, queueName, "_unconfirmed"));

                if (!di.Exists)
                {
                    di.Create();
                }

                var jsonString = JsonSerializer.Serialize(item);
                await File.WriteAllTextAsync(Path.Combine(di.FullName, $"{ item.Id.ToString().ToLower() }.json"), jsonString);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<bool> RemoveUnconfirmedQueueItem(string queueName, Guid itemId)
        {
            try
            {
                FileInfo fi = new FileInfo(Path.Combine(_options.RootPath, queueName, "_unconfirmed", $"{ itemId.ToString().ToLower() }.json"));
                if (fi.Exists)
                {
                    fi.Delete();
                }

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        #endregion
    }
}
