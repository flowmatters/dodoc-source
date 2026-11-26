using System;
using System.Data.Common;
using RiverSystem.Persistence.NHibernatePersistence.DatabaseUpgrades.Util_6_21_0;
using TIME.Core;
using TIME.Management;

namespace FlowMatters.Source.DODOC.Persistence.DatabaseUpgrades
{
    public class DatabaseUpgrade6_21_0 : PluginDatabaseUpgradeBase
    {
        public DatabaseUpgrade6_21_0()
        {
            DatabaseVersion = new DatabaseVersion(6, 21, 0);
            PluginVersion = new Version(1, 1, 9, 0);
        }

        /// <summary>
        /// The DOC and DO storage processing models now use the GUID recording system. Converts the legacy
        /// "Constituents" recorder keys of storages that use them, if an earlier upgrade has not converted them, to GUID
        /// based keys. The instream (link) models of this plugin keep their legacy keys until links move to GUID keys.
        /// </summary>
        public override void PerformUpgrade(DbCommand command)
        {
            var converter = new GrandConstituentOutputRecorderConverter(command);
            converter.SetEntityIdsIntoDb(command);
            converter.UpdateRecorders(command);
        }
    }
}
