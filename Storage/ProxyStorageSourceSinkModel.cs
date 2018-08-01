using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowMatters.Source.DODOC.Core;
using RiverSystem;
using TIME.Core;
using TIME.Core.Metadata;
using TIME.ManagedExtensions;

namespace FlowMatters.Source.DODOC.Storage
{
    public abstract class ProxyStorageSourceSinkModel : StorageSourceSinkModel
    {
        [Parameter, Aka("Flood Plain Elevation"), CalculationUnits(CommonUnits.metres), DisplayUnit(CommonUnits.metres)] 
        public double FloodplainElevation { get; set; }

        public DoDocModel Worker { get; private set; }

        public bool Debug { get; set; }

        public override void reset()
        {
            base.reset();
            Worker = null;
            CentralSourceSinkModel.Instance.Reset();
        }

        private DoDocModel GetWorker()
        {
            var storageAreal = new StorageAreal(StorageModel,FloodplainElevation);

            var doDocModel = CentralSourceSinkModel.Instance.GetModel(storageAreal);

            doDocModel.FloodplainElevation = FloodplainElevation;

            return doDocModel;
        }

        public override void runTimeStep(DateTime now, double theTimeStepInSeconds)
        {
            Worker.Run(now); // If not run

            RetrieveResults();
            //            ProcessedLoad = worker.getFlux(ModelledConstituent, theTimeStepInSeconds);
        }

        public override void InputsUpdated()
        {
            if (Worker == null)
                Worker = GetWorker();
            Worker.Debug = Debug;
            var constituentConcentration = TotalInitialVolume.EqualWithTolerance(0.0) ? 0.0 : (UnprocessedLoad / TotalInitialVolume);
            Worker.WorkingVolume = TotalInitialVolume;
            Worker.Elevation = StorageModel.Level;
            UpdateWorker(constituentConcentration);
        }

        protected abstract void UpdateWorker(double constituentConcentration);
        protected abstract void RetrieveResults();
    }
}
