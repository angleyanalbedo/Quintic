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
        public static void Export(string filePath, CalculationResponse data)
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

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
