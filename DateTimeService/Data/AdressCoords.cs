using System;
using System.Globalization;

namespace DateTimeService.Data
{
    public class AdressCoords
    {
        public AdressCoords()
        {
            X_coordinates = 0;
            Y_coordinates = 0;
            AvailableToUse = false;
        }

        public AdressCoords(string x_coordinates, string y_coordinates)
        {
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

            if (Double.TryParse(x_coordinates, NumberStyles.Float, formatter, out double xCoord) &&
                Double.TryParse(y_coordinates, NumberStyles.Float, formatter, out double yCoord))
            {
                X_coordinates = xCoord;
                Y_coordinates = yCoord;
                AvailableToUse = true;
            }
        }

        public AdressCoords(double x_coordinates, double y_coordinates)
        {
            X_coordinates = x_coordinates;
            Y_coordinates = y_coordinates;
        }

        public double X_coordinates { get; set; }
        public double Y_coordinates { get; set; }
        public Boolean AvailableToUse { get; set; }
    }
}
