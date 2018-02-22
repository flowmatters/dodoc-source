using RiverSystem.Api.NetworkElements.Common.Constituents;
using TIME.Core;
using TIME.Core.Metadata;

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
        public double DocConsumptionCoefficient
        {
            get { return Feature.DocConsumptionCoefficient; }
            set { Feature.DocConsumptionCoefficient = value; }
        }

        [Parameter]
        public double LeafA
        {
            get { return Feature.LeafA; }
            set { Feature.LeafA = value; }
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
        public double InitialLeafDryMatterReadilyDegradable
        {
            get { return Feature.InitialLeafDryMatterReadilyDegradable; }
            set { Feature.InitialLeafDryMatterReadilyDegradable = value; }
        }

        [Parameter]
        public double InitialLeafDryMatterNonReadilyDegradable
        {
            get { return Feature.InitialLeafDryMatterNonReadilyDegradable; }
            set { Feature.InitialLeafDryMatterNonReadilyDegradable = value; }
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