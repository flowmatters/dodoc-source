using System;
using System.Linq;
using FlowMatters.Source.DODOC.Core;
using RiverSystem;
using RiverSystem.Flow;
using RiverSystem.ManagedExtensions;

namespace FlowMatters.Source.DODOC.Instream
{
    public class DivisionAreal : IAreal
    {
        public DivisionAreal(Division d)
        {
            _division = d;
        }

        private readonly Division _division;

        /// <summary>
        /// The current timestep the model is executing under
        /// </summary>
        public DateTime SimulationNow { get; set; }

        public double AreaForHeightLookup(double height)
        {
            var ratingCurve = _division.Link.RatingCurveLibrary.GetCurve(SimulationNow);

            //TODO This looks a little messy. We've taken the same Linear Interpolation Method used in Flow Routing for consistancy. For some reason the first two params are a List and an IList which seems inconsistant.
            var widthForHeight = AbstractLumpedFlowRouting.Lintrpl(
                ratingCurve.Levels.ToList(),
                ratingCurve.Widths,
                height,
                ratingCurve.Levels.Length);

            // Determine the area by multiplying the width by the length of a division.
            return widthForHeight * _division.Link.Length / _division.Link.NumberOfDivisions;
            
        }

        public double Area => _division.Area;

        /// <summary>
        /// The current elevation for the Division
        /// TODO - We believe the Storage Routing implementation of the DoDoc Model needs work. For the time being we are using the inflow into the division to determine the Elevation..
        /// </summary>
        public double Elevation
        {
            get
            {
                var ratingCurve = _division.Link.RatingCurveLibrary.GetCurve(SimulationNow);
                
                //TODO This looks a little messy. We've taken the same Linear Interpolation Method used in Flow Routing for consistancy. For some reason the first two params are a List and an IList which seems inconsistant.
                var depthForFlowRate = AbstractLumpedFlowRouting.Lintrpl(
                    ratingCurve.Discharges.ToList(), 
                    ratingCurve.Levels, 
                    _division.Inflow, 
                    ratingCurve.Levels.Length);

                return depthForFlowRate;
            }
        }

        /// <summary>
        /// The Maximimum elevation that can be reached in the Division
        /// TODO - We believe the Storage Routing implementation of the DoDoc Model needs work. For the time being we are taking the highest defined point in the current rating curve to represent the Max Elevation.
        /// </summary>
        public double MaxElevation
        {
            get
            {
                var ratingCurve = _division.Link.RatingCurveLibrary.GetCurve(SimulationNow);
                
                return ratingCurve.Levels.Last();
            }
        }

        /// <summary> 
        /// The maximum area of the division
        /// TODO - The use of Double.MaxValue here looks unrealistic and dangerous. 
        /// </summary>
        public double MaxArea => Double.MaxValue;
        

        public override bool Equals(object obj)
        {
            return base.Equals(obj) || (obj is DivisionAreal ? ((DivisionAreal) obj)._division.Equals(_division):false);
        }

        public override int GetHashCode()
        {
            return _division.GetHashCode();
        }
    }
}
