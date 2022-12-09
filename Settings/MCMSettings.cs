using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreMercenariesForMyTavern.Settings
{
    internal class MCMSettings : AttributeGlobalSettings<MCMSettings>, ISettingVariables
    {
        public override string Id => "MoreMercsForMyTavern";

        public override string DisplayName => "More Mercenaries For My Tavern";

        public override string FolderName => "MoreMercsForMyTavern";

        public override string FormatType => "xml";

        [SettingPropertyInteger("{=settings_minMercenary}Minimum Number Of Mercenaries Per Type", minValue: GlobalModSettings.minMinMercenaryCount, maxValue: GlobalModSettings.maxMinMercenaryCount, Order = 1, HintText = "{=settings_minMercenary}Minimum number of mercenaries per type that can spawn per town.", RequireRestart = false)]
        public int minNumberOfMercenary { get; set; } = GlobalModSettings.minMinMercenaryCount;
        [SettingPropertyInteger("{=settings_maxMercenary}Maximum Number Of Mercenaries Per Type", minValue: GlobalModSettings.minMaxMercenaryCount, maxValue: GlobalModSettings.maxMaxMercenaryCount, Order = 1, HintText = "{=settings_maxMercenary}Maximum number of mercenaries per type that can spawn per town. MUST BE HIGHER THAN ABOVE MINIMUM VALUE", RequireRestart = false)]
        public int maxNumberOfMercenary { get; set; } = GlobalModSettings.minMaxMercenaryCount;
    }
}
