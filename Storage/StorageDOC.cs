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
    [UsesGuidRecordingSystem]
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
        [Parameter, Aka("Max Accumulation Area"), PropertyId("8D5BF846-CEF2-4AD8-954F-91D4A27C053F")]
        [CalculationUnits(CommonUnits.squareMetres)]
        public double MaxAccumulationArea { get; set; }

        [Parameter, Aka("Fraction Degradeable"), PropertyId("8CB5D6D1-358D-421B-AB93-F189BB4D4324")]
        public double LeafAccumulationConstant { get; set; }

        [Parameter, Aka("Reaeration Coefficient"), PropertyId("100EECAC-D5DC-47D2-A219-31BE3A00467B")]
        public double ReaerationCoefficient { get; set; }

        [Parameter, Aka("Leaf A"), LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Litter Accumulation Constant", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction LeafA { get; set; }

        [Parameter, Aka("Readily degradable dry litter decay rate"), PropertyId("78CDCA14-9B7D-4132-857A-8083EFBA3524")]
        public double LeafK1 { get; set; }

        [Parameter, Aka("Non-readily degradable dry litter decay rate"), PropertyId("86C19A1C-3654-45DB-8BC2-FF637B325DC7")]
        public double LeafK2 { get; set; }

        [Parameter]
        [LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Initial non-readily degradable dry litter", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction InitialLeafDryMatterNonReadilyDegradable { get; set; }

        [Parameter]
        [LinearPerPartDescription("editor...", "Elevation", CommonUnits.metres, CommonUnits.metres, "Initial readily degradable dry litter", CommonUnits.kgPerHa, CommonUnits.kgPerHa)]
        public LinearPerPartFunction InitialLeafDryMatterReadilyDegradable { get; set; }

        [Parameter, Aka("Primary Production Reaeration"), PropertyId("5AA1A299-EABD-4545-9803-CE908FA8263A")]
        public double PrimaryProductionReaeration { get; set; }

        [Parameter, Aka("Water Temperature"), CalculationUnits(CommonUnits.celsius), DisplayUnit(CommonUnits.celsius), PropertyId("53F8F694-51C3-45E1-8B93-76F63FF3787D")]
        public double WaterTemperature { get; set; }

        [Parameter, Aka("First Order DOC Release Rate at 20ºC - Readily"), PropertyId("53244F44-E67A-4B76-A761-4617E517494E")]
        public double FirstOrderDOCReleaseRateAt20DegreeC { get; set; }

        [Parameter, Aka("First Order DOC Release Rate at 20ºC - Non Readily"), PropertyId("B7D18E1D-F0B8-47A5-A9C2-A45EAFC9B428")]
        public double FirstOrderDOCReleaseRateAt20DegreeCNonReadily { get; set; }

        [Parameter, Aka("Max DOC Released from Litter at 20ºC - Readily"), PropertyId("9688423E-479F-40B9-9CD8-5A0464278E21")]
        public double MaxDOCReleasedAt20DegreeC { get; set; }

        [Parameter, Aka("Max DOC Released from Litter at 20ºC - Non Readily"), PropertyId("D3B81B82-34D9-4B88-8B33-6FE8086752FA")]
        public double MaxDOCReleasedAt20DegreeCNonReadily { get; set; }

        [Parameter, Aka("DOC Decomposition rate at 20ºC"), PropertyId("CB3B22BC-BCDA-453B-92C0-C10930FC8FBF")]
        public double DOCDecayConstantAt20DegreeC { get; set; }

        [Parameter, Aka("Water Quality Factor"), PropertyId("968FFF9B-66E6-4378-89CF-ED1B235474BD")]
        public double WaterQualityFactor { get; set; }

        [Parameter, Aka("Weir/Dam/Spillway Reaeration Coefficient"), PropertyId("F5E0BBF4-AE61-4AD3-AB1C-8254D06B487C")]
        public double StructureRerationCoefficient { get; set; }

        [Parameter, Aka("Static Head Loss"), CalculationUnits(CommonUnits.metres), PropertyId("2D97A1FC-2E2D-49AB-9F31-03FFCBFDFB2E")]
        public double StaticHeadLoss { get; set; }

        /// <summary>
        /// Flag used to specify how the <see cref="FlowMatters.Source.DODOC.Core.FloodplainDoDoc"/> Zones are initialised
        /// </summary>
        [Parameter, Aka("Initialise With Multiple Zones")]
        public bool InitialiseWithMultipleZones{ get; set; }

        public double[] ProductionCoefficients { get; set; }

        public double[] ProductionBreaks { get; set; }

        [Output, PropertyId("988EF972-3DE1-493D-8269-8BCA3D022826")]
        public double ZoneCount { get; private set; }

        [Output, PropertyId("29BE8578-749B-4165-AD16-F7D65B5CD266")]
        public int CountInundatedZones { get; private set; }

        [Output, PropertyId("142F2B86-74E3-45A1-AA07-613AE97B61CA")]
        public int CountDryZones { get; private set; }

        [Output, PropertyId("324BAF1E-DB93-4F48-B0E1-5DB53A07B333")]
        public double LeafDryMatterReadilyDegradable { get; private set; }

        [Output, PropertyId("331853C9-B67E-4B40-84BD-4BC992F0E4E9")]
        public double LeafDryMatterNonReadilyDegradable { get; private set; }

        [Output, PropertyId("9FAC9C9D-CF77-4345-B1C7-B5FFBAB18F77")]
        public double TemperatureEst { get; private set; }

        [Output, PropertyId("96440E8D-B5D2-4344-8319-7D00BA950658")]
        public double SoilO2Kg { get; private set; }

        [Output, PropertyId("E628DE97-6BA1-46DA-8980-0BFB98551252")]
        public double DoCo2 { get; private set; }

        [Output, PropertyId("77D4BF06-AEC8-4C91-A8CB-AC69F035942F")]
        public double Production { get; private set; }

        [Output, PropertyId("3758EA61-CE31-4F92-953F-F20F9D68A21A")]
        public double Reaeration { get; private set; }

        [Output, PropertyId("0CD00109-E2C4-4386-9C83-3074E0AAED7A")]
        public double ConsumedDocMilligrams { get; private set; }

        [Output, CalculationUnits(CommonUnits.milligrams), PropertyId("56076782-4E83-4261-83D4-39204B43BB7B")]
        public double DOCEnteringWater { get; private set; }

        [Output, CalculationUnits(CommonUnits.kilograms), PropertyId("E707ADDB-117F-426A-8C8A-9A0E255E5DE8")]
        public double TotalWetLeaf { get; private set; }

        [Output, PropertyId("96686574-0CDF-4A99-B426-B699667077E7")]
        public double LeafWetMatterReadilyDegradable { get; private set; }

        [Output,CalculationUnits(CommonUnits.kgPerHa), PropertyId("D4BE53F6-5422-4842-BB76-C62228CA75C7")]
        public double LeafDryMatterReadilyDegradableRate { get; private set; }

        [Output, CalculationUnits(CommonUnits.kgPerHa), PropertyId("D9D8CC09-E676-4D00-B14E-447ADC5BDE0E")]
        public double LeafDryMatterNonReadilyDegradableRate { get; private set; }
        [Output, PropertyId("FF606F36-EFD2-4724-99C3-63B6EF1D9EF9")]
        public double TotalDryMattergm2 { get; private set; }
        [Output, PropertyId("DD06675D-E6A0-4243-9A07-79373E6EC066")]
        public double LeafWetMatterNonReadilyDegradable { get; private set; }

        [Output, PropertyId("B6265F2B-D3B6-4C11-8533-CEE213ABBDDC")]
        public double FloodplainWetAreaHa { get; private set; }

        [Output, PropertyId("3D91E219-6E9A-4EFE-ADE9-49CFA466CDF7")]
        public double FloodplainDryAreaHa { get; private set; }

        [Output, PropertyId("3B7CA5D3-2C24-4E1E-9983-6704C039841E")]
        public double Leach1 { get; private set; }

        [Output, PropertyId("7B026637-AFA5-4E1A-A4B6-29DA15F1A238")]
        public double Leach1NonReadily { get; private set; }

        [Output, PropertyId("5D86324D-0E3D-4FD0-B6F3-B30F46619A56")]
        public double LeachingRate { get; private set; }

        [Output, PropertyId("2606813A-6CB9-4DCD-8B84-A7CFFD6C9EE4")]
        public double DocMax { get; private set; }

        [Output, PropertyId("D1F5BE93-71D5-4683-AAF4-C352A25CD7A4")]
        public double DocMaxNonReadily { get; private set; }

        [Output, Aka("Total Zone Leaf Accumlation"), PropertyId("EEACA0C6-7F6C-4D9E-BE7F-E0932B236FE6")]
        public double LeafAccumulation { get; private set; }

        [Output, Aka("Average Zone Leaf Accumlation"), PropertyId("521BD453-5E94-4F7E-8CF0-E81DB73D93DC")]
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