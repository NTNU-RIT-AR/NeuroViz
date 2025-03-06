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

    public static TCP Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable
            else DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
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

    private void onDeepLinkActivated(string url)
    {
        string ipAddress = url.Split('?')[1];
        ip = ipAddress;
    }

    //private void ProcessDeepLink(string url)
    //{
    //    Uri uri = new Uri(url);
    //    Console.Write(url);
    //    var ipAddress= System.Web.HttpUtility.ParseQueryString(uri.Query).Get("ipaddress");

    //    if (!string.IsNullOrEmpty(ipAddress))  // Safely check for null or empty values
    //    {
    //        ip = ipAddress;
    //    }
    //}


    private void OnDestroy()
    {
        tcpThread.Abort();
        client.Close();
    }
    // TODO: Destroy the thread when the game is closed
    
    
    private void ListenForData() { 		
        try {
            client = new TcpClient(ip, port);
            var buffer = new byte[1024];       
            var stringBuilder = new StringBuilder(); // Accumulate data until we find a newline
            
            while (true)
            {
                // Get a stream object for reading 				
                using NetworkStream stream = client.GetStream();
                int length; 					
                
                while ((length = stream.Read(buffer, 0, buffer.Length)) != 0) {
                    try
                    {
                        // var incomingData = new byte[length]; 	
                        // Array.Copy(buffer, 0, incomingData, 0, length); 						
                        // Convert byte array to string message. 						
                        var incomingData = Encoding.ASCII.GetString(buffer, 0, length); 				
                        Debug.Log("incomingData: " + incomingData);
                        stringBuilder.Append(incomingData);
                        // Debug.Log("server message received as: " + serverMessage); 	
                    
                        string accumulatedData = stringBuilder.ToString();
                        string[] messages = accumulatedData.Split(new[] { "\n" }, StringSplitOptions.None);

                        for (int i = 0; i < messages.Length - 1; i++) {
                            // Each complete message (excluding the last, which might be incomplete)
                            string serverMessage = messages[i].Trim();
                        
                            Debug.Log("server message received as: " + serverMessage);
                            var jsonData = JsonUtility.FromJson<JsonData>(serverMessage);

                            lock (_lockObject) {
                                data = jsonData;
                            }
                        }
                        
                        stringBuilder.Clear();
                        stringBuilder.Append(messages[^1]); // The last element
                        // var jsonData = JsonUtility.FromJson<JsonData>(serverMessage);
                        //
                        // lock (_lockObject)
                        // {
                        //     data = jsonData;
                        // }
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
            // var sliders = new List<float>() {
            //     data.slider1,
            //     data.slider2,
            //     data.slider3,
            //     data.slider4
            // };

            foreach (var mesh in objects) {
                mesh.material.SetFloat("_Hue", data.slider1);
                mesh.material.SetFloat("_Smoothness", data.slider2);
                mesh.material.SetFloat("_Metallic", data.slider3);
                mesh.material.SetFloat("_Emission", data.slider4);
            }

            // foreach (var (slider, mesh) in sliders.Zip(objects, (slider, renderObject) => (slider,renderObject)))
            // {
            //     Debug.Log(slider);
            //     var oldColor = mesh.material.color;
            //     mesh.material.color = new Color(oldColor.r, oldColor.g, oldColor.b, slider);
            // }
        }
    }
}
