//using RiverSystem;
//using TIME.Core.Metadata;

//namespace FlowMatters.Source.DODOC.Instream
//{
//    public class InstreamDOC : ProxyLinkSourceSinkModel
//    {
//        public InstreamDOC()
//        {
//        }

//        // WHEN ADDING PROPERTIES, REMEMBER TO CLONE!

//        public double DecompositionCoefficient { get; set; }
//        public double MaxAccumulationArea { get; set; }
//        public double Fac { get; set; }
//        public double LeafAccumulationConstant { get; set; }
//        public double ReaerationCoefficient { get; set; }
//        public double DocConsumptionCoefficient { get; set; }
//        public double LeafA { get; set; }
//        public double LeafK1 { get; set; }
//        public double LeafK2 { get; set; }

//        [Output]
//        public double NumberOfZones { get; private set; }

//        [Output]
//        public virtual int CountInundatedZones { get; private set; }

//        [Output]
//        public virtual int CountDryZones { get; private set; }

//        [Output]
//        public double LeafDryMatterReadilyDegradable { get; private set; }

//        [Output]
//        public double LeafDryMatterNonReadilyDegradable { get; private set; }

//        public override LinkSourceSinkModel CloneForMultipleDivisions()
//        {
//            return new InstreamDOC()
//            {
//                IsFloodplain = IsFloodplain,
//                DecompositionCoefficient = DecompositionCoefficient,
//                MaxAccumulationArea = MaxAccumulationArea,
//                Fac = Fac,
//                LeafAccumulationConstant = LeafAccumulationConstant,
//                ReaerationCoefficient = ReaerationCoefficient,
//                DocConsumptionCoefficient = DocConsumptionCoefficient,
//                LeafA = LeafA,
//                LeafK1 = LeafK1,
//                LeafK2 = LeafK2
//            };
//        }

//        protected override void UpdateWorker(double constituentConcentration)
//        {
//            Worker.ConcentrationDoc = constituentConcentration;
//            Worker.DecompositionCoefficient = DecompositionCoefficient;
//            Worker.Fac = Fac;
//            Worker.LeafAccumulationConstant = LeafAccumulationConstant;
//            Worker.ReaerationCoefficient = ReaerationCoefficient;
//            Worker.DocConsumptionCoefficient = DocConsumptionCoefficient;
//            Worker.LeafA = LeafA;
//            Worker.LeafK1 = LeafK1;
//            Worker.LeafK2 = LeafK2;
//            Worker.MaxAccumulationArea = MaxAccumulationArea;

//        }

//        protected override void RetrieveResults()
//        {
//            ProcessedLoad = Worker.DissolvedOrganicCarbonLoad;
//            NumberOfZones = Worker.ZoneCount;
//            CountDryZones = Worker.CountDryZones;
//            CountInundatedZones = Worker.CountInundatedZones;
//            LeafDryMatterNonReadilyDegradable = Worker.LeafDryMatterNonReadilyDegradable;
//            LeafDryMatterReadilyDegradable = Worker.LeafDryMatterReadilyDegradable;
//        }
//    }
//}
