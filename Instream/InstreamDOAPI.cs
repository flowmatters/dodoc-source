using RiverSystem.Api.NetworkElements.Common.Constituents;
using TIME.Core.Metadata;

namespace FlowMatters.Source.DODOC.Instream
{
    [WorksWith(typeof(InstreamDO))]
    // ReSharper disable once UnusedMember.Global
    public class InstreamDOAPI : ProcessingModel<InstreamDO>
    {
        public new string Name => "Instream DO";

        public bool IsFloodplain
        {
            get { return Feature.IsFloodplain; }
            set { Feature.IsFloodplain = value; }
        }
    }
}