#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Threading;
#endregion

namespace treeDiM.DiecutLib
{
    #region Block
    public class ExpBlock
    {
        #region Constructor
        public ExpBlock(string name, uint id)
        {
            _name = name; _id = id;
        }
        #endregion
        #region Public properties
        public string Name { get { return _name; } }
        public uint Id { get { return _id; } }
        #endregion
        #region Data members
        public string _name;
        public uint _id;
        #endregion
    }
    #endregion

    #region BlockRef
    public class ExpBlockRef
    {
        #region Constructor
        public ExpBlockRef(ExpBlock block, double x, double y, double dir)
        {
            _block = block;
            _dir = 0.0;
            _x = x;
            _y = y;
            _dir = dir;
            _scaleX = 1.0;
            _scaleY = 1.0;
        }
        #endregion
        #region Data members
        public ExpBlock _block;
        public double _x, _y, _dir, _scaleX, _scaleY;
        #endregion
    }
    #endregion

    #region Pen
    public class ExpPen
    {
        #region Enums
        public enum ToolAttribute
        {
            LT_CUT,
            LT_CREASING,
            LT_PERFOCREASING,
            LT_HALFCUT,
            LT_CONSTRUCTION,
            LT_COTATION
        };
        #endregion
        #region Constructor
        public ExpPen(uint id, string name, ToolAttribute attribute)
        {
            _id = id; _name = name;
            Attribute = attribute;
       }
        #endregion
        #region Public properties
        public uint Id
        {
            get { return _id; }
        }
        public string Name
        {
            get { return _name; }
        }
        public string ColorString_CMYK
        {
            get
            {
                double RP = (double)colorDesc[0] / 255.0;
                double GP = (double)colorDesc[1] / 255.0;
                double BP = (double)colorDesc[2] / 255.0;
                double K = 1.0 - Math.Max(Math.Max(RP, GP), Math.Max(GP, BP));

                return string.Format("{0:0.####} {1:0.####} {2:0.####} {3:0.####}", (1-RP-K)/(1-K), (1-GP-K)/(1-K), (1-BP-K)/(1-K), K); 
            }
        }
        public string ColorString_RGB
        {
            get { return string.Format("{0} {1} {2}", colorDesc[0], colorDesc[1], colorDesc[2]); }
        }
        public ToolAttribute Attribute
        {
            get { return _toolAttribute; }
            set
            {
                _toolAttribute = value;
                Color penColor = Color.White;

                switch (_toolAttribute)
                {
                    case ToolAttribute.LT_CUT: penColor = Color.Red; break;
                    case ToolAttribute.LT_CREASING: penColor = Color.Blue; break;
                    case ToolAttribute.LT_PERFOCREASING: penColor = Color.Blue; break;
                    case ToolAttribute.LT_HALFCUT: penColor = Color.Red; break;
                    case ToolAttribute.LT_COTATION: penColor = Color.Green; break;
                    case ToolAttribute.LT_CONSTRUCTION: penColor = Color.Pink; break;
                    default: break;
                }

                colorDesc[0] = (int)penColor.R;
                colorDesc[1] = (int)penColor.G;
                colorDesc[2] = (int)penColor.B;
            }
        }
        #endregion
        #region Data members
        public string _name;
        public uint _id;
        public int[] colorDesc= new int[3];
        public ToolAttribute _toolAttribute;
        #endregion
    }
    #endregion

    #region Layer
    public class ExpLayer
    {
        #region Constructor
        public ExpLayer(string name, uint id)
        {
            _name = name;
            _id = id;
        }
        #endregion
        #region Public properties
        public string Name
        { get { return _name; } }
        #endregion
        #region Data members
        public string _name;
        public uint _id;
        #endregion
    }
    #endregion

