using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Quintic.Wpf.Core.Models;

namespace Quintic.Wpf.Core.Services
{
    public static class StCodeGenerator
    {
        public static void Export(string filePath, CalculationResponse data, IEnumerable<CamTrack> tracks = null)
        {
            if (data == null || data.Points == null || data.Points.Count == 0) return;

            var sb = new StringBuilder();
            int count = data.Points.Count;

            sb.AppendLine("(* Quintic Cam Profile Export *)");
            sb.AppendLine($"(* Total Points: {count} *)");
            sb.AppendLine();

            // Master Position Array
            sb.Append("MasterPos : ARRAY[1..");
            sb.Append(count);
            sb.Append("] OF LREAL := [");
            
            for (int i = 0; i < count; i++)
            {
                sb.Append(data.Points[i].Theta.ToString("F4", CultureInfo.InvariantCulture));
                if (i < count - 1) sb.Append(", ");
            }
            sb.AppendLine("];");
            sb.AppendLine();

            // Slave Position Array
            sb.Append("SlavePos : ARRAY[1..");
            sb.Append(count);
            sb.Append("] OF LREAL := [");

            for (int i = 0; i < count; i++)
            {
                sb.Append(data.Points[i].S.ToString("F4", CultureInfo.InvariantCulture));
                if (i < count - 1) sb.Append(", ");
            }
            sb.AppendLine("];");

            // Logic Tracks (Boolean Arrays)
            if (tracks != null && tracks.Any())
            {
                sb.AppendLine();
                sb.AppendLine("(* Logic Tracks *)");

                foreach (var track in tracks)
                {
                    sb.Append($"Track_{track.ChannelIndex} : ARRAY[1..");
                    sb.Append(count);
                    sb.Append("] OF BOOL := [");

                    for (int i = 0; i < count; i++)
                    {
                        double theta = data.Points[i].Theta;
                        bool isOn = false;

                        if (track.Switches != null)
                        {
                            foreach (var sw in track.Switches)
                            {
                                if (theta >= sw.OnAngle && theta <= sw.OffAngle)
                                {
                                    isOn = true;
                                    break;
                                }
                            }
                        }

                        sb.Append(isOn ? "TRUE" : "FALSE");
                        if (i < count - 1) sb.Append(", ");
                    }
                    sb.AppendLine("];");
                }
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
