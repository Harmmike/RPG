using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFUI.CustomConverters
{
    public class FileToBitmapConverter : IValueConverter
    {
        //We make _locations static so that it caches the file and we don't need to re-read from disk each time we go to the location.
        //This creates a dictionary that uses a filename as a key, and a .bmp file as a value.
        private static readonly Dictionary<string, BitmapImage> _locations =
            new Dictionary<string, BitmapImage>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string filename)) //checks if we're able to cast the value as a string, if we can't we bail.
            {
                return null;
            }

            if (!_locations.ContainsKey(filename)) //If the dictionary does not contain the file, this adds it.
            {
                _locations.Add(filename,
                               new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}{filename}",
                                                       UriKind.Absolute)));
            }

            return _locations[filename];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) //We don't use this, so we can return null.
        {
            return null;
        }
    }
}
