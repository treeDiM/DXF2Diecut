using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace treeDiM.DiecutLib
{
    public class ExporterSet
    {
        #region Exporter retrieving
        static public BaseExporter GetExporterFromExtension(string fileExt)
        {
            foreach (BaseExporter exporter in _exporters)
                if (exporter.CanExport(fileExt))
                    return exporter;
            return null;
        }
        #endregion

        #region Static data
        static private BaseExporter[] _exporters = {
                                        new ExporterCF2(),
                                        new ExporterAI()
                                    };
        #endregion
    }
}
