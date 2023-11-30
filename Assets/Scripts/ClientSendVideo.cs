using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.IO;
using NRKernal;
using OpenCvSharp;
using NRKernal.Record;
using System.Linq;

public class ClientSendVideo : MonoBehaviour
{
    public Text extraInfo;
    public Text mainInfo;

    //public RawImage CaptureImage;

    private Socket clientSocket;
    private byte[] buffer = new byte[1024];
    string receivedMessage = "";
    byte[] receivedData = new byte[0];
    string messageSend = "";

    //public float framesPerSecond = 60.0f; // Set your desired frame rate here.
    
    //private float timeBetweenFrames;
    private Texture2D targetTexture;
    byte[] frameData;

    int value = 0;
    private string delimiter = "|||";

    HandState handState;
    HandGesture currentRightGesture;
    public HandEnum handEnum;

    /// <summary> The photo capture object. </summary>
    private NRPhotoCapture m_PhotoCaptureObject;
    /// <summary> The camera resolution. </summary>
    private Resolution m_CameraResolution;
    private bool isOnPhotoProcess = false;

    private void Start()
    {
        TakeAPhoto();
        //timeBetweenFrames = 1.0f / framesPerSecond; // Calculate the time between frames.

        StartClient();
    }

    void Create(Action<NRPhotoCapture> onCreated)
    {
        if (m_PhotoCaptureObject != null)
        {
            NRDebugger.Info("The NRPhotoCapture has already been created.");
            return;
        }

        // Create a PhotoCapture object
        NRPhotoCapture.CreateAsync(false, delegate (NRPhotoCapture captureObject)
        {
            m_CameraResolution = NRPhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

            if (captureObject == null)
            {
                NRDebugger.Error("Can not get a captureObject.");
                return;
            }

            m_PhotoCaptureObject = captureObject;

            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.cameraResolutionWidth = m_CameraResolution.width;
            cameraParameters.cameraResolutionHeight = m_CameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
            cameraParameters.frameRate = NativeConstants.RECORD_FPS_DEFAULT;
            cameraParameters.blendMode = BlendMode.Blend;

            // Activate the camera
            m_PhotoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (NRPhotoCapture.PhotoCaptureResult result)
            {
                NRDebugger.Info("Start PhotoMode Async");
                if (result.success)
                {
                    onCreated?.Invoke(m_PhotoCaptureObject);
                }
                else
                {
                    isOnPhotoProcess = false;
                    this.Close();
                    NRDebugger.Error("Start PhotoMode faild." + result.resultType);
                }
            }, true);
        });
    }

    /// <summary> Take a photo. </summary>
    void TakeAPhoto()
    {
        if (isOnPhotoProcess)
        {
            NRDebugger.Warning("Currently in the process of taking pictures, Can not take photo .");
            return;
        }

        isOnPhotoProcess = true;
        if (m_PhotoCaptureObject == null)
        {
            this.Create((capture) =>
            {
                capture.TakePhotoAsync(OnCapturedPhotoToMemory);
            });
        }
        else
        {
            m_PhotoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
    }

    private void Update()
    {
        if (receivedMessage != null)
        {
            extraInfo.text = "Read Data: " + receivedMessage;
        }
        mainInfo.text = "Data sent to Server: " + messageSend;

        TakeAPhoto();

        if (targetTexture != null)
        {
            //CaptureImage.texture = targetTexture;
            frameData = targetTexture.EncodeToJPG();
            //frameData = frameTexture.EncodeToPNG();
        }
        
        
    }

    void OnCapturedPhotoToMemory(NRPhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        targetTexture = new Texture2D(m_CameraResolution.width, m_CameraResolution.height);
        // Copy the raw image data into our target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        //SaveTextureToGallery(targetTexture);
        // Release camera resource after capture the photo.
        this.Close();
    }

    private void StartClient()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //IPAddress serverIP = IPAddress.Parse("192.168.18.17");  //Home ip
        //IPAddress serverIP = IPAddress.Parse("10.169.2.3"); // Replace with the server IP address. Prof pc ip
        //IPAddress serverIP = IPAddress.Parse("172.20.10.2"); // Iphone IP
        IPAddress serverIP = IPAddress.Parse("10.169.129.57"); // Replace with the server IP address.
        int serverPort = 1755; // Replace with the server port number.

        Debug.Log("Connecting to the server...");
        clientSocket.BeginConnect(new IPEndPoint(serverIP, serverPort), new AsyncCallback(OnConnect), null);
    }

    private void OnConnect(IAsyncResult ar)
    {
        clientSocket.EndConnect(ar);
        Debug.Log("Connected to the server!");

        // Start receiving data from the server.
        clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), clientSocket);

        // Start sending data to the client every second.
        Thread sendThread = new Thread(SendDataLoop);
        sendThread.Start();

    }

    private void OnReceiveData(IAsyncResult ar)
    {
        Socket clientSocket = (Socket)ar.AsyncState;
        int bytesRead = clientSocket.EndReceive(ar);
        receivedData = new byte[bytesRead];
        Array.Copy(buffer, receivedData, bytesRead);
        receivedMessage = Encoding.ASCII.GetString(receivedData);
        if (receivedData.Length > 0) {
            Debug.Log("Read " + receivedData.Length + " bytes from server.\n Data: " + receivedMessage);
        }
        

        // Continue receiving data.
        clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), clientSocket);

    }

    private void SendDataLoop()
    {
        
        
        while (true) // Continuously send image data.
        {
            // Capture a frame from the camera or load an image as needed.
            //Texture2D frameTexture = CaptureFrame(); // Implement a method to capture the frame.
            
            if (targetTexture != null && frameData != null)
            {
                
                // Create headers.
                string contentType = "image/jpg";
                string contentLength = frameData.Length.ToString();
                //string date = DateTime.Now.ToString("r");
                //string webcamTextureWidth = DateTime.Now.ToString("r");
                //string lastModified = DateTime.Now.ToString("r");

                // Process the received headers, content type, date, and last modified as needed.
                Debug.Log("Content-Type: " + contentType);
                Debug.Log("Content-Length: " + contentLength);
                //Debug.Log("Date: " + RGBCamTextureWidth);
                //Debug.Log("Last-Modified: " + RGBCamTextureHeight);

                // Combine headers and frame data using the delimiter.
                //string combinedData = contentType + delimiter + contentLength + delimiter + RGBCamTextureWidth + delimiter + RGBCamTextureHeight + delimiter + Convert.ToBase64String(frameData);
                string imageData = Convert.ToBase64String(frameData, 0, frameData.Length);
                string imageStringLength = imageData.Length.ToString();
                string combinedData = contentType + delimiter + contentLength + delimiter + imageStringLength + delimiter + imageData;
                
                // Send the combined data to the server.
                SendData(combinedData);

                // Encode the frame to JPEG.
                //byte[] frameData = frameTexture.EncodeToJPG();

                // Create headers.
                //string headers = "Content-Type: image/jpeg|||Content-Length: " + frameData.Length + "|||Date: " + DateTime.Now.ToString("r") + "|||Last-Modified: " + DateTime.Now.ToString("r");

                // Combine headers and frame data.
                //string fullData = headers + "|||" + Convert.ToBase64String(frameData);

                //string fullData = Convert.ToBase64String(frameData);


                // Send the combined data to the server.
                //SendData(fullData);
            }

            // Wait for one second before sending the next frame.
            //Thread.Sleep(1000);
            Thread.Sleep(200);
            //value = value + 1;
        }
        

        /*
        for (int i = 0; i < 3; i++)
        {
            // Send the string to the client.
            messageSend = "Hello, Server! " + DateTime.Now.ToString();
            byte[] messageBytes = Encoding.ASCII.GetBytes(messageSend);
            clientSocket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, new AsyncCallback(OnSendData), clientSocket);

            // Wait for one second before sending the next message.
            Thread.Sleep(1000);
        }
        */
    }

    private void SendData(string data)
    {
        byte[] messageBytes = Encoding.ASCII.GetBytes(data);
        Debug.Log("########################messageBytes" + messageBytes.Length.ToString());
        //Debug.Log("data start" + data);
        //Debug.Log("data end " );
        Debug.Log("########frameData" + frameData.Length.ToString());
        //string filePath = "./Assets/PLYFiles/image.jpg";
        //File.WriteAllBytes(filePath, frameData);
        //File.WriteAllBytes("/storage/emulated/0/Nreal/image.png", frameData);
        clientSocket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, new AsyncCallback(OnSendData), clientSocket);
    }

    private void OnSendData(IAsyncResult ar)
    {
        Socket clientSocket = (Socket)ar.AsyncState;
        clientSocket.EndSend(ar);
        Debug.Log("Data sent to Server." + messageSend);
    }

    private void OnApplicationQuit()
    {
        // Clean up resources when the application is closed.
        //isRunning = false;
        if (clientSocket != null && clientSocket.Connected)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }



    void Close()
    {
        if (m_PhotoCaptureObject == null)
        {
            NRDebugger.Error("The NRPhotoCapture has not been created.");
            return;
        }
        // Deactivate our camera
        m_PhotoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    /// <summary> Executes the 'stopped photo mode' action. </summary>
    /// <param name="result"> The result.</param>
    void OnStoppedPhotoMode(NRPhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown our photo capture resource
        m_PhotoCaptureObject?.Dispose();
        m_PhotoCaptureObject = null;
        isOnPhotoProcess = false;
    }

    /// <summary> Executes the 'destroy' action. </summary>
    void OnDestroy()
    {
        // Shutdown our photo capture resource
        m_PhotoCaptureObject?.Dispose();
        m_PhotoCaptureObject = null;
    }

}
