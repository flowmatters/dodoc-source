namespace FlowMatters.Source.DODOC.Core
{
    public class FloodplainData
    {
        public FloodplainData(bool wet)
        {
            Wet = wet;
        }

        public const double M2_TO_HA = 1e-4;

        /// <summary>
        /// This corressponds to the surface area at this Zones Elevation, minus the area at the floodplain elevation (sometimes called the disregarded area).
        /// </summary>
        public double CumulativeAreaM2 { get; set; }

        /// <summary>
        /// The elevation of the Zone.
        /// </summary>
        public double ElevationM { get; set; }

        public double LeafDryMatterReadilyDegradable { get; set; } // mass/ha
        public double LeafDryMatterNonReadilyDegradable { get; set; } // mass/ha


        /// <summary>
        /// The difference between this zone and the Zone below it
        /// The logic in class <see cref="FloodplainDoDoc"/> is responsible for maintaining this
        /// </summary>
        public double ZoneAreaM2 { get; set; }

        public bool Wet { get; }
        public bool Dry { get { return !Wet; } }

        public double LeafAccumulation { get; set; }

        internal double DryMassKg(double byArea)
        {
            return Wet ? 0.0 : (ZoneAreaM2*M2_TO_HA)*byArea;
        }

        public double WetMassKg(double byArea)
        {
            return Dry ? 0.0 : (ZoneAreaM2 * M2_TO_HA) * byArea;
        }

        public override string ToString()
        {
            var status = Wet ? "Wet" : "Dry";
            return $"{CumulativeAreaM2} - {status}";
        }
    }
}