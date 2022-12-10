using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace MoreMercenariesForMyTavern.Data
{
    public class SaveContainer
    {
        [SaveableField(1)]
        public Dictionary<Town, MercenaryTavernEntity> data;
    }
}
