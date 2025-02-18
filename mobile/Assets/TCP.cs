using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

[System.Serializable]
struct JsonData
{
    public float slider1;
    public float slider2;
    public float slider3;
    public float slider4;
}

public class TCP : MonoBehaviour
{
    [SerializeField] private string ip = "localhost";
    [SerializeField] private int port = 8000;
    [SerializeField] private List<MeshRenderer> objects = new List<MeshRenderer>();
    
    private readonly object _lockObject = new();    
    private JsonData data;
    
    private Thread tcpThread;
    private TcpClient client;
    
    void Start() {
        try {  			
            tcpThread = new Thread (ListenForData) {
                IsBackground = true
            };
            
            tcpThread.Start();  		
        } 		
        catch (Exception e) { 			
            Debug.Log("On client connect exception " + e); 		
        } 	
    }
    
    // TODO: Destroy the thread when the game is closed
    
    private void ListenForData() { 		
        try {
            client = new TcpClient(ip, port);
            var buffer = new byte[1024];       
            
            while (true)
            {
                // Get a stream object for reading 				
                using NetworkStream stream = client.GetStream();
                int length; 					
                
                // Read incoming stream into byte array. 					
                while ((length = stream.Read(buffer, 0, buffer.Length)) != 0) {
                    try
                    {
                        var incomingData = new byte[length]; 						
                        Array.Copy(buffer, 0, incomingData, 0, length); 						
                        // Convert byte array to string message. 						
                        var serverMessage = Encoding.ASCII.GetString(incomingData); 						
                        Debug.Log("server message received as: " + serverMessage); 	
                    
                        var jsonData = JsonUtility.FromJson<JsonData>(serverMessage);

                        lock (_lockObject)
                        {
                            data = jsonData;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                    // Debug.Log("slider1: " + jsonData.slider1);
                    // Debug.Log("slider2: " + jsonData.slider2);
                    // Debug.Log("slider3: " + jsonData.slider3);
                    // Debug.Log("slider4: " + jsonData.slider4);
                }
            }         
        }         
        catch (SocketException socketException) {             
            Debug.Log("Socket exception: " + socketException);         
        }     
    }

    private void Update()
    {
        lock (_lockObject)
        {
            var sliders = new List<float>() {
                data.slider1,
                data.slider2,
                data.slider3,
                data.slider4
            };

            foreach (var (slider, mesh) in sliders.Zip(objects, (slider, renderObject) => (slider,renderObject)))
            {
                Debug.Log(slider);
                var oldColor = mesh.material.color;
                mesh.material.color = new Color(oldColor.r, oldColor.g, oldColor.b, slider);
            }
        }
    }
}
