using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Core.Concurrency;

public class MessageQueueFuzzyMutexAsync
{
    private static readonly ConcurrentDictionary<string, DateTime> _globalMutexDirectory = new ConcurrentDictionary<string, DateTime>();
    private static readonly ConcurrentDictionary<string, DateTime> _keyMutexDirectory = new ConcurrentDictionary<string, DateTime>();

    async static public Task<IMutex> LockAsync(string key, int timeoutMilliSeconds = 20000)
    {
        var random = new Random(Environment.TickCount);
        var start = DateTime.Now;
        bool wasBlocked = false;

        while (true)
        {
            if (_globalMutexDirectory.TryAdd(key, DateTime.Now))
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

        return new GlobalMutex(key, wasBlocked);
    }

    async static public Task<IMutex> LockAsync(string globalKey, string[] keys, int timeoutMilliSeconds = 20000)
    {
        using (var globalMutex = await MessageQueueFuzzyMutexAsync.LockAsync(globalKey))
        {
            List<IMutex> mutexList = new List<IMutex>();

            var random = new Random(Environment.TickCount);
            var start = DateTime.Now;

            foreach (var key in keys.Distinct().ToArray())
            {
                bool wasBlocked = false;

                while (true)
                {
                    if (_keyMutexDirectory.TryAdd(key, DateTime.Now))
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

    public interface IMutex : IDisposable
    {
        bool WasBlocked { get; }
    }

    private class GlobalMutex : IMutex
    {
        private readonly string _key;
        private readonly bool _wasBlocked;
        public GlobalMutex(string key, bool hadLocks)
        {
            _key = key;
            _wasBlocked = hadLocks;
        }

        public bool WasBlocked => _wasBlocked;

        #region IDisposable

        public void Dispose()
        {
            if (!_globalMutexDirectory.TryRemove(_key, out DateTime removed))
            {

            }
        }

        #endregion
    }

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
            if (!_keyMutexDirectory.TryRemove(_key, out DateTime removed))
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

        public bool WasBlocked => _mutexList.Any(m => m.WasBlocked);

        public void Dispose()
        {
            foreach (var mutex in _mutexList)
            {
                mutex.Dispose();
            }

            _mutexList.Clear();
        }
    }

    #endregion
}