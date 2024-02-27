// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Xna.Framework
{
    public class Threading
    {
        public const int kMaxWaitForUIThread = 750; // In milliseconds

        static int mainThreadId;

        static List<Action> actions = new List<Action>();

        static Threading()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static bool IsOnUIThread() => mainThreadId == Thread.CurrentThread.ManagedThreadId;

        public static void EnsureUIThread()
        {
            if (mainThreadId != Thread.CurrentThread.ManagedThreadId)
                throw new InvalidOperationException("Operation not called on UI thread.");
        }

        public static void BlockOnUIThread(Action action)
        {
            if (mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                action();
                return;
            }

            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            Add(() =>
            {
                action();
                resetEvent.Set();
            });
        }

        static void Add(Action action)
        {
            lock (actions)
            {
                actions.Add(action);
            }
        }

        public static void Run()
        {
            EnsureUIThread();

            lock (actions)
            {
                foreach (Action action in actions)
                    action();

                actions.Clear();
            }
        }
    }
}
