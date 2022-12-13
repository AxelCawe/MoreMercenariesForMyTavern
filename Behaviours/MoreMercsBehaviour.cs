using MCM.Abstractions.Base.Global;
using MoreMercenariesForMyTavern.Data;
using MoreMercenariesForMyTavern.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.CampaignSystem.Party.PartyScreenLogic;

namespace MoreMercenariesForMyTavern.Behaviours
{
    internal class MoreMercsBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            //TODO: Register Events
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameLoaded));
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, new Action<Town>(DailyTickTown));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(OnSettlementLeft));
        }

        public override void SyncData(IDataStore dataStore)
        {

            
        }

        void OnGameLoaded(CampaignGameStarter starter)
        {
            if (DataManager.Current.MercenaryData == null)
            {
                DataManager.Current.MercenaryData = new Dictionary<Town, MercenaryTavernEntity>();
                
            }
            foreach (Settlement settlement in Campaign.Current.Settlements)
            {
                if (settlement.IsTown && !DataManager.Current.MercenaryData.ContainsKey(settlement.Town))
                {
                    DataManager.Current.MercenaryData.Add(settlement.Town, null);

                    DailyTickTown(settlement.Town);
                }
            }

        }
  

        void OnAfterNewGameCreated(CampaignGameStarter starter)
        {
            
        }
        
        private void DailyTickTown(Town town)
        {
            // TODO: Handle updating of spawning/replacing mercs
            if (!DataManager.Current.MercenaryData.ContainsKey(town))
                DataManager.Current.MercenaryData.Add(town, null);

            // if player is in town when the reset hits
            bool playerIsInTownDuringReset = false;
            if (town.Settlement == Settlement.CurrentSettlement && MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && MobileParty.MainParty.CurrentSettlement.Town == town)
            {
                playerIsInTownDuringReset = true;
                RemoveMercenaryCharactersFromTavern(town);
            }


            DataManager.Current.MercenaryData[town] = null;

            Random random = new Random((int)DateTime.Now.Ticks);
            List<CustomMercenaryDataTypeDefinition> allElligibleTroops = new List<CustomMercenaryDataTypeDefinition>();
            foreach (CustomMercenaryDataTypeDefinition typeDef in DataManager.Current.AllCustomMercenaryTypes)
            {
                if (typeDef.IsGlobalSpawn)
                {
                    allElligibleTroops.Add(typeDef);
                }
                else
                {
                    foreach (string cultureString in typeDef.CultureSpawns)
                    {
                        if (cultureString.ToLower() == town.Culture.ToString().ToLower())
                        {
                            allElligibleTroops.Add(typeDef);
                            break;
                        }
                    }

                }
            }

            if (allElligibleTroops.Count == 0)
                return;


            int randomID = MBRandom.RandomInt(0, allElligibleTroops.Count);
            MercenaryTavernEntity newTavernMerc = new MercenaryTavernEntity();
            newTavernMerc.TroopID = allElligibleTroops[randomID].TroopID;
            if (GlobalSettings<MCMSettings>.Instance.minNumberOfMercenary > GlobalSettings<MCMSettings>.Instance.maxNumberOfMercenary)
                newTavernMerc.Count = random.Next(GlobalSettings<MCMSettings>.Instance.maxNumberOfMercenary, GlobalSettings<MCMSettings>.Instance.minNumberOfMercenary);
            else
                newTavernMerc.Count = random.Next(GlobalSettings<MCMSettings>.Instance.minNumberOfMercenary, GlobalSettings<MCMSettings>.Instance.maxNumberOfMercenary);
            DataManager.Current.MercenaryData[town] = newTavernMerc;

            if (playerIsInTownDuringReset)
            {
                AddCustomMercenaryCharacterToTavern(town);
            }

        }
        public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (mobileParty != MobileParty.MainParty) return;
            if (settlement.IsTown)
            {
                if (!DataManager.Current.MercenaryData.ContainsKey(settlement.Town))
                    DataManager.Current.MercenaryData.Add(settlement.Town, null);
                AddCustomMercenaryCharacterToTavern(settlement.Town);
            }
                
        }

        public void OnSettlementLeft(MobileParty mobileParty, Settlement settlement)
        {
            if (mobileParty != MobileParty.MainParty) return;
            if (settlement.IsTown)
            {
                if (!DataManager.Current.MercenaryData.ContainsKey(settlement.Town))
                    DataManager.Current.MercenaryData.Add(settlement.Town, null);
                RemoveMercenaryCharactersFromTavern(settlement.Town);
            }
        }

        // start of the dialog and game Menu code flows
        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
            AddGameMenus(campaignGameStarter);
            if (DataManager.Current.MercenaryData == null)
            {
                DataManager.Current.MercenaryData = new Dictionary<Town, MercenaryTavernEntity>();
                foreach (Settlement settlement in Campaign.Current.Settlements)
                {
                    if (settlement.IsTown)
                    {
                        DailyTickTown(settlement.Town);
                    }
                }
            }
        }

        //Remove Character from the Tavern
        private void RemoveMercenaryCharactersFromTavern(Town town)
        {

            MercenaryTavernEntity modTavernData  = DataManager.Current.MercenaryData[town];
            if (modTavernData == null)
                return;
            Location tavern = town.Settlement.LocationComplex.GetLocationWithId("tavern");
            {
                LocationCharacter locationChar = modTavernData.LocationChar;
                if (tavern != null && tavern.ContainsCharacter(locationChar))
                {
                    tavern.RemoveLocationCharacter(locationChar);
                }
            }
            
        }

        // Adding Character to the Tavern
        private void AddCustomMercenaryCharacterToTavern(Town town)
        {
            if (!Hero.MainHero.IsPrisoner && !town.Settlement.IsUnderSiege && town.Settlement.LocationComplex != null)
            {
                Location tavern = town.Settlement.LocationComplex.GetLocationWithId("tavern");
                if (tavern != null)
                {
                    for (int i = 0; i < DataManager.Current.MercenaryData[town].Count; ++i)
                        tavern.AddLocationCharacters(new CreateLocationCharacterDelegate(DataManager.Current.MercenaryData[town].AddCharacterData), town.Settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
                }
            }
        }



        // TAVERN CODE
        protected void AddDialogs(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddDialogLine("cust_mercenary_recruit_start", "start", "cust_mercenary_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? Me and {?CMERCS_PLURAL}{CMERCS_MERCENARY_COUNT} of my mates{?}one of my mates{\\?} looking for a master. You might call us mercenaries, like. We'll join you for {CMERCS_GOLD_AMOUNT}{GOLD_ICON}",
                new ConversationSentence.OnConditionDelegate(conversation_mercenary_recruit_plural_start_on_condition), null, 150, null);
            campaignGameStarter.AddDialogLine("cust_mercenary_recruit_start_single", "start", "cust_mercenary_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? I am looking for a master. I'll join you for {CMERCS_GOLD_AMOUNT}{GOLD_ICON}",
                new ConversationSentence.OnConditionDelegate(conversation_mercenary_recruit_single_start_on_condition), null, 150, null);
     
            campaignGameStarter.AddPlayerLine("cust_mercenary_recruit_accept", "cust_mercenary_tavern_talk", "cust_mercenary_tavern_talk_hire", "All right. I will hire {?CMERCS_PLURAL}all of you{?}you{\\?}. Here is {CMERCS_GOLD_AMOUNT_ALL}{GOLD_ICON}",
                new ConversationSentence.OnConditionDelegate(conversation_mercenary_recruit_accept_on_condition), delegate ()
                {
                    HireCustomMercenariesInTavern(false);
                }, 100, null, null);
            campaignGameStarter.AddPlayerLine("cust_mercenary_recruit_accept_some", "cust_mercenary_tavern_talk", "cust_mercenary_tavern_talk_hire", "All right. But I can only hire {CMERCS_MERCENARY_COUNT_SOME} of you. Here is {CMERCS_GOLD_AMOUNT_SOME}{GOLD_ICON}",
                new ConversationSentence.OnConditionDelegate(conversation_mercenary_recruit_accept_some_on_condition), delegate ()
                {
                    HireCustomMercenariesInTavern(false);
                }, 100, null, null);
            campaignGameStarter.AddPlayerLine("cust_mercenary_recruit_reject_gold", "cust_mercenary_tavern_talk", "close_window", "That sounds good. But I can't hire any more men right now.",
                new ConversationSentence.OnConditionDelegate(conversation_mercenary_recruit_reject_gold_on_condition), null, 100, null, null);
            campaignGameStarter.AddPlayerLine("cust_mercenary_recruit_reject", "cust_mercenary_tavern_talk", "close_window", "Sorry. I don't need any other men right now.",
                new ConversationSentence.OnConditionDelegate(conversation_mercenary_recruit_dont_need_men_on_condition), null, 100, null, null);
            campaignGameStarter.AddDialogLine("cust_mercenary_recruit_end", "cust_mercenary_tavern_talk_hire", "close_window", "{RANDOM_HIRE_SENTENCE}",
                new ConversationSentence.OnConditionDelegate(conversation_mercenary_recruit_end_on_condition), null, 100, null);
            campaignGameStarter.AddDialogLine("cust_mercenary_recruit_start", "start", "close_window", "Don't worry, I'll be ready. Just having a last drink for the road.",
                new ConversationSentence.OnConditionDelegate(conversation_mercenary_recruited_on_condition), null, 150, null);
        }

        private bool CustomMercenaryIsInTavern(Town town)
        {
            
            if (CampaignMission.Current == null || CampaignMission.Current.Location == null)
            {
                return false;
            }

            bool flag = false;
            MercenaryTavernEntity entity = DataManager.Current.MercenaryData[town];
            {
                if (entity.LocationChar.Character.Name == CharacterObject.OneToOneConversationCharacter.Name)
                {
                    flag = true;
                }
            }
            return CampaignMission.Current.Location.StringId == "tavern" && flag;
        }


        // Conditions for starting line dialog
        private bool conversation_mercenary_recruit_plural_start_on_condition()
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            MercenaryTavernEntity mercenaryTavernEntity = null;
            MercenaryTavernEntity entity = DataManager.Current.MercenaryData[currentTown];
            {
                if (entity.LocationChar.Character.Name == CharacterObject.OneToOneConversationCharacter.Name)
                    mercenaryTavernEntity = entity;
            }
            if (mercenaryTavernEntity == null)
                return false;
            bool flag =  mercenaryTavernEntity.Count > 1 && CustomMercenaryIsInTavern(currentTown);
            if (flag)
            {
                int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(CharacterObject.OneToOneConversationCharacter, Hero.MainHero, false); 
                MBTextManager.SetTextVariable("CMERCS_PLURAL", (mercenaryTavernEntity.Count > 1) ? 1 : 0);
                MBTextManager.SetTextVariable("CMERCS_MERCENARY_COUNT", mercenaryTavernEntity.Count - 1);
                MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT", troopRecruitmentCost * mercenaryTavernEntity.Count);
            }
            return flag;
        }

        private bool conversation_mercenary_recruit_single_start_on_condition()
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            MercenaryTavernEntity mercenaryTavernEntity = null;
            MercenaryTavernEntity entity = DataManager.Current.MercenaryData[currentTown];
            {
                if (entity.LocationChar.Character.Name == CharacterObject.OneToOneConversationCharacter.Name)
                    mercenaryTavernEntity = entity;
            }
            if (mercenaryTavernEntity == null)
                return false;

            bool flag = mercenaryTavernEntity.Count == 1 && CustomMercenaryIsInTavern(currentTown);
            if (flag)
            {
                int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(CharacterObject.OneToOneConversationCharacter, Hero.MainHero, false);
                MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT", mercenaryTavernEntity.Count * troopRecruitmentCost);
            }
            return flag;
        }

        // Conditions for Hiring options and functions that follow
        private bool conversation_mercenary_recruit_accept_on_condition()
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            MercenaryTavernEntity mercenaryTavernEntity = null;
            MercenaryTavernEntity entity = DataManager.Current.MercenaryData[currentTown];
            {
                if (entity.LocationChar.Character.Name == CharacterObject.OneToOneConversationCharacter.Name)
                    mercenaryTavernEntity = entity;
            }
            if (mercenaryTavernEntity == null)
                return false;

            int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(CharacterObject.OneToOneConversationCharacter, Hero.MainHero, false); 
            int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
            MBTextManager.SetTextVariable("CMERCS_PLURAL", (mercenaryTavernEntity.Count > 1) ? 1 : 0);
            MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_ALL", troopRecruitmentCost * mercenaryTavernEntity.Count);
            return Hero.MainHero.Gold >= mercenaryTavernEntity.Count * troopRecruitmentCost && numOfTroopSlotsOpen >= mercenaryTavernEntity.Count;
        }


        private bool conversation_mercenary_recruit_accept_some_on_condition()
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            MercenaryTavernEntity mercenaryTavernEntity = null;
            MercenaryTavernEntity entity = DataManager.Current.MercenaryData[currentTown];
            {
                if (entity.LocationChar.Character.Name == CharacterObject.OneToOneConversationCharacter.Name)
                    mercenaryTavernEntity = entity;
            }
            if (mercenaryTavernEntity == null)
                return false;
            int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(CharacterObject.OneToOneConversationCharacter, Hero.MainHero, false);
            int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
            if (Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen > 0 && (Hero.MainHero.Gold < mercenaryTavernEntity.Count * troopRecruitmentCost || numOfTroopSlotsOpen < mercenaryTavernEntity.Count))
            {
                int numberToHire = 0;
                while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && numOfTroopSlotsOpen > numberToHire)
                {
                    numberToHire++;
                }
                MBTextManager.SetTextVariable("CMERCS_MERCENARY_COUNT_SOME", numberToHire);
                MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_SOME", troopRecruitmentCost * numberToHire);
                return true;
            }
            return false;
        }

      

        // Conditions close Conversation
        private bool conversation_mercenary_recruited_on_condition()
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
     
            return CustomMercenaryIsInTavern(currentTown);
        }

        private bool conversation_mercenary_recruit_reject_gold_on_condition()
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            MercenaryTavernEntity mercenaryTavernEntity = null;
            MercenaryTavernEntity entity = DataManager.Current.MercenaryData[currentTown];
            {
                if (entity.LocationChar.Character.Name == CharacterObject.OneToOneConversationCharacter.Name)
                    mercenaryTavernEntity = entity;
            }
            if (mercenaryTavernEntity == null)
                return false;
            int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(CharacterObject.OneToOneConversationCharacter, Hero.MainHero, false);
            int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
            return Hero.MainHero.Gold < troopRecruitmentCost || numOfTroopSlotsOpen <= 0;
        }

        private bool conversation_mercenary_recruit_dont_need_men_on_condition()
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            MercenaryTavernEntity mercenaryTavernEntity = null;
            MercenaryTavernEntity entity = DataManager.Current.MercenaryData[currentTown];
            {
                if (entity.LocationChar.Character.Name == CharacterObject.OneToOneConversationCharacter.Name)
                    mercenaryTavernEntity = entity;
            }
            if (mercenaryTavernEntity == null)
                return false;
            int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(CharacterObject.OneToOneConversationCharacter, Hero.MainHero, false);
            int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
            return Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen > 0;
        }

        // Successful hire npc phrase
        public bool conversation_mercenary_recruit_end_on_condition()
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
            MBTextManager.SetTextVariable("RANDOM_HIRE_SENTENCE", GameTexts.FindText("str_mercenary_tavern_talk_hire", MBRandom.RandomInt(4).ToString()));
            return true;
        }

        // Actual Hiring from Tavern
        private void HireCustomMercenariesInTavern(bool pastPartyLimit = false)
        {
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return;
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            MercenaryTavernEntity mercenaryTavernEntity = DataManager.Current.MercenaryData[currentTown];
        
            if (mercenaryTavernEntity == null) return;

            int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID] as CharacterObject, Hero.MainHero, false);
            int numberOfMercsToHire = 0;

            int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
            while (Hero.MainHero.Gold > troopRecruitmentCost * (numberOfMercsToHire + 1) && mercenaryTavernEntity.Count > numberOfMercsToHire && (pastPartyLimit || numOfTroopSlotsOpen > numberOfMercsToHire))
            {
                    numberOfMercsToHire++;
            }
            mercenaryTavernEntity.Count -= numberOfMercsToHire;
            MobileParty.MainParty.AddElementToMemberRoster(DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID] as CharacterObject, numberOfMercsToHire, false);
            int amount = numberOfMercsToHire * troopRecruitmentCost;
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, amount, false);
            CampaignEventDispatcher.Instance.OnUnitRecruited(DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID] as CharacterObject, numberOfMercsToHire);
        }

        // GAME MENU CODE
        public void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            // index is location in menu 0 being top, 1 next if other of same index exist this are placed on top of them
            campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_all", "{=*}Recruit {C_MEN_COUNT} {C_MERCENARY_NAME} ({C_TOTAL_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(HireCustomMercenariesViaMenuCondition), delegate (MenuCallbackArgs x)
            {
                HireCustomMercenariesViaGameMenu(false, false);
            }, false, 5, false);
            campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_party_limit", "{=*}Recruit to Party Limit {C_MEN_COUNT_PL} {C_MERCENARY_NAME_PL} ({C_TOTAL_AMOUNT_PL}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(HireCustomMercenariesViaMenuConditionToPartyLimit), delegate (MenuCallbackArgs x)
            {
                HireCustomMercenariesViaGameMenu(false, true);
            }, false, 5, false);
            campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_hire_one", "{=*}Recruit 1 {C_MERCENARY_NAME_ONLY_ONE} ({C_TOTAL_AMOUNT_ONLY_ONE}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(HireCustomMercenariesViaMenuConditionHireOne), delegate (MenuCallbackArgs x)
            {
                HireCustomMercenariesViaGameMenu(true, false);
            }, false, 5, false);
        }
        private bool HireCustomMercenariesViaMenuConditionHireOne(MenuCallbackArgs args)
        {
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown || DataManager.Current.MercenaryData[currentTown].Count == 0) return false;
            MercenaryTavernEntity mercenaryTavernEntity = DataManager.Current.MercenaryData[currentTown];

            if (mercenaryTavernEntity != null && mercenaryTavernEntity.Count > 1)
            {
                int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID] as CharacterObject, Hero.MainHero, false);
                int numOfTroopPlayerCanBuy = Hero.MainHero.Gold / troopRecruitmentCost;
                if (numOfTroopPlayerCanBuy > 1)
                {
                    MBTextManager.SetTextVariable("C_MERCENARY_NAME_ONLY_ONE", DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID].Name);
                    MBTextManager.SetTextVariable("C_TOTAL_AMOUNT_ONLY_ONE", troopRecruitmentCost);
                    args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                    return true;
                }
            }
            return false;
        }

        private bool HireCustomMercenariesViaMenuCondition(MenuCallbackArgs args)
        {
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown || DataManager.Current.MercenaryData[currentTown].Count == 0) return false;
            MercenaryTavernEntity mercenaryTavernEntity = DataManager.Current.MercenaryData[currentTown];
            if (mercenaryTavernEntity != null && mercenaryTavernEntity.Count > 0)
            {
                int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID] as CharacterObject, Hero.MainHero, false); 
                if (Hero.MainHero.Gold >= troopRecruitmentCost)
                {
                    int numOfTroopPlayerCanBuy = (troopRecruitmentCost == 0) ? mercenaryTavernEntity.Count : Hero.MainHero.Gold / troopRecruitmentCost;
                    int num = Math.Min(mercenaryTavernEntity.Count, numOfTroopPlayerCanBuy);
                    MBTextManager.SetTextVariable("C_MEN_COUNT", num);
                    MBTextManager.SetTextVariable("C_MERCENARY_NAME", DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID].Name);
                    MBTextManager.SetTextVariable("C_TOTAL_AMOUNT", num * troopRecruitmentCost);
                    args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                    return true;
                }
            }
            return false;
        }

        private bool HireCustomMercenariesViaMenuConditionToPartyLimit(MenuCallbackArgs args)
        {
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown || DataManager.Current.MercenaryData[currentTown].Count == 0) return false;
            MercenaryTavernEntity mercenaryTavernEntity = DataManager.Current.MercenaryData[currentTown];
            if (mercenaryTavernEntity != null && mercenaryTavernEntity.Count > 0)
            {
                int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID] as CharacterObject, Hero.MainHero, false);
                int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
                int numOfTroopPlayerCanBuy = (troopRecruitmentCost == 0) ? mercenaryTavernEntity.Count : Hero.MainHero.Gold / troopRecruitmentCost;
                if (numOfTroopSlotsOpen > 0 && Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen < mercenaryTavernEntity.Count && numOfTroopSlotsOpen < numOfTroopPlayerCanBuy)
                {
                    int numOfMercs = Math.Min(mercenaryTavernEntity.Count, numOfTroopPlayerCanBuy);
                    numOfMercs = Math.Min(numOfTroopSlotsOpen, numOfMercs);
                    MBTextManager.SetTextVariable("C_MEN_COUNT_PL", numOfMercs);
                    MBTextManager.SetTextVariable("C_MERCENARY_NAME_PL", DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID].Name);
                    MBTextManager.SetTextVariable("C_TOTAL_AMOUNT_PL", numOfMercs * troopRecruitmentCost);
                    args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                    return true;
                }
            }
            return false;
        }

        private void HireCustomMercenariesViaGameMenu(bool buyingOne, bool toPartyLimit)
        {
            Town currentTown = Campaign.Current.MainParty.LeaderHero.CurrentSettlement.Town;
            if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown || DataManager.Current.MercenaryData[currentTown].Count == 0) return;
            MercenaryTavernEntity mercenaryTavernEntity = DataManager.Current.MercenaryData[currentTown];
            if (mercenaryTavernEntity == null) return;
            int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
            int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID] as CharacterObject, Hero.MainHero, false);
            if (mercenaryTavernEntity.Count > 0 && Hero.MainHero.Gold >= troopRecruitmentCost && (!toPartyLimit || numOfTroopSlotsOpen > 0))
            {
                int numOfMercs = 1;
                if (!buyingOne)
                {
                    int numOfTroopPlayerCanBuy = (troopRecruitmentCost == 0) ? mercenaryTavernEntity.Count : Hero.MainHero.Gold / troopRecruitmentCost;
                    numOfMercs = Math.Min(mercenaryTavernEntity.Count, numOfTroopPlayerCanBuy);
                    if (toPartyLimit) numOfMercs = Math.Min(numOfTroopSlotsOpen, numOfMercs);
                }
                MobileParty.MainParty.MemberRoster.AddToCounts(DataManager.Current.BasicCharacterObjects[mercenaryTavernEntity.TroopID] as CharacterObject, numOfMercs, false, 0, 0, true, -1);
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -(numOfMercs * troopRecruitmentCost), false);
                mercenaryTavernEntity.Count -= numOfMercs;
                GameMenu.SwitchToMenu("town_backstreet");
            }
        }
    }
}
