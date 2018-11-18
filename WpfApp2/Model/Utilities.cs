using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Media.Imaging;
using System.Web.Script.Serialization;
using System.Windows;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace WpfApp2.Model
{

    static class Utilities
    {
        public static object JsonConvert { get; private set; }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        private static BitmapImage BitmapFromSource(BitmapSource bitmapsource)
        {
            Console.WriteLine("bitmapsource" + bitmapsource.ToString());
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        public static void saveInstanceOnDisk(Dictionary<string, List<MessageItem>> dict)
        {
            string json = new JavaScriptSerializer().Serialize(dict);
            System.IO.File.WriteAllText(@"savedInstance.txt", json);
        }

        public static bool hasSavedInstance()
        {
            try {
                using (StreamReader r = new StreamReader("savedInstance.txt")) ;
                return true;

            }catch (FileNotFoundException e)
            {
                return false;
            }
        }

        public static Dictionary<string, List<MessageItem>> retrieveInstanceFromDisk()
        {
            using (StreamReader r = new StreamReader("savedInstance.txt"))
            {
                string json = r.ReadToEnd();
                return  new JavaScriptSerializer().Deserialize< Dictionary<string, List<MessageItem>>>(json);
            }
        }
        public static string convertMessageItemToJSON(MessageItem messageItem)
        {
            return new JavaScriptSerializer().Serialize(messageItem);
        }

        public static MessageItem convertJSONToMessageItem(string json)
        {
            return new JavaScriptSerializer().Deserialize<MessageItem>(json);
        }

        public static string imageToBase64(Image image) { 
            using (MemoryStream m = new MemoryStream())
            {
                image.Save(m, image.RawFormat);
                byte[] imageBytes = m.ToArray();

            // Convert byte[] to Base64 String
            string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }


        public static String bytesToString(byte[] bytes)
        {
            StringBuilder msg = new StringBuilder();

            foreach (byte b in bytes)
            {
                if (b.Equals(00))
                {
                    break;
                }
                else
                {
                    msg.Append(Convert.ToChar(b).ToString());
                }
            }
            return msg.ToString();
        }

        public static BitmapSource GetImageStream(Image myImage)
        {
            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        public static Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        public static byte[] stringToBytes(String text)
        {
            return Encoding.ASCII.GetBytes(text);
        }


        public static bool IsValidImage(byte[] bytes)
        {
            try
            {
                Stream stream = new MemoryStream(bytes);
                var img = Bitmap.FromStream(stream);
                Console.WriteLine("valid image");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("not valid image");
                return false;
            }
            return true;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static byte[] ImageToByte(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }


        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }
    }
}
