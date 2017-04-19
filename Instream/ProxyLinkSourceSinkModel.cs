using System;
using System.ComponentModel;
using FlowMatters.Source.DODOC.Core;
using RiverSystem;
using TIME.Core.Metadata;
using TIME.ManagedExtensions;

namespace FlowMatters.Source.DODOC.Instream
{
    public abstract class ProxyLinkSourceSinkModel : LinkSourceSinkModel
    {
        [Parameter]
        public bool IsFloodplain { get; set; }

        [Parameter,Description("Numeric parameter for user interface. Set to 0 to disable floodplain. +ve values enable floodplain.")]
        public double ModelAsFloodplain
        {
            get { return IsFloodplain ? 1.0 : 0.0; }
            set { IsFloodplain = value > 0; }
        }

        public DoDocModel Worker { get; private set; }

        public bool Debug { get; set; }

        public override void reset()
        {
            base.reset();
            Worker = null;
            CentralSourceSinkModel.Instance.Reset();
//            worker = CentralSourceSinkModel.Instance.GetWorker(Link);
        }

        private DoDocModel GetWorker()
        {
            return CentralSourceSinkModel.Instance.GetModel(new DivisionAreal(Division), IsFloodplain);
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
            var constituentConcentration = TotalInitialVolume.EqualWithTolerance(0.0)?0.0: (UnprocessedLoad / TotalInitialVolume);
            Worker.WorkingVolume = TotalInitialVolume;
            UpdateWorker(constituentConcentration);
        }

        protected abstract void UpdateWorker(double constituentConcentration);
        protected abstract void RetrieveResults();
    }
}
