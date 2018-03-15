using System.Linq;
using FlowMatters.Source.DODOC.Core;
using RiverSystem;
using RiverSystem.Api.Utils;
using RiverSystem.Attributes;
using RiverSystem.Constituents;
using TIME.Core;
using TIME.Core.Metadata;
using TIME.Science.Mathematics.Functions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FlowMatters.Source.DODOC.Instream
{
    public class InstreamDOC : ProxyLinkSourceSinkModel
    {
        public InstreamDOC()
        {
            InitialLeafDryMatterNonReadilyDegradable = new LinearPerPartFunction();
            InitialLeafDryMatterReadilyDegradable = new LinearPerPartFunction();
            LeafA = new LinearPerPartFunction();
        }

        // WHEN ADDING PROPERTIES, REMEMBER TO CLONE!
        [Parameter, Aka("Max Accumulation Area")]
        [CalculationUnits(CommonUnits.squareMetres)]
        public double MaxAccumulationArea { get; set; }

        [Parameter, Aka("Leaf Accumulation Constant")]
        public double LeafAccumulationConstant { get; set; }

        [Parameter, Aka("Reaeration Coefficient")]
        public double ReaerationCoefficient { get; set; }

        [Parameter, Aka("Doc Comsumption Coefficient")]
        public double DocConsumptionCoefficient { get; set; }

        [Parameter, Aka("Leaf A"), LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Leaf Accumulation", CommonUnits.none, CommonUnits.none)]
        public LinearPerPartFunction LeafA { get; set; }

        [Parameter, Aka("Leaf K1")]
        public double LeafK1 { get; set; }

        [Parameter, Aka("Leaf K2")]
        public double LeafK2 { get; set; }


        [Parameter, Aka("Primary Production Reaeration")]
        public double PrimaryProductionReaeration { get; set; }

        [Parameter, Aka("Temperature Obs")]
        public double TemperatureObs { get; set; }

        [Parameter]
        [LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres,
            "Initial Leaf dry matter non readily degradable", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction InitialLeafDryMatterNonReadilyDegradable { get; set; }

        [Parameter]
        [LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres,
            "Initial Leaf dry matter readily degradable", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction InitialLeafDryMatterReadilyDegradable { get; set; }

        public double[] tempX { get; set; }
        public double[] DOC_max { get; set; }
        public double[] DOC_k { get; set; }
        public double[] ProductionCoefficients { get; set; }
        public double[] ProductionBreaks { get; set; }

        [Output]
        public double ZoneCount { get; private set; }

        [Output]
        public int CountInundatedZones { get; private set; }

        [Output]
        public int CountDryZones { get; private set; }

        [Output]
        public double LeafDryMatterReadilyDegradable { get; private set; }

        [Output]
        public double LeafDryMatterNonReadilyDegradable { get; private set; }

        [Output]
        public double TemperatureEst { get; private set; }

        [Output]
        public double SoilO2Kg { get; private set; }

        [Output]
        public double DoCo2 { get; private set; }

        [Output]
        public double Production { get; private set; }

        [Output]
        public double Reaeration { get; private set; }

        [Output]
        public double ConsumedDocMilligrams { get; private set; }

        [Output]
        public double DOCEnteringWater { get; private set; }

        [Output]
        public double TotalWetLeaf { get; private set; }

        [Output]
        public double LeafWetMatterReadilyDegradable { get; private set; }

        [Output]
        public double LeafWetMatterNonReadilyDegradable { get; private set; }

        [Output]
        public double FloodplainWetAreaHa { get; private set; }

        [Output]
        public double FloodplainDryAreaHa { get; private set; }

        [Output]
        public double Leach1 { get; private set; }

        [Output]
        public double LeachingRate { get; private set; }

        [Output]
        public double DocMax { get; private set; }
        
        [Parameter]
        public bool IsFloodplain { get; set; }

        [Output, Aka("Leaf Accumlation")]
        public double LeafAccumulation => LeafA.f(Worker.Elevation);


        public override LinkSourceSinkModel CloneForMultipleDivisions()
        {
            return new InstreamDOC
            {
                IsFloodplain = IsFloodplain,

                MaxAccumulationArea = MaxAccumulationArea,

                LeafAccumulationConstant = LeafAccumulationConstant,

                ReaerationCoefficient = ReaerationCoefficient,

                DocConsumptionCoefficient = DocConsumptionCoefficient,

                LeafA = LeafA,

                LeafK1 = LeafK1,

                LeafK2 = LeafK2,

                InitialLeafDryMatterReadilyDegradable = InitialLeafDryMatterReadilyDegradable.Clone(),
                InitialLeafDryMatterNonReadilyDegradable = InitialLeafDryMatterNonReadilyDegradable.Clone(),

                PrimaryProductionReaeration = PrimaryProductionReaeration,

                TemperatureObs = TemperatureObs,
                tempX = tempX?.ToArray(),
                DOC_max = DOC_max?.ToArray(),
                DOC_k = DOC_k?.ToArray(),
                ProductionCoefficients = ProductionCoefficients?.ToArray(),
                ProductionBreaks = ProductionBreaks?.ToArray()
            };
        }


        protected override void UpdateWorker(double constituentConcentration)
        {
            Worker.ConcentrationDoc = constituentConcentration;

            Worker.MaxAccumulationArea = MaxAccumulationArea;
            Worker.LeafAccumulationConstant = LeafAccumulationConstant;
            Worker.ReaerationCoefficient = ReaerationCoefficient;
            Worker.DocConsumptionCoefficient = DocConsumptionCoefficient;
            Worker.LeafA = LeafA;
            Worker.LeafK1 = LeafK1;
            Worker.LeafK2 = LeafK2;
            Worker.InitialLeafDryMatterReadilyDegradable = InitialLeafDryMatterReadilyDegradable;
            Worker.InitialLeafDryMatterNonReadilyDegradable = InitialLeafDryMatterNonReadilyDegradable;
            Worker.PrimaryProductionReaeration = PrimaryProductionReaeration;
            Worker.TemperatureObs = TemperatureObs;

            if (tempX == null)
            {
                tempX = Worker.tempX;
            }
            else
            {
                Worker.tempX = tempX;
            }

            if (DOC_max == null)
            {
                DOC_max = Worker.DOC_max;
            }
            else
            {
                Worker.DOC_max = DOC_max;
            }

            if (DOC_k == null)
            {
                DOC_k = Worker.DOC_k;
            }
            else
            {
                Worker.DOC_k = DOC_k;
            }

            if (ProductionCoefficients == null)
            {
                ProductionCoefficients = Worker.ProductionCoefficients;
            }
            else
            {
                Worker.ProductionCoefficients = ProductionCoefficients;
            }

            if (ProductionBreaks == null)
            {
                ProductionBreaks = Worker.ProductionBreaks;
            }
            else
            {
                Worker.ProductionBreaks = ProductionBreaks;
            }

            Worker.Fac = 1.0 / Link.Divisions.Count;
        }

        public override void SetUpLinkSourceSinkModel(IRiverReach link, Division division, Constituent constituent,
            SystemConstituentsConfiguration constituentsConfig)
        {
            base.SetUpLinkSourceSinkModel(link, division, constituent, constituentsConfig);
            // division gets set in the base method
            CentralSourceSinkModel.Instance.IsFloodPlain[new DivisionAreal(Division)] = IsFloodplain;
        }

        protected override void RetrieveResults()
        {
            ProcessedLoad = Worker.DissolvedOrganicCarbonLoad;
            ZoneCount = Worker.ZoneCount;
            CountInundatedZones = Worker.CountInundatedZones;
            CountDryZones = Worker.CountDryZones;
            LeafDryMatterReadilyDegradable = Worker.LeafDryMatterReadilyDegradable;
            LeafDryMatterNonReadilyDegradable = Worker.LeafDryMatterNonReadilyDegradable;
            TemperatureEst = Worker.TemperatureEst;
            SoilO2Kg = Worker.SoilO2Kg;
            DoCo2 = Worker.DoCo2;
            Production = Worker.Production;
            Reaeration = Worker.Reaeration;
            ConsumedDocMilligrams = Worker.ConsumedDocMilligrams;
            DOCEnteringWater = Worker.DOCEnteringWater;
            TotalWetLeaf = Worker.TotalWetLeaf;
            LeafWetMatterReadilyDegradable = Worker.LeafWetMatterReadilyDegradable;
            LeafWetMatterNonReadilyDegradable = Worker.LeafWetMatterNonReadilyDegradable;
            FloodplainWetAreaHa = Worker.FloodplainWetAreaHa;
            FloodplainDryAreaHa = Worker.FloodplainDryAreaHa;
            Leach1 = Worker.Leach1;
            LeachingRate = Worker.LeachingRate;
            DocMax = Worker.DocMax;
        }
    }
}