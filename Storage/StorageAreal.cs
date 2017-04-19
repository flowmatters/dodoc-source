using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowMatters.Source.DODOC.Core;
using RiverSystem.Nodes;

namespace FlowMatters.Source.DODOC.Storage
{
    class StorageAreal:Areal
    {
        public StorageAreal(StorageNodeModel s,double elevation)
        {
            _storage = s;
            _minElevation = elevation;
            _disregardedArea = _storage.StoreGeometry.surfaceAreaForHeight(_minElevation);
            //_prevStorage = -1;
        }

        private StorageNodeModel _storage;
        private double _minElevation;
        private double _disregardedArea;
        //private double _prevStorage;
        //private double _prevArea;

        public double Area { get { return _storage.SurfaceArea - _disregardedArea; } }

        //public double PreviousArea
        //{
        //    get
        //    {
        //        double currPrevStorage = _storage.PreviousStorage;
        //        if (currPrevStorage != _prevStorage)
        //        {
        //            _prevStorage = currPrevStorage;
        //            double height = _storage.StoreGeometry.heightForVolume(_prevStorage);
        //            _prevArea = _storage.StoreGeometry.surfaceAreaForHeight(height);
        //        }
        //        return _prevArea - _disregardedArea;
        //    }
        //}

        public override bool Equals(object obj)
        {
            return base.Equals(obj) ||(obj is StorageAreal ? ((StorageAreal)obj)._storage.Equals(_storage):false);
        }

        public override int GetHashCode()
        {
            return _storage.GetHashCode();
        }
    }
}
