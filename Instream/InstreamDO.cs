using RiverSystem;

namespace FlowMatters.Source.DODOC.Instream
{
    public class InstreamDO : ProxyLinkSourceSinkModel
    {
        public override LinkSourceSinkModel CloneForMultipleDivisions()
        {
            return new InstreamDO();
        }
		

        protected override void UpdateWorker(double constituentConcentration)
        {
            Worker.ConcentrationDo = constituentConcentration;
        }

        protected override void RetrieveResults()
        {
            ProcessedLoad = Worker.DissolvedOxygenLoad;
        }
    }
}