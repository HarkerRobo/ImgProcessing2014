#define LOCAL

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MjpegProcessor;
using System.Drawing.Imaging;
using System.Net;
using System.IO;


namespace ImageProcessing2014 {
    class Program {
#if LOCAL
		private const string IMAGE_PATH = @"C:\Users\me\Documents\The Real My Docs\School\Robotics\2013-2014\Visual Studio\2014 Image Processing\4\img5.jpg";
        //private const string IMAGE_PATH = @"Resources/goals/image4g.jpg";
        //private const string IMAGE_PATH = @"Resources/Goal yellow lights test.png";
        //private const string IMAGE_PATH = @"Resources/rt4/image10.jpg";
        //private const string IMAGE_PATH = @"Resources/rt1/photo 4.JPG";
        //private const string IMAGE_PATH = @"Resources/white.png";
#else
        private const string IMAGE_PATH = @"http://10.10.72.11/jpg/image.jpg";
        private const string MJPG_PATH = @"http://10.10.72.11/mjpg/video.mjpg";
#endif

        private const string BALLS_IMAGE = @"Resources/balls/image2.jpg";

        static void Main(string[] args) {
            Stopwatch stopwatch = new Stopwatch();

            ImageProcessing.Init();
			Console.WriteLine("Image Processing initialized");
            
#if LOCAL
            var image = (Bitmap)Image.FromFile(IMAGE_PATH);
#else
            WebRequest requestPic = WebRequest.Create(IMAGE_PATH);
            WebResponse responsePic = requestPic.GetResponse();
            Bitmap image = (Bitmap)Image.FromStream(responsePic.GetResponseStream());
            //Networking.Init();

            var savePath = @"C:\Users\me\Documents\The Real My Docs\School\Robotics\2013-2014\Visual Studio\2014 Image Processing\";
            var saveNum = 0;
            while(Directory.Exists(savePath + ++saveNum + @"\")) ; //find first unused numbered folder, then create folder and dump images into it
            savePath += saveNum + @"\";
            Directory.CreateDirectory(savePath);

            var i = 0; //iteration count for saving images
#endif

            //set camera specs in vision constants
            VisionConstants.Camera.IMAGE_WIDTH = image.Width;
            VisionConstants.Camera.IMAGE_HEIGHT = image.Height;
            BitmapData imageData = image.LockBits(
                    new Rectangle(0, 0, VisionConstants.Camera.IMAGE_WIDTH, VisionConstants.Camera.IMAGE_HEIGHT),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppRgb
                );
            VisionConstants.Camera.IMAGE_STRIDE = imageData.Stride;
            image.UnlockBits(imageData);

            Form f = new Form { Text = IMAGE_PATH, ClientSize = image.Size };

            //set double buffered true on form
            System.Reflection.PropertyInfo aProp =
                typeof(System.Windows.Forms.Control).GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance
                );
            aProp.SetValue(f, true, null);

            //print click location onclick
            f.MouseClick += (sender, e) => {
                Console.WriteLine("Click: " + e.X + " " + e.Y);
            };

            //repaint window
            f.Paint += (s, e) => {
                e.Graphics.DrawImage(image, 0, 0, image.Width, image.Height);
            };

#if LOCAL
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            double i = 0;
            timer.Tick += new EventHandler((Object o, EventArgs eventArgs) => {
                Console.WriteLine(i++);
                image = (Bitmap)Image.FromFile(IMAGE_PATH);
                ImageProcessing.ProcessImage(image);
                f.Refresh();
            });
            timer.Interval = 1000 / 15;
            timer.Start();
#else
            MjpegDecoder mjpeg = new MjpegDecoder();
            mjpeg.FrameReady += (s, e) => {
                stopwatch.Restart();

                image = e.Bitmap;
                image.Save(savePath + "img" + ++i + ".jpg"); //save image for future debugging

                ImageProcessing2014.Goal goal = ImageProcessing.ProcessImage(image);
                f.Refresh();

                image.Dispose();

                //Networking.SendData(goal.Distance, goal.Yaw, goal.isHot);
            };
            mjpeg.Error += (s, e) => {
                Console.WriteLine("Mjpeg error " + e.ErrorCode + ": " + e.Message);
            };

            
            mjpeg.ParseStream(new Uri(MJPG_PATH));

            //System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            //timer.Tick += new EventHandler((Object o, EventArgs eventArgs) => {
            //    if(stopwatch.ElapsedMilliseconds>1000) {
            //        Console.WriteLine("Mjpeg decoder unresponsive for " + stopwatch.ElapsedMilliseconds + "ms. Restarting decoder...");
            //        mjpeg.StopStream();
            //        mjpeg.ParseStream(new Uri(MJPG_PATH));
            //    }
            //});
            //timer.Interval = 1000;
            //timer.Start();
#endif



            



            Application.Run(f);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}