    #region Entities
    public abstract class ExpEntity
    {
        #region Enums
        public enum ExpType
        {
            EXP_SEG
            , EXP_ARC
            , EXP_NURB
            , EXP_BEZIER
        }
        #endregion
        #region Constructor
        public ExpEntity(ExpBlock block, ExpLayer layer, ExpPen pen)
        {
            _block = block; _layer = layer; _pen = pen;
        }
        #endregion
        #region Public properties
        public ExpPen Pen { get { return _pen; } }
        public bool Sorted { get { return _sorted; } set { _sorted = value; } }
        public bool Swaped { get { return _swapped; } set { _swapped = value; } }
        #endregion
        #region Distance
        public double Distance(double xLatest, double yLatest, out int index)
        {
            double xx0 = X(0)-xLatest;
            double yy0 = Y(0) - yLatest;
            double d0 = xx0 * xx0 + yy0 * yy0;
            double xx1 = X(1) - xLatest;
            double yy1 = Y(1) - yLatest;
            double d1 = xx1 * xx1 + yy1 * yy1;
            if (d0 < d1)
            {
                index = 0;
                return d0;
            }
            else
            {
                index = 1;
                return d1;
            }            
        }
        #endregion
        #region Abstract properties / methods
        public abstract double X(int i);
        public abstract double Y(int i);
        public abstract double XMin { get; }
        public abstract double XMax { get; }
        public abstract double YMin { get; }
        public abstract double YMax { get; }
        #endregion
        #region Block/Layer entity
        public bool BelongsBlock(ExpBlock block)
        {
            return _block == block;
        }
        public bool BelongsLayer(ExpLayer layer)
        {
            return _layer == layer;
        }
        #endregion
        #region Data members
        public ExpBlock _block;
        public ExpLayer _layer;
        private ExpPen _pen;
        /// <summary>
        /// This parameter is only used when sorting entities
        /// </summary>
        private bool _sorted = false;
        /// <summary>
        /// entity can be swapped when sorting
        /// when swapped, an entity is traveled from point 1 to point 0
        /// </summary>
        private bool _swapped = false;
        #endregion
    }
    public class ExpLine : ExpEntity
    {
        #region Constructor
        public ExpLine(ExpBlock block, ExpLayer layer, ExpPen pen, double x0, double y0, double x1, double y1)
            : base(block, layer, pen)
        {
            _x0 = x0; _y0 = y0; _x1 = x1; _y1 = y1;
        }
        #endregion
        #region ExpEntity override
        public override double X(int i) { if (0 == i) return _x0; else return _x1; }
        public override double Y(int i) { if (0 == i) return _y0; else return _y1; }
        public override double XMin { get { return Math.Min(_x0, _x1); } }
        public override double XMax { get { return Math.Max(_x0, _x1); } }
        public override double YMin { get { return Math.Min(_y0, _y1); } }
        public override double YMax { get { return Math.Max(_y0, _y1); } }
        #endregion
        #region Public properties
        public double X0 { get { return _x0; } }
        public double Y0 { get { return _y0; } }
        public double X1 { get { return _x1; } }
        public double Y1 { get { return _y1; } }
        #endregion
        #region Data members
        private double _x0, _y0, _x1, _y1;
        #endregion
    }

    public class ExpArc : ExpEntity
    {
        #region Constructor
        public ExpArc(ExpBlock block, ExpLayer layer, ExpPen pen, double xc, double yc, double radius, double angle0, double angle1)
            : base(block, layer, pen)
        {
            _xc = xc; _yc = yc; _radius=radius; _angle0 = angle0; _angle1 = angle1;
        }
        #endregion
        #region ExpEntity override
        public override double X(int i) { if (0 == i) return Xbeg; else return Xend; }
        public override double Y(int i) { if (0 == i) return Ybeg; else return Yend; }
        public override double XMin { get { return Math.Min(Xbeg, Xend); } }
        public override double XMax { get { return Math.Max(Xbeg, Xend); } }
        public override double YMin { get { return Math.Min(Ybeg, Yend); } }
        public override double YMax { get { return Math.Max(Ybeg, Yend); } }
        #endregion
        #region Public properties
        public double Xbeg
        { get { return _xc + _radius * Math.Cos((Math.PI/180.0) * _angle0); } }
        public double Ybeg
        { get { return _yc + _radius * Math.Sin((Math.PI/180.0) * _angle0); } }
        public double Xend
        { get { return _xc + _radius * Math.Cos((Math.PI / 180.0) * _angle1); } }
        public double Yend
        { get { return _yc + _radius * Math.Sin((Math.PI / 180.0) * _angle1); } }
        public double Xcenter
        { get { return _xc; } }
        public double Ycenter
        { get { return _yc; } }
        public double Radius
        { get { return _radius;} }
        public double Angle0
        { get { return _angle0; } }
        public double Angle1
        { get { return _angle1; } }
        public double OpeningAngle
        { get { return _angle1-_angle0; } }
        #endregion
        #region Data members
        private double _xc, _yc;
        private double _radius;
        private double _angle0, _angle1;
        #endregion
    }
    #endregion

