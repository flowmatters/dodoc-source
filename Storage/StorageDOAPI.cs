using RiverSystem.Api.NetworkElements.Common.Constituents;
using TIME.Core.Metadata;

namespace FlowMatters.Source.DODOC.Storage
{
    [WorksWith(typeof(StorageDO))]
    // ReSharper disable once UnusedMember.Global
    public class StorageDOAPI : ProcessingModel<StorageDO>
    {
        public new string Name
        {
            get { return "Storage DO"; }
        }

        public double FloodplainElevation
        {
            get { return Feature.FloodplainElevation; }
            set { Feature.FloodplainElevation = value; }
        }
    }
}