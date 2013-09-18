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
            // do not use static data to prevent reuse of the same instance
            BaseExporter[] exporters
                = {
                    new ExporterCF2(),
                    new ExporterAI()
                };

            foreach (BaseExporter exporter in exporters)
                if (exporter.CanExport(fileExt))
                    return exporter;
            return null;
        }
        #endregion
    }
}