    #region BaseExporter
    public abstract class BaseExporter : IDisposable
    {
        #region Abstract methods
        public abstract bool CanExport(string fileExt);
        public abstract void Initialize();
        public abstract void Close();
        public abstract void InternalSave(Stream stream);
        #endregion

        #region Pens
        public ExpPen CreatePen(string name, ExpPen.ToolAttribute attribute)
        {
            ExpPen pen = new ExpPen(_penInc++, name, attribute);
            _pens.Add(pen);
            return pen;
        }
        public ExpPen CreatePen(uint id, string name, ExpPen.ToolAttribute attribute)
        {
            ExpPen pen = new ExpPen(id, name, attribute);
            _pens.Add(pen);
            return pen;
        }
        public ExpPen GetPenByName(string name)
        {
            return _pens.Find(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
        #endregion

        #region Layers
        public ExpLayer CreateLayer(string name, uint id)
        {
            ExpLayer layer = new ExpLayer(name, id);
            _layers.Add(layer);
            return layer;
        }
        public ExpLayer CreateLayer(string name)
        {
            ExpLayer layer = new ExpLayer(name, _layerInc++);
            _layers.Add(layer);
            return layer;
        }
        public ExpLayer GetLayerByName(string name)
        { 
            return _layers.Find(l => string.Equals(l._name, name, StringComparison.CurrentCultureIgnoreCase));
        }
        #endregion

        #region Blocks
        public ExpBlock CreateBlock(string name)
        {
            ExpBlock block = new ExpBlock(name, ++_blockInc);
            _blocks.Add(block);
            return block;
        }
        public ExpBlock GetBlock(string name)
        { 
            return _blocks.Find(b => string.Equals(b._name, name, StringComparison.CurrentCultureIgnoreCase));
        }
        public ExpBlockRef CreateBlockRef(ExpBlock block)
        {
            ExpBlockRef blockRef = new ExpBlockRef(block, 0.0, 0.0, 0.0);
            _blockRefs.Add(blockRef);
            return blockRef;
        }
        #endregion

        #region Add entities
        public void AddSegment(ExpBlock block, ExpLayer layer, ExpPen pen, double x0, double y0, double x1, double y1)
        {
            _entities.Add(new ExpLine(block, layer, pen, x0, y0, x1, y1));
        }
        public void AddArc(ExpBlock block, ExpLayer layer, ExpPen pen, double xc, double yc, double radius, double angle0, double angle1)
        {
            _entities.Add(new ExpArc(block, layer, pen, xc, yc, radius, angle0, angle1));
        }
        #endregion

        #region Bounding Box / Sort
        private void ComputeBoundingBox()
        {
            _xmin = double.MaxValue;
            _ymin = double.MaxValue;
            _xmax = double.MinValue;
            _ymax = double.MinValue;
            foreach (ExpEntity entity in _entities)
            {
                _xmin = Math.Min(_xmin, entity.XMin);
                _xmax = Math.Max(_xmax, entity.XMax);
                _ymin = Math.Min(_ymin, entity.YMin);
                _ymax = Math.Max(_ymax, entity.YMax);
            }
        }
        private void SortEntities()
        {
            ExpPen.ToolAttribute[] orderedAttributes = {
                ExpPen.ToolAttribute.LT_CUT,
                ExpPen.ToolAttribute.LT_CREASING,
                ExpPen.ToolAttribute.LT_PERFOCREASING,
                ExpPen.ToolAttribute.LT_HALFCUT,
                ExpPen.ToolAttribute.LT_CONSTRUCTION,
                ExpPen.ToolAttribute.LT_COTATION
                };
            // reorder entities
            List<ExpEntity> entitiesOut = new List<ExpEntity>();
            double xLatest = _xmin, yLatest = _ymin;
            // loop through all pen types
            foreach (ExpPen.ToolAttribute attribute in orderedAttributes)
            {
                bool found = true;
                // loop through all entities
                while (found)
                {
                    found = false;
                    double distanceToLatest = double.MaxValue;
                    ExpEntity nearEntity = null;
                    int index = 0, nearIndex = 0;
                    foreach (ExpEntity entity in _entities)
                    {
                        // only deal with current attribute, unsorted entities
                        if (attribute != entity.Pen.Attribute || entity.Sorted) continue;

                        // still some entity to be sorted
                        found = true;
                        // compute distance to latest point
                        double d = entity.Distance(xLatest, yLatest, out index);
                        if (d < distanceToLatest)
                        {
                            // save as smallest distance
                            distanceToLatest = d;
                            // save as nearest entity
                            nearEntity = entity;
                            // save as nearest point
                            nearIndex = index;
                        }
                    } // foreach
                    if (found)
                    {
                        nearEntity.Sorted = true;
                        nearEntity.Swaped = (nearIndex == 1);
                        entitiesOut.Add(nearEntity);
                        xLatest = nearEntity.X(nearIndex == 0 ? 1 : 0);
                        yLatest = nearEntity.Y(nearIndex == 0 ? 1 : 0);
                    }
                }
            }
            _entities = entitiesOut;
        }
        #endregion

        #region Save method
        /// <summary>
        /// Saves the database to a  file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>Return true if the file has been succesfully save, false otherwise.</returns>
        /// <exception cref="IOException"></exception>
        /// <remarks>
        /// If the file already exists it will be overwritten.<br />
        /// The Save method will still raise an exception if they are unable to create the FileStream.<br />
        /// On Debug mode they will raise any exception that migh occur during the whole process.
        /// </remarks>
        public bool Save(string file)
        {
            // set bounding box
            ComputeBoundingBox();
            // set name of file
            FileInfo fileInfo = new FileInfo(file);
            if (string.IsNullOrEmpty(Name))
                Name = Path.GetFileNameWithoutExtension(fileInfo.FullName);

            // In files, the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Stream stream;
            try
            {
                stream = File.Create(file);
            }
            catch (Exception ex)
            {
                throw new IOException("Error trying to create the file " + fileInfo.FullName + " for writing.", ex);
            }

            SortEntities();

#if DEBUG
            this.InternalSave(stream);
            stream.Close();
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            try
            {
                InternalSave(stream);
            }
            catch
            {
                return false;
            }
            finally
            {
                stream.Close();
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }                
#endif
            return true;
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Close();
        }
        #endregion

        #region Public properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string AuthoringTool
        {
            get { return _authoringTool; }
            set { _authoringTool = value; }
        }
        #endregion

        #region Public methods
        public void SetBoundingBox(double xmin, double ymin, double xmax, double ymax)
        {
            _xmin = xmin; _ymin = ymin; _xmax = xmax; _ymax = ymax;
        }
        #endregion

        #region Data members
        /// <summary>
        /// model name
        /// </summary>
        private string _name;
        /// <summary>
        /// authoring tool
        /// </summary>
        private string _authoringTool;
        /// <summary>
        /// bounding box
        /// </summary>
        public double _xmin = double.MaxValue, _ymin = double.MaxValue, _xmax = double.MinValue, _ymax = double.MinValue;
        /// <summary>
        /// increments
        /// </summary>
        private uint _penInc = 0, _layerInc = 0, _blockInc=0;
        /// <summary>
        /// Block references
        /// </summary>
        public List<ExpBlockRef> _blockRefs = new List<ExpBlockRef>();
        /// <summary>
        /// Blocks
        /// </summary>
        public List<ExpBlock> _blocks = new List<ExpBlock>();
        /// <summary>
        /// Pens
        /// </summary>
        public List<ExpPen> _pens = new List<ExpPen>();
        /// <summary>
        /// Layers
        /// </summary>
        public List<ExpLayer> _layers = new List<ExpLayer>();
        /// <summary>
        /// Entities
        /// </summary>
        public List<ExpEntity> _entities = new List<ExpEntity>();
        #endregion
    }
    #endregion
}
