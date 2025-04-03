using System.Collections;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> An editor frame provider. </summary>
    public class NullDataFrameProvider : AbstractFrameProvider
    {
        protected UniversalTextureFrame m_DefaultFrame;
        private bool m_IsPlay = false;
        private int FPS;

        /// <summary> Default constructor. </summary>
        public NullDataFrameProvider(int fps)
        {
            FPS = fps;
            m_DefaultFrame = new UniversalTextureFrame();
            XREALMainThreadDispather.Singleton.StartCoroutine(UpdateFrame());
        }

        /// <summary> Updates the frame. </summary>
        /// <returns> An IEnumerator. </returns>
        public IEnumerator UpdateFrame()
        {
            float timeInteval = 1f * 0.9f / FPS;
            float timeLast = 0f;
            while (true)
            {
                if (m_IsPlay)
                {
                    if (timeLast >= timeInteval)
                    {
                        m_DefaultFrame.timeStamp = (ulong)(Time.realtimeSinceStartup * 1000);
                        OnUpdate?.Invoke(m_DefaultFrame);
                        m_IsFrameReady = true;
                        timeLast = 0;
                    }
                }
                yield return new WaitForEndOfFrame();
                timeLast += Time.unscaledDeltaTime;
            }
        }

        /// <summary> Plays this object. </summary>
        public override void Play()
        {
            m_IsPlay = true;
        }

        /// <summary> Stops this object. </summary>
        public override void Stop()
        {
            m_IsPlay = false;
        }

        /// <summary> Releases this object. </summary>
        public override void Release()
        {
            m_IsPlay = false;
            XREALMainThreadDispather.Singleton.StopCoroutine(UpdateFrame());
        }
    }
}
