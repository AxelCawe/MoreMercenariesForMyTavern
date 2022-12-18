using MoreMercenariesForMyTavern.Behaviours;
using MoreMercenariesForMyTavern.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MoreMercenariesForMyTavern
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            
            AddBehaviors(gameStarter as CampaignGameStarter);
        }

        

        public override void AfterRegisterSubModuleObjects(bool isSavedCampaign)
        {
            base.AfterRegisterSubModuleObjects(isSavedCampaign);
            DataManager.Current.LoadData();
        }

        private void AddBehaviors(CampaignGameStarter gameStarter)
        {
            //gameStarter.AddBehavior(new SaveDataBehaviour());
            gameStarter.AddBehavior(new MoreMercsBehaviour());
        }
    }
}

