using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Quintic.Wpf.Core.Services
{
    public class CsvExporter
    {
        public static void Export(string filePath, CalculationResponse data)
        {
            if (data == null || data.Points == null)
            {
                throw new ArgumentNullException(nameof(data), "Cannot export null data.");
            }

            var sb = new StringBuilder();

            // Header (VDI 2143 Standard)
            sb.AppendLine("Theta (deg),S (Pos),V (Vel),A (Acc),J (Jerk)");

            foreach (var point in data.Points)
            {
                // Format with consistent decimal places (e.g., 6) for precision
                // Use InvariantCulture to ensure dot decimal separator regardless of system locale
                var line = string.Format(CultureInfo.InvariantCulture,
                    "{0:F6},{1:F6},{2:F6},{3:F6},{4:F6}",
                    point.Theta,
                    point.S,
                    point.V,
                    point.A,
                    point.J);
                
                sb.AppendLine(line);
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
