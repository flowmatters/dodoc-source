namespace FlowMatters.Source.DODOC.Core
{
    public class RoutingDoDoc : DoDocModel
    {
        protected override void ProcessDoc()
        {
            var load = ConcentrationDoc * WorkingVolume;
            ConsumedDocMilligrams = load*DocConsumptionCoefficient*Sigma*KG_TO_MG;
            DissolvedOrganicCarbonLoad = load - ConsumedDocMilligrams*MG_TO_KG;
        }

        protected override double SoilO2mg()
        {
            return 0.0;
        }
    }
}
