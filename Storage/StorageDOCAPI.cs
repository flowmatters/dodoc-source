using RiverSystem.Api.NetworkElements.Common.Constituents;
using RiverSystem.Api.Utils;
using TIME.Core;
using TIME.Core.Metadata;
using TIME.Science.Mathematics.Functions;

namespace FlowMatters.Source.DODOC.Storage
{
    [WorksWith(typeof(StorageDOC))]
    // ReSharper disable once UnusedMember.Global
    public class StorageDOCAPI : ProcessingModel<StorageDOC>
    {
        public new string Name => "Storage DOC";

        public double FloodplainElevation
        {
            get { return Feature.FloodplainElevation; }
            set { Feature.FloodplainElevation = value; }
        }

        [Parameter]
        [CalculationUnits(CommonUnits.squareMetres)]
        public double MaxAccumulationArea
        {
            get { return Feature.MaxAccumulationArea; }
            set { Feature.MaxAccumulationArea = value; }
        }

        [Parameter]
        public double LeafAccumulationConstant
        {
            get { return Feature.LeafAccumulationConstant; }
            set { Feature.LeafAccumulationConstant = value; }
        }

        [Parameter]
        public double ReaerationCoefficient
        {
            get { return Feature.ReaerationCoefficient; }
            set { Feature.ReaerationCoefficient = value; }
        }

        [Parameter]
        public LinearPerPartFunction LeafA
        {
            get { return Feature.LeafA?.Clone(); }
            set { value.copyPointsTo(Feature.LeafA); }
        }

        [Parameter]
        public double LeafK1
        {
            get { return Feature.LeafK1; }
            set { Feature.LeafK1 = value; }
        }

        [Parameter]
        public double LeafK2
        {
            get { return Feature.LeafK2; }
            set { Feature.LeafK2 = value; }
        }


        [Parameter]
        public LinearPerPartFunction InitialLeafDryMatterReadilyDegradable
        {
            get { return Feature.InitialLeafDryMatterReadilyDegradable?.Clone(); }
            set { value.copyPointsTo(Feature.InitialLeafDryMatterReadilyDegradable); }
        }

        [Parameter]
        public LinearPerPartFunction InitialLeafDryMatterNonReadilyDegradable
        {
            get { return Feature.InitialLeafDryMatterNonReadilyDegradable?.Clone(); }
            set { value.copyPointsTo(Feature.InitialLeafDryMatterNonReadilyDegradable); }
        }

        [Parameter]
        public double PrimaryProductionReaeration
        {
            get { return Feature.PrimaryProductionReaeration; }
            set { Feature.PrimaryProductionReaeration = value; }
        }

        [Parameter]
        public double WaterTemperature
        {
            get { return Feature.WaterTemperature; }
            set { Feature.WaterTemperature = value; }
        }

        [Parameter]
        public double[] ProductionCoefficients
        {
            get { return Feature.ProductionCoefficients; }
        }

        [Parameter]
        public double[] ProductionBreaks
        {
            get { return Feature.ProductionBreaks; }
        }

        [Parameter]
        public double FirstOrderDOCReleaseRateAt20DegreeC
        {
            get { return Feature.FirstOrderDOCReleaseRateAt20DegreeC; }
            set { Feature.FirstOrderDOCReleaseRateAt20DegreeC = value; }
        }

        public double FirstOrderDOCReleaseRateAt20DegreeCNonReadily
        {
            get { return Feature.FirstOrderDOCReleaseRateAt20DegreeCNonReadily; }
            set { Feature.FirstOrderDOCReleaseRateAt20DegreeCNonReadily = value; }
        }

        [Parameter]
        public double MaxDOCReleasedFromComponentOfLitterAt20DegreeC
        {
            get { return Feature.MaxDOCReleasedAt20DegreeC;}
            set { Feature.MaxDOCReleasedAt20DegreeC = value; }
        }

        public double MaxDOCReleasedFromComponentOfLitterAt20DegreeCNonReadily
        {
            get { return Feature.MaxDOCReleasedAt20DegreeCNonReadily; }
            set { Feature.MaxDOCReleasedAt20DegreeCNonReadily = value; }
        }

        [Parameter]
        public double DOCDecayConstantAt20DegreeC
        {
            get { return Feature.DOCDecayConstantAt20DegreeC; }
            set { Feature.DOCDecayConstantAt20DegreeC = value; }
        }

        [Parameter]
        public double WaterQualityFactor
        {
            get { return Feature.WaterQualityFactor; }
            set { Feature.WaterQualityFactor = value; }
        }

        [Parameter]
        public double StructureRerationCoefficient
        {
            get { return Feature.StructureRerationCoefficient; }
            set { Feature.StructureRerationCoefficient = value; }
        }

        [Parameter]
        public double WaterQuaStaticHeadLosslityFactor
        {
            get { return Feature.StaticHeadLoss; }
            set { Feature.StaticHeadLoss = value; }
        }
        

        [Parameter]
        public bool InitialiseWithMultipleZones
        {
            get { return Feature.InitialiseWithMultipleZones; }
            set { Feature.InitialiseWithMultipleZones = value; }
        }
    }
}