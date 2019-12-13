using System;
using System.Collections.Generic;
using System.Linq;
using RiverSystem.Api.NetworkElements.Storage;
using RiverSystem.Attributes;
using RiverSystem.Prototyping;
using RiverSystem.Storages.Geometry;
using TIME.Core;
using TIME.Core.Metadata;
using TIME.Core.Units;
using TIME.Science.Mathematics.Functions;
using TIME.Science.Utils;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FlowMatters.Source.DODOC.Storage
{
    public class StorageDOC : ProxyStorageSourceSinkModel
    {
        public StorageDOC()
        {
            InitialLeafDryMatterNonReadilyDegradable = new LinearPerPartFunction();
            InitialLeafDryMatterReadilyDegradable = new LinearPerPartFunction();
            LeafA = new LinearPerPartFunction();
            
            // set default values
            DOCDecayConstantAt20DegreeC = 0.03;
            FirstOrderDOCReleaseRateAt20DegreeC = 0.86;
            FirstOrderDOCReleaseRateAt20DegreeCNonReadily = 0.125;
            LeafK1 = 0.03;
            LeafK2 = 0.003;
            MaxDOCReleasedAt20DegreeC = 80;
            MaxDOCReleasedAt20DegreeCNonReadily = 10;
            ReaerationCoefficient = 0.08;
            StructureRerationCoefficient = 0.6;
            WaterQualityFactor = 0.65;
            WaterTemperature = 20;

            _heightForSurfaceAreaLookup = surfaceArea =>
            {
                var points = StorageModel.StoreGeometry.Cast<DiscreteStoreGeometryEntry>().ToList();
                for (var i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    if (i == points.Count - 1)
                    {
                        var lowerPoint = points[i - 1];
                        return MathUtils.linearInterpolation(surfaceArea, lowerPoint.surfaceArea, lowerPoint.height, point.surfaceArea, point.height);
                    }

                    var nextPoint = points[i + 1];

                    if (surfaceArea >= point.surfaceArea && surfaceArea <= nextPoint.surfaceArea)
                        return MathUtils.linearInterpolation(surfaceArea, point.surfaceArea, point.height, nextPoint.surfaceArea, nextPoint.height);
                }
                throw new Exception($"Could not lookup height for surface area: {surfaceArea}");
            };
        }

        // WHEN ADDING PROPERTIES, REMEMBER TO CLONE!
        [Parameter, Aka("Max Accumulation Area")]
        [CalculationUnits(CommonUnits.squareMetres)]
        public double MaxAccumulationArea { get; set; }

        [Parameter, Aka("Fraction Degradeable")]
        public double LeafAccumulationConstant { get; set; }

        [Parameter, Aka("Reaeration Coefficient")]
        public double ReaerationCoefficient { get; set; }

        [Parameter, Aka("Leaf A"), LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Litter Accumulation Constant", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction LeafA { get; set; }

        [Parameter, Aka("Readily degradable dry litter decay rate")]
        public double LeafK1 { get; set; }

        [Parameter, Aka("Non-readily degradable dry litter decay rate")]
        public double LeafK2 { get; set; }

        [Parameter]
        [LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Initial non-readily degradable dry litter", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction InitialLeafDryMatterNonReadilyDegradable { get; set; }

        [Parameter]
        [LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Initial readily degradable dry litter", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction InitialLeafDryMatterReadilyDegradable { get; set; }

        [Parameter, Aka("Primary Production Reaeration")]
        public double PrimaryProductionReaeration { get; set; }

        [Parameter, Aka("Water Temperature"), CalculationUnits(CommonUnits.celsius), DisplayUnit(CommonUnits.celsius)]
        public double WaterTemperature { get; set; }

        [Parameter, Aka("First Order DOC Release Rate at 20ºC - Readily")]
        public double FirstOrderDOCReleaseRateAt20DegreeC { get; set; }

        [Parameter, Aka("First Order DOC Release Rate at 20ºC - Non Readily")]
        public double FirstOrderDOCReleaseRateAt20DegreeCNonReadily { get; set; }

        [Parameter, Aka("Max DOC Released from Litter at 20ºC - Readily")]
        public double MaxDOCReleasedAt20DegreeC { get; set; }

        [Parameter, Aka("Max DOC Released from Litter at 20ºC - Non Readily")]
        public double MaxDOCReleasedAt20DegreeCNonReadily { get; set; }

        [Parameter, Aka("DOC Decomposition rate at 20ºC")]
        public double DOCDecayConstantAt20DegreeC { get; set; }

        [Parameter, Aka("Water Quality Factor")]
        public double WaterQualityFactor { get; set; }

        [Parameter, Aka("Weir/Dam/Spillway Reaeration Coefficient")]
        public double StructureRerationCoefficient { get; set; }

        [Parameter, Aka("Static Head Loss"), CalculationUnits(CommonUnits.metres)]
        public double StaticHeadLoss { get; set; }

        /// <summary>
        /// Flag used to specify how the <see cref="FlowMatters.Source.DODOC.Core.FloodplainDoDoc"/> Zones are initialised
        /// </summary>
        [Parameter, Aka("Initialise With Multiple Zones")]
        public bool InitialiseWithMultipleZones{ get; set; }

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

        [Output, CalculationUnits(CommonUnits.milligrams)]
        public double DOCEnteringWater { get; private set; }

        [Output, CalculationUnits(CommonUnits.kilograms)]
        public double TotalWetLeaf { get; private set; }

        [Output]
        public double LeafWetMatterReadilyDegradable { get; private set; }

        [Output,CalculationUnits(CommonUnits.kgPerHa)]
        public double LeafDryMatterReadilyDegradableRate { get; private set; }

        [Output, CalculationUnits(CommonUnits.kgPerHa)]
        public double LeafDryMatterNonReadilyDegradableRate { get; private set; }
        [Output]
        public double TotalDryMattergm2 { get; private set; }
        [Output]
        public double LeafWetMatterNonReadilyDegradable { get; private set; }

        [Output]
        public double FloodplainWetAreaHa { get; private set; }

        [Output]
        public double FloodplainDryAreaHa { get; private set; }

        [Output]
        public double Leach1 { get; private set; }

        [Output]
        public double Leach1NonReadily { get; private set; }

        [Output]
        public double LeachingRate { get; private set; }

        [Output]
        public double DocMax { get; private set; }

        [Output]
        public double DocMaxNonReadily { get; private set; }

        [Output, Aka("Total Zone Leaf Accumlation")]
        public double LeafAccumulation { get; private set; }

        [Output, Aka("Average Zone Leaf Accumlation")]
        public double AverageLeafAccumulation { get; private set; }

        private Func<double, double> _heightForSurfaceAreaLookup;


        protected override void UpdateWorker(double constituentConcentration)
        {
            Worker.ConcentrationDoc = constituentConcentration;

            Worker.MaxAccumulationArea = MaxAccumulationArea;
            Worker.LeafAccumulationConstant = LeafA;
            Worker.ReaerationCoefficient = ReaerationCoefficient;
            Worker.LeafA = LeafAccumulationConstant;
            Worker.LeafK1 = LeafK1;
            Worker.LeafK2 = LeafK2;
            Worker.InitialLeafDryMatterNonReadilyDegradable = InitialLeafDryMatterNonReadilyDegradable;
            Worker.InitialLeafDryMatterReadilyDegradable = InitialLeafDryMatterReadilyDegradable;
            Worker.PrimaryProductionReaeration = PrimaryProductionReaeration;
            Worker.WaterTemperature = WaterTemperature;
            Worker.FirstOrderDOCReleaseRateAt20DegreeC = FirstOrderDOCReleaseRateAt20DegreeC;
            Worker.FirstOrderDOCReleaseRateAt20DegreeCNonReadily = FirstOrderDOCReleaseRateAt20DegreeCNonReadily;
            Worker.MaxDOCReleasedAt20DegreeC = MaxDOCReleasedAt20DegreeC;
            Worker.MaxDOCReleasedAt20DegreeCNonReadily = MaxDOCReleasedAt20DegreeCNonReadily;
            Worker.DOCDecayConstantAt20DegreeC = DOCDecayConstantAt20DegreeC;
            Worker.StructureRerationCoefficient = StructureRerationCoefficient;
            Worker.WaterQualityFactor = WaterQualityFactor;
            Worker.StaticHeadLoss = StaticHeadLoss;
            Worker.InitialiseWithMultipleZones = InitialiseWithMultipleZones;

            
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

            Worker.Fac = 1.0;
        }
        
        protected override void RetrieveResults()
        {
            ProcessedLoad = Worker.DissolvedOrganicCarbonLoad;
            ZoneCount = Worker.ZoneCount;
            CountInundatedZones = Worker.CountInundatedZones;
            CountDryZones = Worker.CountDryZones;
            LeafDryMatterReadilyDegradable = Worker.LeafDryMatterReadilyDegradable;
            LeafDryMatterNonReadilyDegradable = Worker.LeafDryMatterNonReadilyDegradable;
            TemperatureEst = Worker.WaterTemperatureEst;
            SoilO2Kg = Worker.SoilO2Kg;
            DoCo2 = Worker.DoCo2;
            Production = Worker.Production;
            Reaeration = Worker.Reaeration;
            ConsumedDocMilligrams = Worker.ConsumedDocMilligrams;
            DOCEnteringWater = Worker.DOCEnteringWater;
            TotalWetLeaf = Worker.TotalWetLeaf;
            LeafWetMatterReadilyDegradable = Worker.LeafWetMatterReadilyDegradable;
            LeafDryMatterReadilyDegradableRate = Worker.LeafDryMatterReadilyDegradableRate;
            LeafDryMatterNonReadilyDegradableRate = Worker.LeafDryMatterNonReadilyDegradableRate;
            TotalDryMattergm2 = Worker.TotalDryMattergm2;
            LeafWetMatterNonReadilyDegradable = Worker.LeafWetMatterNonReadilyDegradable;
            FloodplainWetAreaHa = Worker.FloodplainWetAreaHa;
            FloodplainDryAreaHa = Worker.FloodplainDryAreaHa;
            Leach1 = Worker.Leach1;
            Leach1NonReadily = Worker.Leach1NonReadily;
            LeachingRate = Worker.LeachingRate;
            DocMax = Worker.DocMax;
            DocMaxNonReadily = Worker.DocMaxNonReadily;
            LeafAccumulation = Worker.TotalZoneAccumulation;
            AverageLeafAccumulation = Worker.AverageZoneAccumulation;
        }
    }
}