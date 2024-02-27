using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Logger;

namespace Engine
{
    public class ChildLoaderList<T>
    {
        private CancellationTokenSource loaderCancellationTokenSource;

        private bool queueReadyForMainThreadProcessing = false;
        private ThreadSafeList<T> addQueue;
        private HashSet<T> addQueueCopy;

        private ThreadSafeList<T> removeQueue;
        private HashSet<T> removeQueueCopy;

        /// <summary>
        /// Run a background task to handle the queuing of objects to be loaded
        /// </summary>
        /// <param name="addAction">An action to perform on the items before being loaded</param>
        /// <param name="removeAction">An action to perform on the items before being unloaded</param>
        public void Run(Func<HashSet<T>, Task> addAction, Func<HashSet<T>, Task> removeAction)
        {
            loaderCancellationTokenSource = new CancellationTokenSource();
            Log.Instance.Write("Starting loader thread");

            addQueue = new ThreadSafeList<T>();
            removeQueue = new ThreadSafeList<T>();

            addQueueCopy = new HashSet<T>();
            removeQueueCopy = new HashSet<T>();

#pragma warning disable CS4014
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (loaderCancellationTokenSource.IsCancellationRequested)
                            break;

                        if (queueReadyForMainThreadProcessing)
                        {
                            await Task.Delay(10).ConfigureAwait(false);
                            continue;
                        }

                        addQueueCopy = await addQueue.CopyToHashSet().ConfigureAwait(false);
                        await addQueue.Clear().ConfigureAwait(false);

                        if (addAction != null)
                            await addAction(addQueueCopy).ConfigureAwait(false);

                        removeQueueCopy = await removeQueue.CopyToHashSet().ConfigureAwait(false);
                        await removeQueue.Clear().ConfigureAwait(false);

                        if (removeAction != null)
                            await removeAction(removeQueueCopy).ConfigureAwait(false);

                        queueReadyForMainThreadProcessing = true;

                        await Task.Delay(100).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteFatalException(ex);
                }

                loaderCancellationTokenSource.Dispose();
                loaderCancellationTokenSource = null;
                Log.Instance.Write("Closed loader thread");
            });
#pragma warning restore CS4014
        }

        public void Update(Action<T> addAction, Action<T> removeAction)
        {
            try
            {
                if (queueReadyForMainThreadProcessing)
                {
                    foreach (var i in addQueueCopy)
                        addAction(i);

                    foreach (var i in removeQueueCopy)
                        removeAction(i);

                    queueReadyForMainThreadProcessing = false;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteFatalException(ex);
            }
        }

        public void Stop()
        {
            if (loaderCancellationTokenSource != null && !loaderCancellationTokenSource.IsCancellationRequested && loaderCancellationTokenSource.Token.CanBeCanceled)
                loaderCancellationTokenSource.Cancel();
        }

        public void QueueObjectAdd(T value)
        {
            Task.Run(async () =>
            {
                try
                {
                    var contains = await removeQueue.Contains(value).ConfigureAwait(false);

                    if (contains)
                        return;

                    await addQueue.Add(value).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteFatalException(ex);
                }
            });
        }

        public void QueueObjectRemove(T value)
        {
            Task.Run(async () =>
            {
                try
                {
                    var contains = await removeQueue.Contains(value).ConfigureAwait(false);

                    if (contains)
                        return;

                    await removeQueue.Add(value).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteFatalException(ex);
                }
            });
        }
    }

    public class ChildLoaderDictionary<K, V>
    {
        private CancellationTokenSource loaderCancellationTokenSource;

        private bool queueReadyForMainThreadProcessing = false;
        private ThreadSafeDictionary<K, V> addQueue;
        private Dictionary<K, V> addQueueCopy;

        private ThreadSafeList<K> removeQueue;
        private HashSet<K> removeQueueCopy;

        /// <summary>
        /// Run a background task to handle the queuing of objects to be loaded
        /// </summary>
        /// <param name="addAction">An action to perform on the items before being loaded</param>
        /// <param name="removeAction">An action to perform on the items before being unloaded</param>
        public void Run(Func<Dictionary<K, V>, Task> addAction, Func<HashSet<K>, Task> removeAction)
        {
            loaderCancellationTokenSource = new CancellationTokenSource();
            Log.Instance.Write("Starting loader thread");

            addQueue = new ThreadSafeDictionary<K, V>();
            removeQueue = new ThreadSafeList<K>();

            addQueueCopy = new Dictionary<K, V>();
            removeQueueCopy = new HashSet<K>();

#pragma warning disable CS4014
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (loaderCancellationTokenSource.IsCancellationRequested)
                            break;

                        if (queueReadyForMainThreadProcessing)
                        {
                            await Task.Delay(10).ConfigureAwait(false);
                            continue;
                        }

                        addQueueCopy = await addQueue.Copy().ConfigureAwait(false);
                        await addQueue.Clear().ConfigureAwait(false);

                        if (addAction != null && addQueueCopy.Count > 0)
                            await addAction(addQueueCopy).ConfigureAwait(false);

                        removeQueueCopy = await removeQueue.CopyToHashSet().ConfigureAwait(false);
                        await removeQueue.Clear().ConfigureAwait(false);

                        if (removeAction != null && removeQueueCopy.Count > 0)
                            await removeAction(removeQueueCopy).ConfigureAwait(false);

                        queueReadyForMainThreadProcessing = true;

                        await Task.Delay(100).ConfigureAwait(false);
                    }

                    loaderCancellationTokenSource.Dispose();
                    loaderCancellationTokenSource = null;
                    Log.Instance.Write("Closed loader thread");
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteException(ex);
                    Debugger.Break();
                }
            });
#pragma warning restore CS4014
        }

        public void Update(Action<K, V> addAction, Action<K> removeAction)
        {
            try
            {
                if (queueReadyForMainThreadProcessing)
                {
                    foreach (var i in addQueueCopy)
                        addAction(i.Key, i.Value);

                    foreach (var i in removeQueueCopy)
                        removeAction(i);

                    queueReadyForMainThreadProcessing = false;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteFatalException(ex);
            }
        }

        public void Stop()
        {
            if (loaderCancellationTokenSource != null && !loaderCancellationTokenSource.IsCancellationRequested && loaderCancellationTokenSource.Token.CanBeCanceled)
                loaderCancellationTokenSource.Cancel();
        }

        public void QueueObjectAdd(K key, V value)
        {
            Task.Run(async () =>
            {
                try
                {
                    var contains = await addQueue.ContainsKey(key).ConfigureAwait(false);

                    if (contains)
                        return;

                    await addQueue.Add(key, value).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteFatalException(ex);
                }
            });
        }

        public void QueueObjectRemove(K key)
        {
            Task.Run(async () =>
            {
                try
                {
                    var contains = await removeQueue.Contains(key).ConfigureAwait(false);

                    if (contains)
                        return;

                    await removeQueue.Add(key).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            });
        }
    }
}
