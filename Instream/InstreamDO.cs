//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FlowMatters.Source.DODOC.Instream;
//using RiverSystem;

//namespace FlowMatters.Source.DODOC
//{
//    public class InstreamDO : ProxyLinkSourceSinkModel
//    {
//        public InstreamDO()
//        {
//        }

//        public override LinkSourceSinkModel CloneForMultipleDivisions()
//        {
//            return new InstreamDO() { IsFloodplain = IsFloodplain };
//        }

//        protected override void UpdateWorker(double constituentConcentration)
//        {
//            Worker.ConcentrationDo = constituentConcentration;
//        }

//        protected override void RetrieveResults()
//        {
//            ProcessedLoad = Worker.DissolvedOxygenLoad;
//        }
//    }
//}
