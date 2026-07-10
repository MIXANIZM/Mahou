using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using NLog;

namespace Mahou
{
    internal static class InputOperationQueue
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly object Sync = new object();
        private static readonly Queue<WorkItem> Work = new Queue<WorkItem>();
        private static readonly Queue<BufferedKey> BufferedKeys = new Queue<BufferedKey>();
        private static readonly AutoResetEvent Signal = new AutoResetEvent(false);

        private static Thread worker;
        private static bool stopping;
        private static bool capturing;
        private static Keys? swallowPhysicalKeyUp;

        internal static void Start()
        {
  lock (Sync)
  {
      if (worker != null)
          return;

      stopping = false;
      worker = new Thread(WorkerLoop)
      {
          IsBackground = true,
          Name = "MIXANIZM Mahou input worker"
      };
      worker.SetApartmentState(ApartmentState.STA);
      worker.Start();
  }
        }

        internal static void Stop()
        {
  Thread thread;
  lock (Sync)
  {
      stopping = true;
      thread = worker;
  }

  Signal.Set();
  if (thread != null && thread != Thread.CurrentThread)
      thread.Join(2000);

  lock (Sync)
  {
      worker = null;
      capturing = false;
      Work.Clear();
      BufferedKeys.Clear();
      swallowPhysicalKeyUp = null;
  }
        }

        internal static void Enqueue(Action action)
        {
  EnqueueInternal(action, null);
        }

        internal static void EnqueueBoundary(Action action, Keys boundaryKey)
        {
  EnqueueInternal(action, boundaryKey);
        }

        internal static bool TryCaptureKey(Keys key, IntPtr message)
        {
  bool isDown = message == (IntPtr)(int)KMHook.KMMessages.WM_KEYDOWN ||
      message == (IntPtr)(int)KMHook.KMMessages.WM_SYSKEYDOWN;
  bool isUp = message == (IntPtr)(int)KMHook.KMMessages.WM_KEYUP ||
      message == (IntPtr)(int)KMHook.KMMessages.WM_SYSKEYUP;

  if (!isDown && !isUp)
      return false;

  lock (Sync)
  {
      if (isUp && swallowPhysicalKeyUp.HasValue && swallowPhysicalKeyUp.Value == key)
      {
          swallowPhysicalKeyUp = null;
          return true;
      }

      if (!capturing)
          return false;

      BufferedKeys.Enqueue(new BufferedKey(key, isDown));
      return true;
  }
        }

        private static void EnqueueInternal(Action action, Keys? boundaryKey)
        {
  if (action == null)
      return;

  Start();
  lock (Sync)
  {
      if (stopping)
          return;

      capturing = true;
      if (boundaryKey.HasValue)
          swallowPhysicalKeyUp = boundaryKey;
      Work.Enqueue(new WorkItem(action, boundaryKey));
  }
  Signal.Set();
        }

        private static void WorkerLoop()
        {
  while (true)
  {
      Signal.WaitOne();

      while (true)
      {
          WorkItem item;
          lock (Sync)
          {
              if (Work.Count == 0)
              {
                  if (stopping)
                  {
                      ReplayBufferedKeys();
                      return;
                  }
                  break;
              }
              item = Work.Dequeue();
          }

          RunAction(item.Action);
          if (item.BoundaryKey.HasValue)
              ReplayKeyPair(item.BoundaryKey.Value);

          bool moreWork;
          lock (Sync)
              moreWork = Work.Count != 0;
          if (!moreWork)
              ReplayBufferedKeys();
      }
  }
        }

        private static void RunAction(Action action)
        {
  bool previousSelf = KMHook.self;
  KMHook.self = true;
  try
  {
      action();
  }
  catch (Exception ex)
  {
      Log.Error(ex, "Queued input operation failed");
  }
  finally
  {
      KMHook.self = previousSelf;
  }
        }

        private static void ReplayKeyPair(Keys key)
        {
  bool previousSelf = KMHook.self;
  KMHook.self = true;
  try
  {
      KInputs.MakeInput(new[]
      {
          KInputs.AddKey(key, true),
          KInputs.AddKey(key, false)
      });
  }
  finally
  {
      KMHook.self = previousSelf;
  }
        }

        private static void ReplayBufferedKeys()
        {
  while (true)
  {
      BufferedKey[] batch;
      lock (Sync)
      {
          if (BufferedKeys.Count == 0)
          {
              capturing = false;
              return;
          }

          batch = BufferedKeys.ToArray();
          BufferedKeys.Clear();
      }

      bool previousSelf = KMHook.self;
      KMHook.self = true;
      try
      {
          var inputs = new KInputs.INPUT[batch.Length];
          for (int i = 0; i < batch.Length; i++)
              inputs[i] = KInputs.AddKey(batch[i].Key, batch[i].Down);
          KInputs.MakeInput(inputs);
      }
      finally
      {
          KMHook.self = previousSelf;
      }
  }
        }

        private sealed class WorkItem
        {
  internal readonly Action Action;
  internal readonly Keys? BoundaryKey;

  internal WorkItem(Action action, Keys? boundaryKey)
  {
      Action = action;
      BoundaryKey = boundaryKey;
  }
        }

        private struct BufferedKey
        {
  internal readonly Keys Key;
  internal readonly bool Down;

  internal BufferedKey(Keys key, bool down)
  {
      Key = key;
      Down = down;
  }
        }
    }
}
