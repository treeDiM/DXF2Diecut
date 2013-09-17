#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
// command line parsing
using NDesk.Options;
// dxf loading
using netDxf;
// exporters
using treeDiM.DiecutLib;
#endregion

namespace DXF2Diecut
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFileName = string.Empty;
            string outputFileName = string.Empty;
            bool verbose = false;
            bool show_help = true;

            #region Do command line parsing
            var p = new OptionSet()
            {
            { "i|input=", "the {INPUT} file name to convert.",
               v => inputFileName = v },
            { "o|output=", "the {OUTPUT} file name",
               v => outputFileName = v },
            { "v|verbose", "increase debug message verbosity",
               v => verbose = v != null },
            { "h|help",  "syntax : DXF2Diecut --i input.dxf --o output.(cf2,ai)", 
               v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("DXF2Diecut: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `DXF2Diecut --help' for more information.");
                return;
            }
            #endregion

            // shows input commands
            if (verbose)
            {
                Console.WriteLine("Input  : {0}", inputFileName);
                Console.WriteLine("Output : {0}", outputFileName);
            }

            // exit if no valid input file
            if (!File.Exists(inputFileName))
            {
                Console.WriteLine("Input file {0} does not exists! Exiting...", inputFileName);
                return;
            }
            // check if output file already exist
            if (File.Exists(outputFileName))
            {
                if (verbose)
                    Console.WriteLine("Output file {1} already exists! Deleting...");
                try { File.Delete(outputFileName); }
                catch (Exception /*ex*/)
                {
                    Console.WriteLine("Failed to delete file {0}", outputFileName);
                    return;
                }
            }
            try
            {                
                // open dxf document
                DxfDocument dxf = DxfDocument.Load(inputFileName);
                if (null == dxf)
                {
                    Console.WriteLine("Failed to load dxf document!");
                    return;
                }
                if (verbose)
                    Console.WriteLine("FILE VERSION: {0}", dxf.DrawingVariables.AcadVer);
                
                // ### find exporter
                BaseExporter exporter = ExporterSet.GetExporterFromExtension(Path.GetExtension(outputFileName));
                if (null == exporter)
                {
                    Console.WriteLine("Failed to find valid exporter for file {0}", outputFileName);
                    return;
                }
                if (verbose)
                    Console.WriteLine("Now using exporter {0}", exporter.ToString());
                // ###
                // initialization
                exporter.Initialize();
                // set authoring tool
                exporter.AuthoringTool = "DXF2Diecut";
                // bounding box
                exporter.SetBoundingBox(0.0, 0.0, 100.0, 100.0);
                
                // create layers, pens (actually using layers)
                foreach (var o in dxf.Layers)
                    if (dxf.Layers.GetReferences(o).Count > 0)
                    {
                        exporter.CreateLayer(o.Name);
                        ExpPen pen = exporter.CreatePen(o.Name, string.Equals(o.Name, "20") ? ExpPen.ToolAttribute.LT_CREASING : ExpPen.ToolAttribute.LT_CUT);
                    }
                
                // create blocks
                foreach (var o in dxf.Blocks)
                {
                    if (verbose)
                        Console.WriteLine("Processing block : {0}", o.Name);
                }
                // entities
                foreach (netDxf.Entities.Line line in dxf.Lines)
                {
                    ExpLayer layer = exporter.GetLayerByName(line.Layer.Name);
                    ExpPen pen = exporter.GetPenByName(line.Layer.Name);
                    exporter.AddSegment(exporter.GetBlock("default"), layer, pen
                        , line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
                }
                foreach (netDxf.Entities.Arc arc in dxf.Arcs)
                {
                    ExpLayer layer = exporter.GetLayerByName(arc.Layer.Name);
                    ExpPen pen = exporter.GetPenByName(arc.Layer.Name);
                    exporter.AddArc(exporter.GetBlock("default"), layer, pen
                        , arc.Center.X, arc.Center.Y, arc.Radius, arc.StartAngle, arc.EndAngle);
                }
                // saving
                if (verbose) Console.WriteLine("Saving as {0}", outputFileName);
                exporter.Save(outputFileName);
                // done!
                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }
    }
}
