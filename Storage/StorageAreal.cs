using System;
using FlowMatters.Source.DODOC.Core;
using RiverSystem.Storages.Interfaces;

namespace FlowMatters.Source.DODOC.Storage
{
    class StorageAreal:IAreal
    {
        public StorageAreal(IStorageModel s,double elevation)
        {
            _storage = s;
            _minElevation = elevation;
            _disregardedArea = _storage.StoreGeometry.surfaceAreaForHeight(_minElevation);
            MaxArea = _storage.StoreGeometry.surfaceAreaForHeight(_storage.StoreGeometry.MaxHeight()) -
                      _disregardedArea;
        }

        private IStorageModel _storage;
        private double _minElevation;
        private double _disregardedArea;

        public double MaxArea { get; private set; }

        public double Area { get { return Math.Max(0.0,_storage.SurfaceArea - _disregardedArea); } }

     
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
