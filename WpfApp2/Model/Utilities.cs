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
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);


        public static void saveInstanceOnDisk(Dictionary<string, List<MessageItem>> dict)
        {
            string json = new JavaScriptSerializer().Serialize(dict);
            System.IO.File.WriteAllText(@"savedInstance.txt", json);
        }

        public static bool hasSavedInstance()
        {
            try {
                using (StreamReader r = new StreamReader("savedInstance.txt"));
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
    }
}
