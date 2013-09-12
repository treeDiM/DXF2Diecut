#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        public ExpPen(uint id, string name)
        {
            _id = id; _name = name;
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
        public ToolAttribute Attribute
        {
            get { return _toolAttribute; }
            set { _toolAttribute = value; }
        }
        #endregion
        #region Data members
        public string _name;
        public uint _id;
        public int[] colorDesc;
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
    public class ExpEntity
    {
        public enum ExpType
        {
            EXP_SEG
            , EXP_ARC
            , EXP_NURB
            , EXP_BEZIER
        }
        public ExpEntity(ExpBlock block, ExpLayer layer, ExpPen pen)
        {
            _block = block; _layer = layer; _pen = pen;
        }
        public bool BelongsBlock(ExpBlock block)
        {
            return _block == block;
        }
        public bool BelongsLayer(ExpLayer layer)
        {
            return _layer == layer;
        }

        #region Data members
        public ExpBlock _block;
        public ExpLayer _layer;
        public ExpPen _pen;
        #endregion
    }
    public class ExpSegment : ExpEntity
    {

        public ExpSegment(ExpBlock block, ExpLayer layer, ExpPen pen, double x0, double y0, double x1, double y1)
            : base(block, layer, pen)
        {
            _x0 = x0; _y0 = y0; _x1 = x1; _y1 = y1;
        }
        public double X0 { get { return _x0; } }
        public double Y0 { get { return _y0; } }
        public double X1 { get { return _x1; } }
        public double Y1 { get { return _y1; } }

        private double _x0, _y0, _x1, _y1;
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
        public ExpPen CreatePen(string name)
        {
            ExpPen pen = new ExpPen(_penInc++, name);
            _pens.Add(pen);
            return pen;
        }
        public ExpPen CreatePen(string name, uint id)
        {
            ExpPen pen = new ExpPen(id, name);
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
            _entities.Add(new ExpSegment(block, layer, pen, x0, y0, x1, y1));
        }
        public void AddArc(ExpBlock block, ExpLayer layer, ExpPen pen, double xc, double yc, double radius, double angle0, double angle1)
        {
            _entities.Add(new ExpArc(block, layer, pen, xc, yc, radius, angle0, angle1));
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
            FileInfo fileInfo = new FileInfo(file);
            this.name = Path.GetFileNameWithoutExtension(fileInfo.FullName);

            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
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
        private string name;
        private string _authoringTool;
        public double _xmin, _ymin, _xmax, _ymax;
        private uint _penInc = 0, _layerInc = 0, _blockInc=0;

        public List<ExpBlockRef> _blockRefs = new List<ExpBlockRef>();
        public List<ExpBlock> _blocks = new List<ExpBlock>();
        public List<ExpPen> _pens = new List<ExpPen>();
        public List<ExpLayer> _layers = new List<ExpLayer>();
        public List<ExpEntity> _entities = new List<ExpEntity>();
        #endregion
    }
    #endregion
}
