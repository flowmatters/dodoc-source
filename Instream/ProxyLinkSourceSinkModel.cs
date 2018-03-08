using System;
using System.ComponentModel;
using System.Timers;
using FlowMatters.Source.DODOC.Core;
using RiverSystem;
using RiverSystem.Flow;
using TIME.Core.Metadata;
using TIME.ManagedExtensions;
using TIME.Science.Algebra;

namespace FlowMatters.Source.DODOC.Instream
{
    public abstract class ProxyLinkSourceSinkModel : LinkSourceSinkModel
    {
        public DoDocModel Worker { get; private set; }

        public bool Debug { get; set; }

        private bool _firstTimeStep;
        public override void reset()
        {
            base.reset();
            Worker = null;
            _firstTimeStep = true;
            CentralSourceSinkModel.Instance.Reset();
        }

        private DoDocModel GetWorker()
        {
            return CentralSourceSinkModel.Instance.GetModel(new DivisionAreal(Division));
        }
    
        public override void runTimeStep(DateTime now, double theTimeStepInSeconds)
        {
            if (_firstTimeStep)
            {
                // we call get Elevation in the first timestep since we don't know what the initial value will
                // get until the first timestep starts (i.e. what rating curve to use)
                Worker.Elevation = GetElevation(now, theTimeStepInSeconds);
                _firstTimeStep = false;
            }
            Worker.Run(now); // If not run

            RetrieveResults();
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

        private double GetElevation(DateTime now, double stepInSeconds)
        {
            if (Link.RatingCurveLibrary == null || Link.RatingCurveLibrary.Curves.Count == 0)
                return 0;
            
            if (Worker is FloodplainDoDoc)
            {
                // need to get the floodplain elevation
                var floodPlainLevel = Link.RatingCurveLibrary.GetCurrentOverbankFlowLevel(now);
                return floodPlainLevel;

            }
            else
            {
                var storageFlowRouting = Link.FlowRouting as StorageRouting;
                if (storageFlowRouting == null)
                    return 0d;
                if (storageFlowRouting.IsInitFlow)
                {
                    return Link.RatingCurveLibrary.LevelForDischarge(storageFlowRouting.InitFlow, now);
                }

                return Link.RatingCurveLibrary.LevelForDischarge(storageFlowRouting.InitStorage/stepInSeconds, now);
            }
        }

        protected abstract void UpdateWorker(double constituentConcentration);
        protected abstract void RetrieveResults();
    }
}
