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
    class ExporterAI : BaseExporter
    {
        public ExporterAI()
        { 
        
        }
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

        public override void InternalSave(Stream stream)
        {
            TextWriter writer = new StreamWriter(stream, Encoding.Default);
            string textOutput = SaveToString();
            byte[] byteArray = new byte[textOutput.Length];
            for (int i = 0; i < textOutput.Length; ++i)
                byteArray[i] = Convert.ToByte(textOutput[i]);

            stream.Write(byteArray, 0, textOutput.Length);
        }

        public override string ToString()
        {
            return "ExporterAI";
        }

        #region cf2 specific
        private string SaveToString()
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }
        #endregion

    }
}
