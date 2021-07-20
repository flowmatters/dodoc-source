using RiverSystem.Api;
using RiverSystem.Api.NetworkElements.Common.Constituents;
using RiverSystem.Api.Utils;
using TIME.Core;
using TIME.Core.Metadata;
using TIME.Science.Mathematics.Functions;

namespace FlowMatters.Source.DODOC.Instream
{
    [WorksWith(typeof(InstreamDOC))]
    // ReSharper disable once UnusedMember.Global
    public class InstreamDOCAPI : ProcessingModel<InstreamDOC>
    {
        public new string Name => "Instream DOC";

        public override void Initialise(ApiContext context, object feature)
        {
            base.Initialise(context, feature);

            DOCDecayConstantAt20DegreeC = new DataPointer(context, Feature, () => Feature.DOCDecayConstantAt20DegreeC);
            FirstOrderDOCReleaseRateAt20DegreeC = new DataPointer(context, Feature, () => Feature.FirstOrderDOCReleaseRateAt20DegreeC);
            FirstOrderDOCReleaseRateAt20DegreeCNonReadily = new DataPointer(context, Feature, () => Feature.FirstOrderDOCReleaseRateAt20DegreeCNonReadily);
            LeafAccumulationConstant = new DataPointer(context, Feature, () => Feature.LeafAccumulationConstant);
            LeafK1 = new DataPointer(context, Feature, () => Feature.LeafK1);
            LeafK2 = new DataPointer(context, Feature, () => Feature.LeafK2);
            MaxDOCReleasedFromComponentOfLitterAt20DegreeC = new DataPointer(context, Feature, () => Feature.MaxDOCReleasedAt20DegreeC);
            MaxDOCReleasedFromComponentOfLitterAt20DegreeCNonReadily = new DataPointer(context, Feature, () => Feature.MaxDOCReleasedAt20DegreeCNonReadily);
            ReaerationCoefficient = new DataPointer(context, Feature, () => Feature.ReaerationCoefficient);
            WaterTemperature = new DataPointer(context, Feature, () => Feature.WaterTemperature);
        }


        public bool IsFloodplain
        {
            get { return Feature.IsFloodplain; }
            set { Feature.IsFloodplain = value; }
        }

        [Parameter]
        [CalculationUnits(CommonUnits.squareMetres)]
        public double MaxAccumulationArea
        {
            get { return Feature.MaxAccumulationArea; }
            set { Feature.MaxAccumulationArea = value; }
        }

        public DataPointer LeafAccumulationConstant
        {
            get;set;
        }

        public DataPointer ReaerationCoefficient
        {
            get;set;
        }
        
        [Parameter]
        public LinearPerPartFunction LeafA
        {
            get { return Feature.LeafA?.Clone(); }
            set { value.copyPointsTo(Feature.LeafA); }
        }

        public DataPointer LeafK1
        {
            get;set;
        }

        public DataPointer LeafK2
        {
            get;set;
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

        public DataPointer WaterTemperature
        {
            get;set;
        }
        
        public DataPointer FirstOrderDOCReleaseRateAt20DegreeC
        {
            get; set;
        }

        public DataPointer FirstOrderDOCReleaseRateAt20DegreeCNonReadily
        {
            get;
            set;
        }

        public DataPointer MaxDOCReleasedFromComponentOfLitterAt20DegreeC
        {
            get; set;
        }

        public DataPointer MaxDOCReleasedFromComponentOfLitterAt20DegreeCNonReadily
        {
            get; set;
        }

        public DataPointer DOCDecayConstantAt20DegreeC
        {
            get;set;
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
    }
}