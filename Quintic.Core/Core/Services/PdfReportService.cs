using System;
using System.IO;
using System.Windows.Media.Imaging;
using OxyPlot;
using OxyPlot.Wpf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Quintic.Wpf.ViewModels;

namespace Quintic.Wpf.Core.Services
{
    public class PdfReportService
    {
        public static void GenerateReport(string filePath, KinematicAnalysisViewModel analysisVm, CamPlotViewModel plotVm)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var sPlotImage = ExportPlotToByteArray(plotVm.SPlotModel, 800, 300);
            var vajPlotImage = ExportPlotToByteArray(plotVm.VAPlotModel, 800, 400);
            var tnPlotImage = ExportPlotToByteArray(analysisVm.TorqueSpeedModel, 800, 400);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, analysisVm, sPlotImage, vajPlotImage, tnPlotImage));
                    page.Footer().Element(ComposeFooter);
                });
            })
            .GeneratePdf(filePath);
        }

        private static byte[] ExportPlotToByteArray(PlotModel model, int width, int height)
        {
            if (model == null) return new byte[0];
            
            var exporter = new PngExporter { Width = width, Height = height };
            
            var oldBackground = model.Background;
            var oldTextColor = model.TextColor;
            var oldTitleColor = model.TitleColor;
            
            model.Background = OxyColors.White;
            model.TextColor = OxyColors.Black;
            model.TitleColor = OxyColors.Black;
            
            foreach(var axis in model.Axes)
            {
                axis.TextColor = OxyColors.Black;
                axis.TicklineColor = OxyColors.Black;
                if (axis.MajorGridlineColor.A > 0)
                    axis.MajorGridlineColor = OxyColor.FromAColor(50, OxyColors.Black);
            }
            
            model.InvalidatePlot(false);

            var bitmap = exporter.ExportToBitmap(model);
            byte[] result = new byte[0];

            if (bitmap != null)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    result = stream.ToArray();
                }
            }

            model.Background = oldBackground;
            model.TextColor = oldTextColor;
            model.TitleColor = oldTitleColor;
            foreach(var axis in model.Axes)
            {
                axis.TextColor = oldTextColor;
                axis.TicklineColor = OxyColor.Parse("#444444");
                if (axis.MajorGridlineColor.A > 0)
                    axis.MajorGridlineColor = OxyColor.Parse("#222222");
            }
            model.InvalidatePlot(false);

            return result;
        }

        private static void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Quintic Cam Editor").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Kinematic Analysis & Crash Prevention Report").FontSize(14).FontColor(Colors.Grey.Darken2);
                    column.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10);
                });
            });
        }

        private static void ComposeContent(IContainer container, KinematicAnalysisViewModel vm, byte[] sPlot, byte[] vajPlot, byte[] tnPlot)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeParameters(c, vm));
                    row.ConstantItem(20);
                    row.RelativeItem().Element(c => ComposeKPIs(c, vm));
                });

                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                column.Item().Element(c => ComposeDiagnostics(c, vm));

                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                column.Item().Text("Torque-Speed Characteristic (T-N Curve)").FontSize(14).SemiBold();
                if (tnPlot.Length > 0)
                    column.Item().PaddingVertical(5).Image(tnPlot);

                column.Item().PageBreak();

                column.Item().Text("Displacement Profile").FontSize(14).SemiBold();
                if (sPlot.Length > 0)
                    column.Item().PaddingVertical(5).Image(sPlot);

                column.Item().PaddingVertical(10);
                
                column.Item().Text("Derivatives & Physics Profile").FontSize(14).SemiBold();
                if (vajPlot.Length > 0)
                    column.Item().PaddingVertical(5).Image(vajPlot);
            });
        }

        private static void ComposeParameters(IContainer container, KinematicAnalysisViewModel vm)
        {
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).Text("Physical Parameters").FontSize(12).SemiBold();
                
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Cell().Text("Load Inertia:");
                    table.Cell().Text($"{vm.Config?.LoadInertia:F4} kg·m²");

                    table.Cell().Text("Motor Inertia:");
                    table.Cell().Text($"{vm.Config?.MotorInertia:F4} kg·m²");

                    table.Cell().Text("Friction Torque:");
                    table.Cell().Text($"{vm.Config?.FrictionTorque:F2} Nm");

                    table.Cell().Text("Rated Torque (S1):");
                    table.Cell().Text($"{vm.RatedTorque:F2} Nm");

                    table.Cell().Text("Max Torque (S3):");
                    table.Cell().Text($"{vm.MaxTorque:F2} Nm");
                });
            });
        }

        private static void ComposeKPIs(IContainer container, KinematicAnalysisViewModel vm)
        {
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).Text("Key Performance Indicators").FontSize(12).SemiBold();
                
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Cell().Text("RMS Acceleration:");
                    table.Cell().Text($"{vm.RmsAcceleration:F2}").SemiBold();

                    table.Cell().Text("Peak Jerk:");
                    table.Cell().Text($"{vm.PeakJerk:F2}").SemiBold();

                    table.Cell().Text("RMS Torque:");
                    table.Cell().Text($"{vm.RmsTorque:F2} Nm").SemiBold().FontColor(Colors.Green.Darken2);

                    table.Cell().Text("Peak Torque:");
                    table.Cell().Text($"{vm.PeakTorque:F2} Nm").SemiBold().FontColor(Colors.Orange.Darken2);

                    table.Cell().Text("Peak Power:");
                    table.Cell().Text($"{vm.PeakPower:F2} W").SemiBold();
                });
            });
        }

        private static void ComposeDiagnostics(IContainer container, KinematicAnalysisViewModel vm)
        {
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).Text("Health & Diagnostics").FontSize(12).SemiBold();

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"RMS Load: {vm.RmsLoadPercentage:F0}%");
                        var rmsColor = vm.RmsLoadPercentage > 100 ? Colors.Red.Medium : (vm.RmsLoadPercentage > 80 ? Colors.Orange.Medium : Colors.Green.Medium);
                        c.Item().Height(10).Background(Colors.Grey.Lighten3).Row(r => 
                        {
                            r.RelativeItem((float)Math.Max(Math.Min(vm.RmsLoadPercentage, 100), 0.1)).Background(rmsColor);
                            r.RelativeItem((float)Math.Max(100 - vm.RmsLoadPercentage, 0));
                        });
                    });
                    
                    row.ConstantItem(20);

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Peak Load: {vm.PeakLoadPercentage:F0}%");
                        var peakColor = vm.PeakLoadPercentage > 100 ? Colors.Red.Medium : (vm.PeakLoadPercentage > 90 ? Colors.Orange.Medium : Colors.Green.Medium);
                        c.Item().Height(10).Background(Colors.Grey.Lighten3).Row(r => 
                        {
                            r.RelativeItem((float)Math.Max(Math.Min(vm.PeakLoadPercentage, 100), 0.1)).Background(peakColor);
                            r.RelativeItem((float)Math.Max(100 - vm.PeakLoadPercentage, 0));
                        });
                    });
                });

                column.Item().PaddingTop(10).Background(Colors.Grey.Darken4).Padding(10)
                      .Text(string.IsNullOrEmpty(vm.DiagnosticsLog) ? "System Healthy" : vm.DiagnosticsLog)
                      .FontFamily(Fonts.CourierNew).FontColor(Colors.White);
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        }
    }
}