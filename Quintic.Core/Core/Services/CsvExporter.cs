using Quintic.Wpf.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Quintic.Wpf.Core.Services
{
    public class CsvExporter
    {
        public static void Export(string filePath, CalculationResponse data, IEnumerable<CamTrack> tracks = null)
        {
            if (data == null || data.Points == null)
            {
                throw new ArgumentNullException(nameof(data), "Cannot export null data.");
            }

            var sb = new StringBuilder();

            // Header
            sb.Append("Theta (deg),S (Pos),V (Vel),A (Acc),J (Jerk)");
            
            var trackList = tracks?.ToList() ?? new List<CamTrack>();
            foreach (var track in trackList)
            {
                sb.Append($",{track.Name.Replace(",", "")} (CH{track.ChannelIndex})");
            }
            sb.AppendLine();

            foreach (var point in data.Points)
            {
                // Format with consistent decimal places (e.g., 6) for precision
                var line = string.Format(CultureInfo.InvariantCulture,
                    "{0:F6},{1:F6},{2:F6},{3:F6},{4:F6}",
                    point.Theta,
                    point.S,
                    point.V,
                    point.A,
                    point.J);
                
                sb.Append(line);

                foreach (var track in trackList)
                {
                    bool isOn = false;
                    if (track.Switches != null)
                    {
                        foreach (var sw in track.Switches)
                        {
                            if (point.Theta >= sw.OnAngle && point.Theta <= sw.OffAngle)
                            {
                                isOn = true;
                                break;
                            }
                        }
                    }
                    sb.Append(isOn ? ",1" : ",0");
                }
                
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
