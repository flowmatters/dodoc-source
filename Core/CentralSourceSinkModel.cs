using System;
using System.Collections.Concurrent;

// Try to avoid dependencies on TIME/RiverSystem

namespace FlowMatters.Source.DODOC.Core
{
    public class CentralSourceSinkModel
    {
        private static CentralSourceSinkModel _instance;

        private CentralSourceSinkModel()
        {
            
        }

        public static CentralSourceSinkModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CentralSourceSinkModel();
                return _instance;
            }
        }

        private ConcurrentDictionary<IAreal, DoDocModel> _models;

        public DoDocModel GetModel(IAreal key,bool floodplain)
        {
            DoDocModel result = _models.GetOrAdd(key, k => floodplain?((DoDocModel)new FloodplainDoDoc()):(new RoutingDoDoc()));
            result.Areal = key;
            return result;
        }

        public void Reset()
        {
            if(_models==null||_models.Count>0)
                _models = new ConcurrentDictionary<IAreal, DoDocModel>();
        }
    }
}
