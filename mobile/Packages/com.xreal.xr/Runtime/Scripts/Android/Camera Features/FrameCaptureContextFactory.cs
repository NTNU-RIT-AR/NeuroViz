using System.Collections.Generic;

namespace Unity.XR.XREAL
{
    /// <summary> A frame capture context factory. Create and dispose FrameCaptureContext. </summary>
    public class FrameCaptureContextFactory
    {
        /// <summary> List of contexts. </summary>
        private static List<FrameCaptureContext> m_ContextList = new List<FrameCaptureContext>();

        /// <summary> Creates a new FrameCaptureContext. </summary>
        /// <returns> A FrameCaptureContext. </returns>
        public static FrameCaptureContext Create()
        {
            FrameCaptureContext context = new FrameCaptureContext();

            m_ContextList.Add(context);
            return context;
        }

        /// <summary> Dispose all context. </summary>
        public static void DisposeAllContext()
        {
            foreach (var item in m_ContextList)
            {
                if (item != null)
                {
                    item.StopCapture();
                    item.Release();
                }
            }
        }
    }
}
