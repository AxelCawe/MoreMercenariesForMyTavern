using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace MoreMercenariesForMyTavern.Data
{
    internal class MoreMercsSaveTypeDefiner : SaveableTypeDefiner
    {
        public MoreMercsSaveTypeDefiner() : base(356355346)
        {
        }

        protected override void DefineClassTypes()
        {
            // The Id's here are local and will be related to the Id passed to the constructor
            AddClassDefinition(typeof(MercenaryTavernEntity), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<MercenaryTavernEntity>));
            ConstructContainerDefinition(typeof(Dictionary<Town, List<MercenaryTavernEntity>>));
        }
    }
}
