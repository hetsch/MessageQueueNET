using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MessageQueueNET.Core.Concurrency.MessageQueueFuzzyMutexAsync;

namespace MessageQueueNET.Core.Concurrency;

public class MessageQueueFuzzyMutexAsync
{
    private static readonly ConcurrentDictionary<string, DateTime> _mutexDirectory = new ConcurrentDictionary<string, DateTime>();

    async static public Task<IMutex> LockAsync(string key, int timeoutMilliSeconds = 20000)
    {
        var random = new Random(Environment.TickCount);
        var start = DateTime.Now;
        bool wasBlocked = false;

        while (true)
        {
            if (_mutexDirectory.TryAdd(key, DateTime.Now))
            {
                break;
            }
            else
            {
                wasBlocked = true;
                if ((DateTime.Now - start).TotalMilliseconds > timeoutMilliSeconds)
                {
                    throw new MessageQueueFuzzyMutexAsyncExcepiton($"FuzzyMutex - timeout milliseconds reached: {(DateTime.Now - start).TotalMilliseconds} > {timeoutMilliSeconds}");
                }

                await Task.Delay(random.Next(50));
            }
        }

        return new Mutex(key, wasBlocked);
    }

    #region Classes

    public interface IMutex : IDisposable
    {
        bool WasBlocked { get; }
    }

    private class Mutex : IMutex
    {
        private readonly string _key;
        private readonly bool _wasBlocked;
        public Mutex(string key, bool hadLocks)
        {
            _key = key;
            _wasBlocked = hadLocks;
        }

        public bool WasBlocked => _wasBlocked;

        #region IDisposable

        public void Dispose()
        {
            if (!_mutexDirectory.TryRemove(_key, out DateTime removed))
            {

            }
        }

        #endregion
    }

    #endregion
}

public class MessageQueueMultiFuzzyMutexAsync
{
    private static readonly ConcurrentDictionary<string, DateTime> _mutexDirectory = new ConcurrentDictionary<string, DateTime>();

    async static public Task<IMutex> LockAsync(string[] keys, int timeoutMilliSeconds = 20000)
    {
        using (var globalMutex = await MessageQueueFuzzyMutexAsync.LockAsync(Guid.NewGuid().ToString()))
        {
            List<IMutex> mutexList = new List<IMutex>();

            foreach (var key in keys.Distinct().ToArray())
            {
                var random = new Random(Environment.TickCount);
                var start = DateTime.Now;
                bool wasBlocked = false;

                while (true)
                {
                    if (_mutexDirectory.TryAdd(key, DateTime.Now))
                    {
                        break;
                    }
                    else
                    {
                        wasBlocked = true;
                        if ((DateTime.Now - start).TotalMilliseconds > timeoutMilliSeconds)
                        {
                            throw new MessageQueueFuzzyMutexAsyncExcepiton($"FuzzyMutex - timeout milliseconds reached: {(DateTime.Now - start).TotalMilliseconds} > {timeoutMilliSeconds}");
                        }

                        await Task.Delay(random.Next(50));
                    }
                }

                mutexList.Add(new KeyMutex(key, wasBlocked));
            }

            return new MutexList(mutexList);
        }
    }

    #region Classes

    private class KeyMutex : IMutex
    {
        private readonly string _key;
        private readonly bool _wasBlocked;
        public KeyMutex(string key, bool hadLocks)
        {
            _key = key;
            _wasBlocked = hadLocks;
        }

        public bool WasBlocked => _wasBlocked;

        #region IDisposable

        public void Dispose()
        {
            if (!_mutexDirectory.TryRemove(_key, out DateTime removed))
            {

            }
        }

        #endregion
    }

    private class MutexList : IMutex
    {
        private readonly List<IMutex> _mutexList;

        public MutexList(List<IMutex> mutexList)
        {
            _mutexList = mutexList;
        }

        public bool WasBlocked => _mutexList.Any(m=>m.WasBlocked);

        public void Dispose()
        {
            foreach(var mutex in _mutexList) 
            { 
                mutex.Dispose();
            }

            _mutexList.Clear();
        }
    }

    #endregion
}