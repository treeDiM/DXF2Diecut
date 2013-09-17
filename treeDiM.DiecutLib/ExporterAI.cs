#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Reflection;
#endregion

namespace treeDiM.DiecutLib
{
    class ExporterAI : BaseExporter
    {
        #region Constructor
        public ExporterAI()
        {
        }
        #endregion

        #region Override BaseExporter
        public override bool CanExport(string fileExt)
        {
            return string.Equals(fileExt, ".ai", StringComparison.CurrentCultureIgnoreCase);
        }
        public override void Initialize()
        {
        }
        public override void Close()
        {
        }
        public override byte[] GetResultByteArray()
        {
            string textOutput = SaveToString();
            byte[] byteArray = new byte[textOutput.Length];
            for (int i = 0; i < textOutput.Length; ++i)
                byteArray[i] = Convert.ToByte(textOutput[i]);
            return byteArray;
        }
        public override string ToString()
        {
            return "ExporterAI";
        }
        #endregion

        #region Adobe Illustrator (*.ai) Format specific
        private string SaveToString()
        {
            StringBuilder sb = new StringBuilder();

            const double INCH2MM = 72.0 / 25.4;
            double xminInch = _xmin * INCH2MM;
            double yminInch = _ymin * INCH2MM;
            double xmaxInch = _xmax * INCH2MM;
            double ymaxInch = _ymax * INCH2MM;
            double wMM = _xmax - _xmin;
            double hMM = _ymax - _ymin;
            double wInch = xmaxInch - xminInch;
            double hInch = ymaxInch - yminInch;
            int ix0 = (int)xminInch;
            int iy0 = (int)yminInch;

            sb.AppendLine("%!PS-Adobe-3.0 EPSF-3.0");
            sb.AppendLine(string.Format("%%Creator: {0}", AuthoringTool));
            sb.AppendLine(string.Format("%%Title: ({0})", Name));
            sb.AppendLine(string.Format("%%CreationDate:({0})", DateTime.Now.ToShortDateString()));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture
                , "%%BoundingBox:{0:0.##} {1:0.##} {2:0.##} {3:0.##}"
                , xminInch - 1.0, yminInch - 1.0, xmaxInch + 1.0, ymaxInch + 1.0));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture
                , "%%HiResBoundingBox:{0:0.##} {1:0.##} {2:0.##} {3:0.##}"
                , xminInch, yminInch, xmaxInch, ymaxInch));
            sb.AppendLine("%%DocumentProcessColors: Cyan Magenta Yellow Black");
            sb.AppendLine("%%DocumentNeededResources: procset Adobe_level2_AI5 1.2 0");
            sb.AppendLine("%%+ procset Adobe_blend_AI5 1.0 0");
            sb.AppendLine("%%+ procset Adobe_Illustrator_AI5 1.0 1");
            sb.AppendLine("%AI5_FileFormat 1.1");
            sb.AppendLine("%AI3_ColorUsage: Color");
            for (int i = 0; i < _pens.Count; ++i)
            {
                if (i == 0) sb.AppendLine(string.Format("%DocumentCustomColors: ({0})", _pens[i].Name));
                else sb.AppendLine(string.Format("%+ ({0})", _pens[i].Name));
            }
            for (int i = 0; i < _pens.Count; ++i)
            {
                if (i == 0) sb.AppendLine(string.Format("%CMYKCustomColor: {0} ({1})", _pens[i].ColorString_CMYK, _pens[i].Name));
                else sb.AppendLine(string.Format("%+ {0} ({1})", _pens[i].ColorString_CMYK, _pens[i].Name));
            }
            sb.AppendLine(string.Format("%AI3_TemplateBox:{0:0.###} {1:0.###} {2:0.###} {3:0.###}"
                , ix0 + (int)(wInch / 2.0)
                , iy0 + (int)(hInch / 2.0)
                , ix0 + (int)(wInch / 2.0)
                , iy0 + (int)(hInch / 2.0)));
            sb.AppendLine(string.Format("%AI5_ArtSize: {0} {1}", wInch, hInch));
            sb.AppendLine("%AI5_RulerUnits: 1");
            sb.AppendLine(string.Format("%AI5_NumLayers: {0}", _layers.Count));
            sb.AppendLine("%AI3_DocumentPreview: None");
            int zm = -4;
            if ((wInch < 1200.0) || (hInch < 1000.0)) zm = -2;
            if ((wInch < 600.0) || (hInch < 500.0)) zm = -1;
            if ((wInch > 4400.0) || (hInch > 3000.0)) zm = -8;
            sb.AppendLine(string.Format("%AI5_OpenToView: {0:0.##} {1:0.##} {2} 1242 702 26 1 0 19 79 0 0",
                xminInch - wInch / 5.0, yminInch + hInch / 5.0, zm));
            sb.AppendLine("%AI5_OpenViewLayers: 2222");
            sb.AppendLine("%%EndComments");
            sb.AppendLine("%%BeginProlog");
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            sb.AppendLine(LoadFileAsString(Path.Combine(assemblyFolder, "Resource_AI5.txt")));
            sb.AppendLine("%%EndProlog");
            sb.AppendLine("%%BeginSetup");
            sb.AppendLine("Adobe_level2_AI5 /initialize get exec");
            sb.AppendLine("Adobe_Illustrator_AI5_vars Adobe_Illustrator_AI5 Adobe_blend_AI5 /initialize get exec");
            sb.AppendLine("Adobe_Illustrator_AI5 /initialize get exec");
            sb.AppendLine("%AI5_Begin_NonPrinting");
            sb.AppendLine("Np");
            sb.AppendLine("%AI5_End_NonPrinting--");
            sb.AppendLine("%%EndSetup");

            // pens as layers
            int penIndex = 0;
            foreach (ExpPen pen in _pens)
            {
                // using CMYK color
                sb.AppendLine("%AI5_BeginLayer");
                sb.AppendLine(string.Format("1 1 1 1 0 0 -1 {0} Lb", _pens[penIndex].ColorString_RGB));
                sb.AppendLine(string.Format("({0}) Ln", pen.Name)); // Cut

                sb.AppendLine("0 A");
                sb.AppendLine("0 R");
                sb.AppendLine("u");     // u : démarre l’association,  U : fin de l’association

                foreach (ExpEntity entity in _entities)
                {
                    if (entity.Pen != pen) continue;
                    // initialize pen

                    int ep_line = (int)((wMM > hMM ? wMM : hMM) * 5.0 / 2000.0);
                    if (ep_line < 1) ep_line = 1;
                    if (ep_line > 10) ep_line = 10;

                    sb.AppendLine(string.Format("{0} ({1}) 0 X", pen.ColorString_CMYK, pen.Name)); // 0 1 1 0 K\n   CUT
                    sb.AppendLine("0 j");
                    sb.AppendLine("0 J");
                    sb.AppendLine(string.Format("{0} w", ep_line));

                    ExpLine line = entity as ExpLine;
                    if (null != line)
                    {
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0:0.###} {1:0.###} m", line.X0 * INCH2MM, line.Y0 * INCH2MM));
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0:0.###} {1:0.###} L", line.X1 * INCH2MM, line.Y1 * INCH2MM));
                        sb.AppendLine("S");
                         
                    }
                    ExpArc arc = entity as ExpArc;
                    if (null != arc)
                    {
                        // divide arc if necessary
                        int iStep = (int)Math.Ceiling(arc.OpeningAngle / 91.0);

                        iStep = iStep >= 1 ? iStep : 1;

                        const double rad = Math.PI / 180.0;
                        const double paga = 1.0;
                        double angleStep = arc.OpeningAngle / iStep;
                        double ang1 = arc.Angle0;
                        double xc = arc.Xcenter;
                        double yc = arc.Ycenter;
                        double dim = arc.Radius;
                        double dir = arc.Angle0;

                        for (int i = 0; i < iStep; ++i)
                        {
                            double ang2 = ang1 + angleStep;
                            // control points of elipse arc in ellipse local coord
                            double x1 = arc.Radius * Math.Cos((ang1 - dir) * rad);
                            double y1f = arc.Radius * paga * Math.Sin((ang1 - dir) * rad);
                            double x4 = dim * Math.Cos((ang2 - dir) * rad);
                            double y4 = dim * paga * Math.Sin((ang2 - dir) * rad);
                            double dx1 = -dim * Math.Sin((ang1 - dir) * rad);
                            double dy1 = dim * paga * Math.Cos((ang1 - dir) * rad);
                            double dx4 = -dim * Math.Sin((ang2 - dir) * rad);
                            double dy4 = dim * paga * Math.Cos((ang2 - dir) * rad);
                            double alpha = Math.Sin((ang2 - ang1) * rad) * ((Math.Sqrt(4.0 + 3.0 * Math.Atan(0.5 * (ang2 - ang1) * rad) * Math.Atan(0.5 * (ang2 - ang1) * rad))) - 1.0f) / 3.0f;
                            double x2 = x1 + alpha * dx1;
                            double y2 = y1f + alpha * dy1;
                            double x3 = x4 - alpha * dx4;
                            double y3 = y4 - alpha * dy4;
                            // rotation
                            Rotation(ref x1, ref y1f, x1, y1f, Math.Cos(dir * rad), Math.Sin(dir * rad));
                            Rotation(ref x2, ref y2, x2, y2, Math.Cos(dir * rad), Math.Sin(dir * rad));
                            Rotation(ref x3, ref y3, x3, y3, Math.Cos(dir * rad), Math.Sin(dir * rad));
                            Rotation(ref x4, ref y4, x4, y4, Math.Cos(dir * rad), Math.Sin(dir * rad));
                            // translation
                            x1 += xc; y1f += yc;
                            x2 += xc; y2 += yc;
                            x3 += xc; y3 += yc;
                            x4 += xc; y4 += yc;
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture
                                , "{0:0.##} {1:0.##} m", x1 * INCH2MM, y1f * INCH2MM));
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture
                                , "{0:0.##} {1:0.##} {2:0.##} {3:0.##} {4:0.##} {5:0.##} c", x2 * INCH2MM, y2 * INCH2MM, x3 * INCH2MM, y3 * INCH2MM, x4 * INCH2MM, y4 * INCH2MM));
                            sb.AppendLine("S");
                        }
                    }
                }
                sb.AppendLine("U");
                sb.AppendLine("LB");
                sb.AppendLine("%AI5_EndLayer--"); // fin du Layer

                ++penIndex;
            }
            sb.AppendLine("%%PageTrailer");
            sb.AppendLine("gsave annotatepage grestore showpage");
            sb.AppendLine("%%Trailer");
            sb.AppendLine("Adobe_Illustrator_AI5 /terminate get exec");
            sb.AppendLine("Adobe_blend_AI5 /terminate get exec");
            sb.AppendLine("Adobe_level2_AI5 /terminate get exec");
            sb.AppendLine("%%EOF");

            return sb.ToString();
        }

        void Rotation(ref double x, ref double y, double x0, double y0, double cod, double sid)
        {
            x = x0 * cod - y0 * sid;
            y = y0 * cod + x0 * sid;
        }
        #endregion
    }
}
