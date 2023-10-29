using MessageQueueNET.Client;
using MessageQueueNET.Client.Models;
using MessageQueueNET.Extensions;
using MessageQueueNET.Models;
using MessageQueueNET.Services;
using MessageQueueNET.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly QueuesService _queues;
        private readonly IQueuesPersistService _persist;

        public QueueController(QueuesService queues,
                               IQueuesPersistService persist)
        {
            _queues = queues;
            _persist = persist;
        }

        [HttpGet]
        [Route("dequeue/{idPattern}")]
        async public Task<MessagesResult> Dequeue(string idPattern, int count = 1, bool register = false)
        {
            try
            {
                if (register == true
                    && idPattern.IsPattern() == false
                    && _queues.QueueExists(idPattern) == false)
                {
                    _queues.GetQueue(idPattern);  // register Queue
                }

                var queryQueues = _queues.GetQueues(idPattern);
                var result = new MessagesResult(queryQueues.CalcHashCode());
                var modifiedQueues=new List<Queue>();

                if (queryQueues.Any() == false)
                {
                    return result;
                }

                List<MessageResult> items = new List<MessageResult>();

                while (items.Count() < count)
                {
                    var queue = queryQueues.QueueWithOldestDequeueAbleItem(_queues);
                    if (queue is null)
                    {
                        break;
                    }

                    if (queue.TryDequeue(out QueueItem? item))
                    {
                        if (queue.Properties.ConfirmationPeriodSeconds > 0)
                        {
                            var unconfirmedItem = item.Clone();
                            if (!await _persist.PersistUnconfirmedQueueItem(queue.Name, unconfirmedItem))
                            {
                                throw new Exception("Can't unable to persist unconfirmed queue item");
                            }

                            _queues.AddToUnconfirmedMessage(queue, unconfirmedItem);
                        }

                        if (await _persist.RemoveQueueItem(queue.Name, item.Id) && item.IsValid(queue))
                        {
                            items.Add(new MessageResult()
                            {
                                Queue = queue.Name,
                                Id = item.Id,
                                Value = item.Message,
                                RequireConfirmation = queue.Properties.ConfirmationPeriodSeconds > 0 ? true : null,
                                ConfirmationPeriod = queue.Properties.ConfirmationPeriodSeconds > 0 ? queue.Properties.ConfirmationPeriodSeconds : null,
                            });
                        }

                        modifiedQueues.Add(queue);
                    }
                }

                result.Messages = items;
                queryQueues.SetModified();

                return result;
            }
            catch(Exception ex)
            {
                return new MessagesResult().AddExceptionMessage(ex);
            }
        }

        [HttpGet]
        [Route("confirmdequeue/{id}")]
        async public Task<ApiResult> ConfirmDequeue(string id, Guid messageId)
        {
            try
            {
                var queue = _queues.GetQueue(id);

                var result = new ApiResult(queue.CalcHashCode());

                if (queue.Properties.ConfirmationPeriodSeconds > 0)
                {
                    if (await _persist.RemoveUnconfirmedQueueItem(queue.Name, messageId))
                    {
                        result.Success = _queues.ConfirmDequeuedMessage(queue, messageId);
                        queue.SetModified();
                    }
                }

                return result;
            }
            catch(Exception ex)
            {
                return new ApiResult().AddExceptionMessage(ex);
            }
        }

        [HttpPut]
        [Route("enqueue/{id}")]
        async public Task<ApiResult> Enqueue(string id, IEnumerable<string> messages)
        {
            try
            {
                var queue = _queues.GetQueue(id);

                if (queue.Properties.SuspendEnqueue == true)
                {
                    throw new Exception($"Enqueue suspended");
                }

                foreach (var message in messages)
                {
                    var item = new QueueItem() { Message = message };
                    if (!await _persist.PersistQueueItem(id, item))
                    {
                        throw new Exception($"Error when persisting item");
                    }

                    queue.Enqueue(item);
                }

                queue.SetModified();

                return new ApiResult(queue.CalcHashCode());
            }
            catch(Exception ex)
            {
                return new ApiResult().AddExceptionMessage(ex);
            }
        }

        [HttpGet]
        [Route("all/{idPattern}")]
        public MessagesResult AllMessages(string idPattern, int max = 0, bool unconfirmedOnly = false)
        {
            try
            {
                if (_queues.AnyQueueExists(idPattern))
                {
                    var messages = new List<MessageResult>();
                    var unconfirmedMessages = new List<MessageResult>();

                    var queues = _queues.GetQueues(idPattern);
                    var result = new MessagesResult(queues.CalcHashCode());

                    foreach (var queue in queues)
                    {
                        if (unconfirmedOnly == false)
                        {
                            messages.AddRange(queue
                                .Where(message => message.IsValid(queue))
                                .Select(message => new MessageResult()
                                {
                                    Queue = queue.Name,
                                    Id = message.Id,
                                    Value = message.Message,
                                    CreationDateUTC = message.CreationDateUTC,
                                    RequireConfirmation = queue.Properties.ConfirmationPeriodSeconds > 0 ? true : null,
                                    ConfirmationPeriod = queue.Properties.ConfirmationPeriodSeconds > 0 ? queue.Properties.ConfirmationPeriodSeconds : null,
                                }));

                            if (max > 0)
                            {
                                messages = messages.Take(max).ToList();
                            }
                        }

                        unconfirmedMessages.AddRange(_queues.UnconfirmedItems(queue) ?? Array.Empty<MessageResult>());

                        if (max > 0)
                        {
                            unconfirmedMessages = unconfirmedMessages.Take(max).ToList();
                        }
                    }

                    result.Messages = messages;
                    result.UnconfirmedMessages = unconfirmedMessages;

                    return result;
                }
            }
            catch(Exception ex)
            {
                new MessagesResult().AddExceptionMessage(ex);
            }

            return new MessagesResult(0);
        }

        [HttpGet]
        [Route("length/{idPattern}")]
        public QueueLengthResult Length(string idPattern)
        {
            try
            {
                if (_queues.AnyQueueExists(idPattern))
                {
                    var queues = _queues.GetQueues(idPattern);
                    var result = new QueueLengthResult(queues.CalcHashCode());

                    var queueLengthItems = new Dictionary<string, QueueLengthItem>();

                    foreach (var queue in queues)
                    {
                        queueLengthItems[queue.Name] = new QueueLengthItem()
                        {
                            QueueLength = queue.Where(item => item.IsValid(queue))
                                               .Count(),
                            UnconfirmedItems = _queues.UnconfirmedMessagesCount(queue)
                        };
                    }

                    result.Queues = queueLengthItems;
                }
            }
            catch(Exception ex)
            {
                new QueueLengthResult().AddExceptionMessage(ex);
            }

            return new QueueLengthResult(0);
        }

        [HttpGet]
        [Route("remove/{idPattern}")]
        async public Task<ApiResult> Remove(string idPattern, RemoveType removeType = RemoveType.Queue)
        {
            
            try
            {
                if (_queues.AnyQueueExists(idPattern))
                {
                    var queues = _queues.GetQueues(idPattern);
                    var result = new ApiResult(queues.CalcHashCode());

                    foreach (var queue in queues)
                    {
                        if (removeType == RemoveType.Messages)
                        {
                            if (await _persist.RemoveQueueMessages(queue.Name))
                            {
                                result.Success &= _queues.RemoveQueueMessages(queue);
                                queue.SetModified();
                            }
                            else { result.Success = false; }
                        }
                        else if (removeType == RemoveType.UnconfirmedMessages)
                        {
                            if (await _persist.RemoveQueueUnconfirmedMessages(queue.Name))
                            {
                                result.Success &= _queues.RemoveUnconfirmedQueueItems(queue);
                                queue.SetModified();
                            }
                            else { result.Success = false; }
                        }
                        else
                        {
                            if (await _persist.RemoveQueue(queue.Name))
                            {
                                result.Success &= _queues.RemoveQueue(queue.Name);
                            }
                            else { result.Success = false; }
                        }
                    }

                    return result;
                }  
            }
            catch(Exception ex)
            {
                new ApiResult().AddExceptionMessage(ex);
            }

            return new ApiResult(0);
        }

        [HttpGet]
        [Route("register/{idPattern}")]
        public QueuePropertiesResult Register(
                             string idPattern,
                             int? lifetimeSeconds = null,
                             int? itemLifetimeSeconds = null,
                             int? confirmationPeriodSeconds = null,
                             int? maxUnconfirmedItems = null,
                             bool? suspendEnqueue = null,
                             bool? suspendDequeue = null)
        {
            try
            {
                var queues = _queues.GetQueues(idPattern);

                foreach (var queue in queues)
                {
                    queue.Properties.LifetimeSeconds = lifetimeSeconds.GetValueOrDefault(queue.Properties.LifetimeSeconds);
                    queue.Properties.ItemLifetimeSeconds = itemLifetimeSeconds.GetValueOrDefault(queue.Properties.ItemLifetimeSeconds);
                    queue.Properties.ConfirmationPeriodSeconds = confirmationPeriodSeconds.GetValueOrDefault(queue.Properties.ConfirmationPeriodSeconds);
                    queue.Properties.MaxUnconfiredItems = maxUnconfirmedItems.GetValueOrDefault(queue.Properties.MaxUnconfiredItems);
                    queue.Properties.SuspendEnqueue = suspendEnqueue.GetValueOrDefault(queue.Properties.SuspendEnqueue);
                    queue.Properties.SuspendDequeue = suspendDequeue.GetValueOrDefault(queue.Properties.SuspendDequeue);

                    _persist.PersistQueueProperties(queue);
                }

                queues.SetModified();

                return QueueProperties(idPattern);
            }
            catch (Exception ex)
            {
                return new QueuePropertiesResult().AddExceptionMessage(ex);
            }
        }

        [HttpGet]
        [Route("properties/{idPattern}")]
        public QueuePropertiesResult QueueProperties(string idPattern)
        {
            try
            {
                if (_queues.AnyQueueExists(idPattern))
                {
                    var queues = _queues.GetQueues(idPattern)
                                        .OrderBy(q => q.Name);

                    var queuePropertiesResult = new QueuePropertiesResult(queues.CalcHashCode())
                    {
                        Queues = new Dictionary<string, Client.Models.QueueProperties>()
                    };

                    foreach (var queue in queues)
                    {
                        queuePropertiesResult.Queues[queue.Name] = new Client.Models.QueueProperties()
                        {
                            LastAccessUTC = queue.LastAccessUTC,
                            LastModifiedUTC = queue.LastModifiedUTC,

                            Length = queue.Where(item => item.IsValid(queue))
                                          .Count(),
                            UnconfirmedItems = queue.Properties.ConfirmationPeriodSeconds > 0 ? _queues.UnconfirmedMessagesCount(queue) : null,

                            LifetimeSeconds = queue.Properties.LifetimeSeconds,
                            ItemLifetimeSeconds = queue.Properties.ItemLifetimeSeconds,

                            ConfirmationPeriodSeconds = queue.Properties.ConfirmationPeriodSeconds,
                            MaxUnconfirmedItems = queue.Properties.MaxUnconfiredItems > 0 ? queue.Properties.MaxUnconfiredItems : null,

                            SuspendDequeue = queue.Properties.SuspendDequeue,
                            SuspendEnqueue = queue.Properties.SuspendEnqueue,
                        };
                    }

                    return queuePropertiesResult;
                }
            }
            catch (Exception ex)
            {
                return new QueuePropertiesResult().AddExceptionMessage(ex);
            }


            return new QueuePropertiesResult(0);
        }

        [HttpGet]
        [Route("queuenames")]
        public QueueNamesResult QueueNames()
        {
            try
            {
                return new QueueNamesResult() { QueueNames = _queues.QueueNames };
            }
            catch (Exception ex) 
            {
                return new QueueNamesResult().AddExceptionMessage(ex);
            }
        }
    }
}
