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
            _disregardedArea = _storage.StoreGeometry.surfaceAreaForHeight(elevation);

            // Max area and Max elevation correspond to the highest defined point for the storage.
            // Note: At time of writing (02/08/2017) Storages in riversystem are not constrained by the geometry and can actually rise higher than the highes tdefined point.

            MaxArea = _storage.StoreGeometry.surfaceAreaForHeight(_storage.StoreGeometry.MaxHeight()) -
                      _disregardedArea;

            MaxElevation = _storage.StoreGeometry.MaxHeight();
        }
        

        private readonly IStorageModel _storage;
        private readonly double _disregardedArea;

        /// <summary>
        /// The current timestep the model is executing under
        /// </summary>
        public DateTime SimulationNow { get; set; }

        public double MaxArea { get; private set; }

        public double Area => Math.Max(0.0, _storage.SurfaceArea - _disregardedArea);

        /// <summary>
        /// The current elevation of the water level in the Storage 
        /// </summary>
        public double Elevation => _storage.Level;

        /// <summary>
        /// The highest elevation for the Storage. Corresponds with the <see cref="MaxArea"/>.
        /// </summary>
        public double MaxElevation { get; }


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
