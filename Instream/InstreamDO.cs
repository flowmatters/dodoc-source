using RiverSystem;

namespace FlowMatters.Source.DODOC.Instream
{
    public class InstreamDO : ProxyLinkSourceSinkModel
    {
        public InstreamDO()
        {
        }

        public override LinkSourceSinkModel CloneForMultipleDivisions()
        {
            return new InstreamDO() { 
                IsFloodplain = IsFloodplain,
            };
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