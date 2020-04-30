using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.InteropServices;


    public class VideoCap : MonoBehaviour
    {

        public string requestedDeviceName = null;
        public int requestedWidth = 640;
        public int requestedHeight = 480;

        public int requestedFPS = 30;

        public bool requestedIsFrontFacing = false;

        public Toggle adjustPixelsDirectionToggle;

        public bool adjustPixelsDirection = false;

        WebCamTexture webcamTexture;

        Color32[] colors;
        Color32[] rotatedColors;

        

        bool hasInitDone = false;
        ScreenOrientation screenOrientation;
        int screenWidth;
        int screenHeight;
        Texture2D texture;
        bool doprocess = true;

        public RawImage cameraView;

        public Text inferenceText;

        public bool isFrontFacing = true;
        public Button ExitButton;
        public Button noProcessBtn;


        [DllImport("SharedOcv")]
        private static extern void ProcessImage(ref Color32[] rawImage, int width, int height);

        [DllImport("SharedOcv")]
        private static extern int returnNum();

        [DllImport("SharedOcv")]
        private static extern int initBuffer(int width, int height);

        [DllImport("SharedOcv")]
        private static extern IntPtr processFrame(int width, int height, IntPtr bufferAddr);


        [DllImport("SharedOcv")]
        private static extern IntPtr rotate90Degree(int width, int height, IntPtr bufferAddr);


        [DllImport("SharedOcv")]
        private static extern void startDetector(string conf,string weight);

        [DllImport("SharedOcv")]
        private static extern IntPtr facedetector   (int width, int height, IntPtr bufferAddr);


        string configFilePath;
        string weightFilePath;

      
    
    void CopyFileAsyncOnAndroid()
    {
        string fromPath = Application.streamingAssetsPath +"/";
        //In Android = "jar:file://" + Application.dataPath + "!/assets/" 
        string toPath =   Application.persistentDataPath +"/";
        string[] filesNamesToCopy = new string[] { "deploy.prototxt","weights.caffemodel"};
        foreach (string fileName in filesNamesToCopy)
        {
            Debug.Log("copying from "+ fromPath + fileName +" to "+ toPath);
            WWW www1 = new WWW( fromPath +fileName);
            while ( ! www1.isDone) {}
            System.IO.File.WriteAllBytes(toPath+ fileName, www1.bytes);
        }
    }


        void Start()
        {      


            CopyFileAsyncOnAndroid();
            //init buffer
            initBuffer(requestedWidth, requestedHeight);

            ExitButton.onClick.AddListener(OnExitButtonClick);
            noProcessBtn.onClick.AddListener(OnNoProcessButtonClick);
            string cameraName = WebCamUtil.FindName();
            webcamTexture = new WebCamTexture(cameraName, requestedWidth, requestedHeight, 30);   
            webcamTexture.Play();


            if (colors == null || colors.Length != webcamTexture.width * webcamTexture.height)
            {
                colors = new Color32[webcamTexture.width * webcamTexture.height];
            }
            texture = new Texture2D(webcamTexture.height, webcamTexture.width, TextureFormat.RGBA32, false);
        
            cameraView.texture = texture;


            configFilePath =  Application.persistentDataPath +"/deploy.prototxt";
            weightFilePath =  Application.persistentDataPath +"/weights.caffemodel";
            startDetector(configFilePath,weightFilePath);
            Debug.Log("weight files initialized");
           


        
         }



        void Update()
        {

            Color32[] colors = webcamTexture.GetPixels32();
            float startTimeSeconds = Time.realtimeSinceStartup;
            //update texture here

            if(doprocess)
            {
                GCHandle pixelHandle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr results = facedetector(webcamTexture.width, webcamTexture.height, pixelHandle.AddrOfPinnedObject());
                int bufferSize = webcamTexture.width * webcamTexture.height * 4;
                byte[] rawData = new byte[bufferSize];
                 if (results != IntPtr.Zero)
                {
                    Marshal.Copy(results, rawData, 0, bufferSize);
                    texture.LoadRawTextureData(rawData);
                    texture.Apply();
                }
                rawData = null;
                pixelHandle.Free();

            }
            else{
                GCHandle pixelHandle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr results = rotate90Degree(webcamTexture.width, webcamTexture.height, pixelHandle.AddrOfPinnedObject());
                int bufferSize = webcamTexture.width * webcamTexture.height * 4;
                byte[] rawData = new byte[bufferSize];
                if (results != IntPtr.Zero)
                {
                    Marshal.Copy(results, rawData, 0, bufferSize);
                    texture.LoadRawTextureData(rawData);
                    texture.Apply();
                }
                rawData = null;
                pixelHandle.Free();
            }

            cameraView.texture = texture;

            float inferenceTimeSeconds = Time.realtimeSinceStartup - startTimeSeconds;

            inferenceText.text = string.Format(
            " {0:0.0000} ms\n",
            inferenceTimeSeconds * 1000.0);

            
        

        }

           void Rotate90CCW(Color32[] src, Color32[] dst, int width, int height)
        {
            int i = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    dst[i] = src[x + y * width];
                    i++;
                }
            }
        }

        void Rotate90CW(Color32[] src, Color32[] dst, int height, int width)
        {
            int i = 0;
            for (int x = height - 1; x >= 0; x--)
            {
                for (int y = 0; y < width; y++)
                {
                    dst[i] = src[x + y * height];
                    i++;
                }
            }
        }



         private Color32[] GetColors()
        {
              webcamTexture.GetPixels32(colors);
              Rotate90CW(colors, rotatedColors, webcamTexture.width, webcamTexture.height);
              FlipColors(rotatedColors, webcamTexture.width, webcamTexture.height);
              return rotatedColors;
        }




         void FlipColors(Color32[] colors, int width, int height)
        {
            FlipVertical(colors, colors, height, width);
        }
        void FlipVertical(Color32[] src, Color32[] dst, int width, int height)
        {
            for (var i = 0; i < height / 2; i++)
            {
                var y = i * width;
                var x = (height - i - 1) * width;
                for (var j = 0; j < width; j++)
                {
                    int s = y + j;
                    int t = x + j;
                    Color32 c = src[s];
                    dst[s] = src[t];
                    dst[t] = c;
                }
            }
        }

        /// <summary>
        /// Flips horizontal.
        /// </summary>
        /// <param name="src">Src colors.</param>
        /// <param name="dst">Dst colors.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        void FlipHorizontal(Color32[] src, Color32[] dst, int width, int height)
        {
            for (int i = 0; i < height; i++)
            {
                int y = i * width;
                int x = y + width - 1;
                for (var j = 0; j < width / 2; j++)
                {
                    int s = y + j;
                    int t = x - j;
                    Color32 c = src[s];
                    dst[s] = src[t];
                    dst[t] = c;
                }
            }
        }


        
        private void OnExitButtonClick()
        {
            Application.Quit();
        }

        private void OnNoProcessButtonClick()
        {
            if(doprocess)
            {doprocess = false;}
            else
            {
                doprocess=true;
            }
        }

    }