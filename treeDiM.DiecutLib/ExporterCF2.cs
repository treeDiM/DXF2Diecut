#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
#endregion

namespace treeDiM.DiecutLib
{
    public class ExporterCF2 : BaseExporter
    {
        public ExporterCF2()
        {
        }

        public override bool CanExport(string fileExt)
        {
            return string.Equals(fileExt, ".cf2", StringComparison.CurrentCultureIgnoreCase)
                || string.Equals(fileExt, ".cff2", StringComparison.CurrentCultureIgnoreCase);
        }

        public override void Initialize()
        {
            // pen tool to index
            penToolMap[ExpPen.ToolAttribute.LT_CUT] = 1;
            penToolMap[ExpPen.ToolAttribute.LT_CREASING] = 2;
            penToolMap[ExpPen.ToolAttribute.LT_PERFOCREASING] = 3;
            penToolMap[ExpPen.ToolAttribute.LT_HALFCUT] = 4;
            penToolMap[ExpPen.ToolAttribute.LT_CONSTRUCTION] = 43;
            penToolMap[ExpPen.ToolAttribute.LT_COTATION] = 46;
            // create default block
            ExpBlock block = CreateBlock("default");
            // create default block ref
            ExpBlockRef blockRef = CreateBlockRef(block);
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
            return "ExporterCF2";
        }

        #region cf2 specific
        private string SaveToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("$BOF");
            sb.AppendLine("V2");
            sb.AppendLine("ORDER");
            sb.AppendLine(string.Format("%1 : {0}", AuthoringTool));
            sb.AppendLine("END");
            // ### MAIN : beg
            sb.AppendLine(string.Format("MAIN, {0}", AuthoringTool));
            sb.AppendLine("UM");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "LL,{0},{1}", _xmin, _ymin));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "UR,{0},{1}", _xmax, _ymax));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "SCALE,{0},{1}", 1.0, 1.0));
            foreach (ExpBlockRef blockRef in _blockRefs)
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "C,MODEL{0},{1},{2},{3},{4},{5}"
                    , blockRef._block._id, blockRef._x, blockRef._y, blockRef._dir, blockRef._scaleX, blockRef._scaleY));
            sb.AppendLine("END");
            // ### MAIN : end
            foreach (ExpBlock block in _blocks)
            {
                sb.AppendLine(string.Format("SUB,MODEL{0}", block.Id));
                foreach (ExpEntity entity in _entities)
                    if (entity.BelongsBlock(block))
                    {
                        int tool = PenToTool(entity.Pen);
                        int pt = PenToPt(entity.Pen);
                        ExpLine seg = entity as ExpLine;
                        if (null != seg)
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "L,{0},{1},0,{2},{3},{4},{5},0,0.0"
                                , pt, tool, seg.X0, seg.Y0, seg.X1, seg.Y1));
                        ExpArc arc = entity as ExpArc;
                        if (null != arc)
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "A,{0},{1},0,{2},{3},{4},{5},{6},{7},1,0,0.0"
                                , pt, tool, arc.Xbeg, arc.Ybeg, arc.Xend, arc.Yend, arc.Xcenter, arc.Ycenter));
                    };
                sb.AppendLine("END");
            }
            return sb.ToString();
        }

        private int PenToTool(ExpPen pen)
        {
            if (penToolMap.ContainsKey(pen.Attribute))
                return penToolMap[pen.Attribute];
            else
                return penToolMap[ExpPen.ToolAttribute.LT_CONSTRUCTION];
        }
        private int PenToPt(ExpPen pen)
        {
            return 1;
        }
        #endregion

        #region Data members
        private Dictionary<ExpPen.ToolAttribute, int> penToolMap = new Dictionary<ExpPen.ToolAttribute, int>();
        #endregion
    }
}
