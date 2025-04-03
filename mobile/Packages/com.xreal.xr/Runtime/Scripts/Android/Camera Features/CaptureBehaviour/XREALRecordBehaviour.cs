using UnityEngine;

namespace Unity.XR.XREAL
{
    public class XREALRecordBehaviour : CaptureBehaviourBase
    {
        /// <summary> Sets out put path. </summary>
        /// <param name="path"> Full pathname of the file.</param>
        public void SetOutPutPath(string path)
        {
            var encoder = this.GetContext().GetEncoder();
            Debug.Log($"[NRRecorderBehaviour] SetOutPutPath {encoder?.GetType()}");
            ((VideoEncoder)encoder).EncodeConfig.SetOutPutPath(path);
        }
    }
}
