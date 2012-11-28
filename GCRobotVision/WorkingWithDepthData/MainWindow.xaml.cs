// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Diagnostics;
using System.IO;
//using System.Drawing;

namespace WorkingWithDepthData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        //Ignore these 3 variables
        const float MaxDepthDistance = 4095; // max value returned
        const float MinDepthDistance = 850; // min value returned
        const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;
        
        private int counter = 0; //number of frames since last save
        private string output = "output.txt";
        private string OutputImage = "output.jpg"; //output file names
        //private string DepthImage = "depth.jpg"; 
        //private string ColorImage = "color.jpg";
        private int delay = 60; //number of frames between saves
        /*
        public static int xoffset = 0;
        public static int yoffset = 25;
        public static int xcenter = 320;
        public static int ycenter = 240;
        public static int scaling = 1;
        */

        //Set Boundaries of the Connect 4 Board

        private static KinectSensor sensor;
        private static int angle = 0; //camera angle: should be between -27 and 27 degrees

        private static int off1 = 60;
        private static int off2 = 45;
        private static int offw = 5;
        private static int offh = 5;
        private static int offx = -5;
        private static int offy = -18;

        private static int colorminx = 320-off1;//-240; //boundaries of board on color image
        private static int colormaxx = 320+off1;//+240;
        private static int colorminy = 240-off2;//-180;
        private static int colormaxy = 240+off2;//+180;

        private static int colorimagewidth = colormaxx - colorminx;
        private static int colorimageheight = colormaxy - colorminy;
        private static int colordatalength = colorimagewidth * colorimageheight * 4; //in bytes
        
        private static int depthminx = 320-off1-offw+offx;//240; //boundaries of board on depth image
        private static int depthmaxx = 320+off1+offw+offx;//+240;
        private static int depthminy = 240-off2-offh+offy;//-180;
        private static int depthmaxy = 240+off2+offh+offy;//+180;

        private static int depthimagewidth = depthmaxx - depthminx;
        private static int depthimageheight = depthmaxy - depthminy;
        private static int depthdatalength = depthimagewidth * depthimageheight * 4; //in bytes
        
        public static byte[] pixels1; //depth data
        public static byte[] pixels2; //color data
        public static byte[] pixels3; //cropped depth data
        public static byte[] pixels4; //cropped color data
        public static byte[] pixels5; //overlayed image data

        private static int mindist = 800; //distance in mm: board should be between min and max dist away from camera
        private static int maxdist = 1600;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
        }

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            var oldSensor = (KinectSensor)e.OldValue;

            //stop the old sensor
            if (oldSensor != null)
            {
                oldSensor.Stop();
                oldSensor.AudioSource.Stop();
            }

            //get the new sensor
            var newSensor = (KinectSensor)e.NewValue;
            if (newSensor == null)
            {
                return;
            }

            sensor = newSensor;

            //turn on features that you need
            newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);                
            newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            newSensor.SkeletonStream.Enable();
                        
            //sign up for events if you want to get at API directly
            newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(newSensor_AllFramesReady);
            

            try
            {
                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                //this happens if another app is using the Kinect
                kinectSensorChooser1.AppConflictOccurred();
            }

            try
            {
                newSensor.ElevationAngle = (int) slider1.Value;
            }
            catch (System.InvalidOperationException) { }

        }

        void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }

                pixels2 = new byte[colorFrame.PixelDataLength];
                pixels4 = new byte[colordatalength];
                colorFrame.CopyPixelDataTo(pixels2);
                for (int i = colorminy; i < colormaxy; i++)
                {
                    Array.Copy(pixels2, (i * colorFrame.Width + colorminx) * 4, pixels4, (i - colorminy) * colorimagewidth * 4, colorimagewidth * 4);
                }

                //number of bytes per row width * 4 (B,G,R,Empty)
                int stride = colorimagewidth * 4;

                //create image
                image2.Source =
                    BitmapSource.Create(colorimagewidth, colorimageheight,
                    96, 96, PixelFormats.Bgr32, null, pixels4, stride);
                
            }

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }

                pixels1 = GenerateColoredBytes(depthFrame);
                pixels3 = new byte[depthdatalength];
                for (int i = depthminy; i < depthmaxy; i++)
                {
                    Array.Copy(pixels1, (i * depthFrame.Width + depthminx) * 4, pixels3, (i - depthminy) * depthimagewidth * 4, depthimagewidth * 4);
                }
            
                //number of bytes per row width * 4 (B,G,R,Empty)
                int stride = depthimagewidth * 4;

                //create image
                image1.Source =
                    BitmapSource.Create(depthimagewidth, depthimageheight,
                    96, 96, PixelFormats.Bgr32, null, pixels3, stride);
            }

            if (counter == 0)
            {
                try
                {
                    sensor.ElevationAngle = (int)slider1.Value;
                }
                catch (System.InvalidOperationException) { }

                pixels5 = imageMerge(pixels3, depthimagewidth, depthimageheight, pixels4, colorimagewidth, colorimageheight);
                int stride = depthimagewidth * 4;
                image3.Source = BitmapSource.Create(depthimagewidth, depthimageheight, 96, 96, PixelFormats.Bgr32, null, pixels5, stride);

                image4.Source = image3.Source;

                SaveImageToJPG(image4, OutputImage);

                /*
                using (StreamWriter newTask = new StreamWriter(output, false))
                {
                    newTask.WriteLine(calculateboardstring());
                }*/

                off1 = (int)slider2.Value;
                off2 = (int)slider3.Value;

                colorminx = 320 - off1;//-240; //boundaries of board on color image
                colormaxx = 320 + off1;//+240;
                colorminy = 240 - off2;//-180;
                colormaxy = 240 + off2;//+180;

                colorimagewidth = colormaxx - colorminx;
                colorimageheight = colormaxy - colorminy;
                colordatalength = colorimagewidth * colorimageheight * 4; //in bytes

                depthminx = 320 - off1 - offw + offx;//240; //boundaries of board on depth image
                depthmaxx = 320 + off1 + offw + offx;//+240;
                depthminy = 240 - off2 - offh + offy;//-180;
                depthmaxy = 240 + off2 + offh + offy;//+180;

                depthimagewidth = depthmaxx - depthminx;
                depthimageheight = depthmaxy - depthminy;
                depthdatalength = depthimagewidth * depthimageheight * 4; //in bytes

                border4.Height = depthimageheight;
                border4.Width = depthimagewidth;
                image4.Height = depthimageheight;
                image4.Width = depthimagewidth;
                counter++;
            }
            else if (counter > delay)
            {
                counter = 0;
            }
            else counter++;

        }

        private string calculateboardstring()
        {
            string bs = "";
            for (int i = depthimagewidth / 14; i < depthimagewidth; i += depthimagewidth / 7)
            {
                for (int j = depthimageheight - (depthimageheight / 14); j > 0; j -= depthimageheight / 6)
                {
                    bs = bs + calculateboardchar(i, j);
                }

            }

                return bs;}

            private char calculateboardchar(int x, int y){
                int red = 0;
                int black = 0;
                int white = 0;
                byte[] b = new byte[4];
                int index = 0;
                for (int i = x-4; i<=x+4; i += 2){
                for (int j = y-4; j<=y+4; j += 2){
                    index = (i + j * depthimagewidth)*4;
                    b[0] = pixels5[index + 0];
                    b[1] = pixels5[index + 1];
                    b[2] = pixels5[index + 2];
                    b[3] = pixels5[index + 3];
                    if (b[0]>=96 && b[1]>=96 && b[2]>=96){
                    white++;
                    }
                    else if(b[0]<64 && b[1]<64 && b[2]>=96){
                    red++;
                    }
                    else if(b[0]<64 && b[1]<64 && b[2]<64){
                    black++;
                    }
                }
                }

                if (white>red && white>black){
                    return ' ';}
                else if (red>white && red>black){
                    return 'X';}
                else if (black>white && black>red){
                    return 'O';}
                else return ' ';
            }


        internal byte[] GetEncodedImageData(ImageSource image, string preferredFormat){
            byte[] result = null;
            BitmapEncoder encoder = null;
            switch (preferredFormat.ToLower()){
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".tif":
                case ".tiff":
                    encoder = new TiffBitmapEncoder();
                    break;
                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case ".wmp":
                    encoder = new WmpBitmapEncoder();
                    break;
            }
            if (image is BitmapSource){
                MemoryStream stream = new MemoryStream();
                encoder.Frames.Add(
                    BitmapFrame.Create(image as BitmapSource));
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                result = new byte[stream.Length];
                BinaryReader br = new BinaryReader(stream);
                br.Read(result, 0, (int)stream.Length);
                br.Close();
                stream.Close();
            }
            return result;
        }

        private byte[] imageMerge(byte[] image1, int width1, int height1, byte[] image2, int width2, int height2)
        {
            byte[] result = new byte[image1.Length];
            int ii;
            int jj;

            float w21 = (float) width2 / (float) width1;
            float h21 = (float) height2 / (float) height1;

            int depthindex;
            int colorindex;
            const int blueindex = 0;
            const int greenindex = 1;
            const int redindex = 2;
            const int alphaindex = 3;

            //image1 = depth image
            //image2 = color image
            for (int i = 0; i < width1; i++)
            {
                ii = (int) (i * w21);
                for (int j = 0; j < height1; j++)
                {
                    //If depth pixel is black, replace with pixel from color image
                    depthindex = (i + width1 * j) * 4;
                    if (image1[depthindex] == 0)
                    {
                        jj = (int)(j * h21);
                        colorindex = (ii + width2 * jj) * 4;
                        result[depthindex + blueindex] = image2[colorindex + blueindex];
                        result[depthindex + greenindex] = image2[colorindex + greenindex];
                        result[depthindex + redindex] = image2[colorindex + redindex];
                        result[depthindex + alphaindex] = image2[colorindex + alphaindex];
                    }
                    else
                    {
                        result[depthindex + blueindex] = 255;
                        result[depthindex + greenindex] = 255;
                        result[depthindex + redindex] = 255;
                        result[depthindex + alphaindex] = 255;
                    }
                }
            }
            return result;
        }

        private void OldResizeImage(byte[] imageBytes, byte[] imageTarget, int width, int height)
        {
            ImageSource imageSource = CreateImage(imageBytes, width, height);
            imageBytes = GetEncodedImageData(imageSource, ".bmp");
            Array.Copy(imageBytes, imageTarget, imageBytes.Length);
        }

        private static ImageSource CreateImage(byte[] imageData,int decodePixelWidth, int decodePixelHeight){
            if (imageData == null) return null;
            BitmapImage result = new BitmapImage();
            result.BeginInit();
           if (decodePixelWidth > 0){
                result.DecodePixelWidth = decodePixelWidth;
            }
            if (decodePixelHeight > 0){
                result.DecodePixelHeight = decodePixelHeight;
            }
            result.StreamSource = new MemoryStream(imageData);
            result.CreateOptions = BitmapCreateOptions.None;
            result.CacheOption = BitmapCacheOption.Default;
            result.EndInit();
            return result;
        }
        
        private void SaveImageToJPG(Image ImageToSave, string Location)
        {
            
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)ImageToSave.Source.Width,
                                                                           (int)ImageToSave.Source.Height,
                                                                           96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(ImageToSave);
            JpegBitmapEncoder jpgBitmapEncoder = new JpegBitmapEncoder();
            jpgBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (FileStream fileStream = new FileStream(Location, FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                jpgBitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }

        }

        private void SaveImageToPNG(Image ImageToSave, string Location)
        {

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)ImageToSave.Source.Width,
                                                                           (int)ImageToSave.Source.Height,
                                                                           96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(ImageToSave);
            PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (FileStream fileStream = new FileStream(Location, FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                pngBitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }

        }


        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {

            //get the raw data from kinect with the depth for every pixel
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData); 

            //use depthFrame to create the image to display on-screen
            //depthFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            //Bgr32  - Blue, Green, Red, empty byte
            //Bgra32 - Blue, Green, Red, transparency 
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

            //hardcoded locations to Blue, Green, Red (BGR) index positions       
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;
            const int AlphaIndex = 3;
            
            //loop through all distances
            //pick a RGB color based on distance
            for (int depthIndex = 0, colorIndex = 0; 
                depthIndex < rawDepthData.Length && colorIndex < pixels.Length; 
                depthIndex++, colorIndex += 4)
            {
                //get the player (requires skeleton tracking enabled for values)
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                //gets the depth value
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // 0-0.8M
                if (depth <= mindist)
                {
                    //we are very close
                    pixels[colorIndex + BlueIndex] = 255;
                    pixels[colorIndex + GreenIndex] = 255;
                    pixels[colorIndex + RedIndex] = 255;
                    pixels[colorIndex + AlphaIndex] = 255;
                }
                // 0.8M - 1.8M
                else if (depth > mindist && depth<=maxdist)
                {
                    //we are a bit further away

                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;
                    pixels[colorIndex + AlphaIndex] = 0;
                    

                    /*
                    pixels[colorIndex + BlueIndex] = pixels2[colorIndex + BlueIndex];
                    pixels[colorIndex + GreenIndex] = pixels2[colorIndex + GreenIndex];
                    pixels[colorIndex + RedIndex] = pixels2[colorIndex + RedIndex];
                    pixels[colorIndex + AlphaIndex] = pixels2[colorIndex + AlphaIndex];
                    */
                    
                }
                    // 1.8M+
                else
                {
                    //we are a bit further away
                    pixels[colorIndex + BlueIndex] = 255;
                    pixels[colorIndex + GreenIndex] = 255;
                    pixels[colorIndex + RedIndex] = 255;
                    pixels[colorIndex + AlphaIndex] = 255;
                }

                ////equal coloring for monochromatic histogram
                
                /*
                byte intensity = CalculateIntensityFromDepth(depth);
                pixels[colorIndex + BlueIndex] = intensity;
                pixels[colorIndex + GreenIndex] = intensity;
                pixels[colorIndex + RedIndex] = intensity;
                */

                //Color all players "gold"
                
                /*
                if (player > 0)
                {
                    pixels[colorIndex + BlueIndex] = Colors.Gold.B;
                    pixels[colorIndex + GreenIndex] = Colors.Gold.G;
                    pixels[colorIndex + RedIndex] = Colors.Gold.R;
                }
                 */

            }
          

            return pixels;
        }


        public static byte CalculateIntensityFromDepth(int distance)
        {
            //formula for calculating monochrome intensity for histogram
            return (byte)(255 - (255 * Math.Max(distance - MinDepthDistance, 0) 
                / (MaxDepthDistanceOffset)));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(kinectSensorChooser1.Kinect); 
        }

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }

        private void image1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void kinectSensorChooser1_Loaded(object sender, RoutedEventArgs e)
        {

        } 



    }

}

