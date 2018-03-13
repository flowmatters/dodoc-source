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
        public double DocConsumptionCoefficient
        {
            get { return Feature.DocConsumptionCoefficient; }
            set { Feature.DocConsumptionCoefficient = value; }
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
        public double TemperatureObs
        {
            get { return Feature.TemperatureObs; }
            set { Feature.TemperatureObs = value; }
        }

        [Parameter]
        public double[] tempX
        {
            get { return Feature.tempX; }
            set { Feature.tempX = value; }
        }

        [Parameter]
        public double[] DOC_max
        {
            get { return Feature.DOC_max; }
            set { Feature.DOC_max = value; }
        }

        [Parameter]
        public double[] DOC_k
        {
            get { return Feature.DOC_k; }
            set { Feature.DOC_k = value; }
        }

        [Parameter]
        public double[] ProductionCoefficients
        {
            get { return Feature.ProductionCoefficients; }
            set { Feature.ProductionCoefficients = value; }
        }

        [Parameter]
        public double[] ProductionBreaks
        {
            get { return Feature.ProductionBreaks; }
            set { Feature.ProductionBreaks = value; }
        }
    }
}