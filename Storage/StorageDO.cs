namespace FlowMatters.Source.DODOC.Storage
{
    public class StorageDO : ProxyStorageSourceSinkModel
    {
        public StorageDO()
        {
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