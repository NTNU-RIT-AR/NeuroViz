using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ZXing;
using ZXing.Unity;

namespace NeuroViz
{
    public class QrReader: IDisposable
    {
        public delegate (Color32[], int, int) GetTextureData();
        
        private GetTextureData getTextureData;
        private Thread qrThread;
        
        public event Action<string> OnQrCodeFound;
        
        public QrReader(GetTextureData getTextureData)
        {
            this.getTextureData = getTextureData;
            qrThread = new Thread(ScanQr);
            qrThread.Start();
        }

        private void ScanQr()
        {
            var barcodeReader = new BarcodeReader { AutoRotate = false, Options = new ZXing.Common.DecodingOptions
            {
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                TryHarder = true,
                TryInverted = true,
            } };
        
            while (true)
            {
                try
                {
                    var (pixels, width, height) = getTextureData();
                    var result = barcodeReader.Decode(pixels, width, height);

                    if (result != null)
                    {
                        OnQrCodeFound?.Invoke(result.Text);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message);
                }
            
                // Wait for a while before scanning the next frame
                Thread.Sleep(50);
            }
        }
        
        public void Dispose()
        {
            qrThread.Abort();
        }
    }
}