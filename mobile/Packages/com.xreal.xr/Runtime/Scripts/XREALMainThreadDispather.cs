using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// A singleton MonoBehaviour class that dispatches actions to be executed on the Unity main thread.
    /// </summary>
    public class XREALMainThreadDispather : SingletonMonoBehaviour<XREALMainThreadDispather>
    {
        ConcurrentQueue<Action> m_Actions = new ConcurrentQueue<Action>();
        ConcurrentQueue<Action> m_RunningActions = new ConcurrentQueue<Action>();

        /// <summary>
        /// Event invoked every frame during the Update method.
        /// </summary>
        public static event Action OnUpdate;

        /// <summary>
        /// Queues an action to be executed on the Unity main thread in the next frame.
        /// </summary>
        /// <param name="action">The action to execute on the main thread.</param>
        public void QueueOnMainThread(Action action)
        {
            m_Actions.Enqueue(action);
        }

        /// <summary>
        /// Queues an action to be executed on the Unity main thread after a specified delay.
        /// Returns a CancellationTokenSource that can be used to cancel the scheduled action.
        /// </summary>
        /// <param name="action">The action to execute on the main thread.</param>
        /// <param name="delaySeconds">The delay in seconds before the action is executed.</param>
        /// <returns>A CancellationTokenSource that can be used to cancel the scheduled action.</returns>
        public CancellationTokenSource QueueOnMainThreadWithDelay(Action action, float delaySeconds)
        {
            CancellationTokenSource ctSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                if (!ctSource.IsCancellationRequested)
                    m_Actions.Enqueue(action);
            }, ctSource.Token);
            return ctSource;
        }

        void Update()
        {
            OnUpdate?.Invoke();
            if (m_Actions.Count > 0)
            {
                (m_Actions, m_RunningActions) = (m_RunningActions, m_Actions);
                while (m_RunningActions.TryDequeue(out var action))
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void OnApplicationPause(bool pause)
        {
            Debug.Log($"[XREALMainThreadDispather] OnApplicationPause: {pause}");
            if (pause)
            {
                XREALPlugin.PauseSession();
            }
            else
            {
                XREALPlugin.ResumeSession();
            }
        }
#endif
    }
}
