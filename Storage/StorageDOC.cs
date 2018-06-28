using System;
using System.Collections.Generic;
using System.Linq;
using RiverSystem.Api.NetworkElements.Storage;
using RiverSystem.Attributes;
using RiverSystem.Prototyping;
using RiverSystem.Storages.Geometry;
using TIME.Core;
using TIME.Core.Metadata;
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
            FirstOrderDOCReleaseRateAt20DegreeC = 0.4;
            FirstOrderDOCReleaseRateAt20DegreeCNonReadily = 0.3;
            LeafK1 = 0.03;
            LeafK2 = 0.003;
            MaxDOCReleasedAt20DegreeC = 40;
            MaxDOCReleasedAt20DegreeCNonReadily = 80;
            ReaerationCoefficient = 0.08;
            StructureRerationCoefficient = 0.6;
            WaterQualityFactor = 0.65;
            WaterTemperature = 20;
        }

        // WHEN ADDING PROPERTIES, REMEMBER TO CLONE!
        [Parameter, Aka("Max Accumulation Area")]
        [CalculationUnits(CommonUnits.squareMetres)]
        public double MaxAccumulationArea { get; set; }

        [Parameter, Aka("Fraction Degradeable")]
        public double LeafAccumulationConstant { get; set; }

        [Parameter, Aka("Reaeration Coefficient")]
        public double ReaerationCoefficient { get; set; }

        [Parameter, Aka("Leaf A"), LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Leaf Accumulation Constant", CommonUnits.none, CommonUnits.none)]
        public LinearPerPartFunction LeafA { get; set; }

        [Parameter, Aka("Leaf dry matter readily degradable decay rate")]
        public double LeafK1 { get; set; }

        [Parameter, Aka("Leaf dry matter non readily degradable decay rate")]
        public double LeafK2 { get; set; }

        [Parameter]
        [LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres,
            "Initial Leaf dry matter non readily degradable", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction InitialLeafDryMatterNonReadilyDegradable { get; set; }

        [Parameter]
        [LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Initial Leaf dry matter readily degradable", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
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
        public double Leach1NonReadily { get; private set; }

        [Output]
        public double LeachingRate { get; private set; }

        [Output]
        public double DocMax { get; private set; }

        [Output]
        public double DocMaxNonReadily { get; private set; }

        [Output, Aka("Leaf Accumlation")]
        public double LeafAccumulation => LeafA.f(Worker.Elevation);


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
            Worker.FirstOrderDOCReleaseRateAt20DegreeC = FirstOrderDOCReleaseRateAt20DegreeCNonReadily;
            Worker.MaxDOCReleasedAt20DegreeC = MaxDOCReleasedAt20DegreeC;
            Worker.MaxDOCReleasedAt20DegreeCNonReadily = MaxDOCReleasedAt20DegreeCNonReadily;
            Worker.DOCDecayConstantAt20DegreeC = DOCDecayConstantAt20DegreeC;
            Worker.StructureRerationCoefficient = StructureRerationCoefficient;
            Worker.WaterQualityFactor = WaterQualityFactor;
            Worker.StaticHeadLoss = StaticHeadLoss;

            Worker.AreaForHeightLookup = StorageModel.StoreGeometry.surfaceAreaForHeight;

            var heightForSurfaceAreeaLookup = new Func<double, double>(surfaceArea =>
            {
                var points = StorageModel.StoreGeometry.Cast<DiscreteStoreGeometryEntry>().ToList();

                for (var i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    if (i == points.Count - 1)
                        return point.height;
                    var nextPoint = points[i + 1];

                    if (surfaceArea >= point.surfaceArea && surfaceArea <= nextPoint.surfaceArea)
                        return MathUtils.linearInterpolation(surfaceArea, point.surfaceArea, point.height, nextPoint.surfaceArea, nextPoint.height);
                }
                throw new Exception($"Could not lookup height for surface area: {surfaceArea}");
            });

            Worker.HeightForAreaLookup = heightForSurfaceAreeaLookup;

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
            LeafWetMatterNonReadilyDegradable = Worker.LeafWetMatterNonReadilyDegradable;
            FloodplainWetAreaHa = Worker.FloodplainWetAreaHa;
            FloodplainDryAreaHa = Worker.FloodplainDryAreaHa;
            Leach1 = Worker.Leach1;
            Leach1NonReadily = Worker.Leach1NonReadily;
            LeachingRate = Worker.LeachingRate;
            DocMax = Worker.DocMax;
            DocMaxNonReadily = Worker.DocMaxNonReadily;
        }
    }
}