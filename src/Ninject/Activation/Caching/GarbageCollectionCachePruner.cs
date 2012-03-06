#region License
// 
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 
#endregion



namespace Ninject.Activation.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Ninject.Components;
    using Ninject.Infrastructure;
    using Ninject.Infrastructure.Language;

#if WINRT
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Uses a <see cref="Timer"/> and some <see cref="WeakReference"/> magic to poll
    /// the garbage collector to see if it has run.
    /// </summary>
    public class GarbageCollectionCachePruner : NinjectComponent, ICachePruner
    {
        /// <summary>
        /// indicator for if GC has been run.
        /// </summary>
        private readonly WeakReference indicator = new WeakReference(new object());
        
        /// <summary>
        /// The caches that are being pruned.
        /// </summary>
        private readonly List<IPruneable> caches = new List<IPruneable>();

        /// <summary>
        /// The timer used to trigger the cache pruning
        /// </summary>
#if !WINRT
        private Timer timer;
#else
        private TaskTimer timer;
#endif
        /// <summary>
        /// Releases resources held by the object.
        /// </summary>
        public override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed && this.timer != null)
            {
                this.Stop();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Starts pruning the specified pruneable based on the rules of the pruner.
        /// </summary>
        /// <param name="pruneable">The pruneable that will be pruned.</param>
        public void Start(IPruneable pruneable)
        {
            Ensure.ArgumentNotNull(pruneable, "pruneable");

            this.caches.Add(pruneable);
            if (this.timer == null)
            {
#if !WINRT
                this.timer = new Timer(this.PruneCacheIfGarbageCollectorHasRun, null, this.GetTimeoutInMilliseconds(), Timeout.Infinite);
#else
                this.timer = new TaskTimer(PruneCacheIfGarbageCollectorHasRun, null, this.GetTimeoutInMilliseconds());
#endif
            }
        }

        /// <summary>
        /// Stops pruning.
        /// </summary>
        public void Stop()
        {
            using (var signal = new ManualResetEvent(false))
            {
#if !NETCF
                this.timer.Dispose(signal);
                signal.WaitOne();
#else
                this.timer.Dispose();
#endif

                this.timer = null;
                this.caches.Clear();
            }
        }

        private void PruneCacheIfGarbageCollectorHasRun(object state)
        {
            try
            {
                if (this.indicator.IsAlive)
                {
                    return;
                }

                this.caches.Map(cache => cache.Prune());
                this.indicator.Target = new object();
            }
            finally
            {
#if !WINRT
                this.timer.Change(this.GetTimeoutInMilliseconds(), Timeout.Infinite);
#else
                this.timer.Change(this.GetTimeoutInMilliseconds());
#endif
            }
        }

        private int GetTimeoutInMilliseconds()
        {
            TimeSpan interval = Settings.CachePruningInterval;
            return interval == TimeSpan.MaxValue ? -1 : (int)interval.TotalMilliseconds;
        }
    }

#if WINRT
    internal class TaskTimer
    {
        private readonly Action<object> _callback;
        private readonly object _state;
        private int _interval;

        private Task _lastTask;

        public TaskTimer(Action<object> callback, object state, int interval)
        {
            _callback = callback;
            _state = state;
            _interval = interval;


            Tick();
        }

        private void Tick()
        {
            _lastTask = Task.Run(async () => await StartLoop());
        }

        private async Task StartLoop()
        {
            await Task.Delay(_interval);
            _callback(_state);
        }

        public void Change(int interval)
        {
            _interval = interval;

            Tick();
        }

        public void Dispose(ManualResetEvent @event)
        {
            if(_lastTask != null)
            {
                _lastTask.ContinueWith(_ => @event.Set());
            }
        }
    }
#endif
}