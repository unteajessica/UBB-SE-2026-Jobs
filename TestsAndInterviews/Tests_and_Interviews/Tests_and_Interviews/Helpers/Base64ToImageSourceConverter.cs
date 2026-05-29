using System;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Tests_and_Interviews.Helpers
{
    public sealed class Base64ToImageSourceConverter : IValueConverter
    {
        /// <summary>
        /// Function that converts an image to a bitmapImage
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string base64 || string.IsNullOrWhiteSpace(base64))
            {
                return null;
            }

            byte[] bytes;
            try
            {
                bytes = System.Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }

            var bitmapImage = new BitmapImage();

            // Create an in-memory stream from the base64 bytes and load it into the Image.
            using (var memStream = new InMemoryRandomAccessStream())
            {
                memStream.WriteAsync(bytes.AsBuffer()).AsTask().GetAwaiter().GetResult();
                memStream.Seek(0);
                bitmapImage.SetSource(memStream);
            }

            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotSupportedException();
    }
}

