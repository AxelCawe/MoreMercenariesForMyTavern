using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace MoreMercenariesForMyTavern.Data
{
    public class DataManager
    {
        private string _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Mount and Blade II Bannerlord\\Configs\\ModSettings\\MoreMercenariesForMyTavern\\Entities");
        private string _errorDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Mount and Blade II Bannerlord\\Configs\\ModSettings\\MoreMercenariesForMyTavern");
        [SaveableField(1)]
        private static DataManager _instance;
        public static DataManager Current
        { 
            get 
            {
                if (_instance != null)
                    return _instance;
                else
                {
                    _instance = new DataManager();
                    return _instance;
                }
            } 
        }

        
        private List<CustomMercenaryDataTypeDefinition> _customMercenaryTypes;
        public ReadOnlyCollection<CustomMercenaryDataTypeDefinition> AllCustomMercenaryTypes { get { return _customMercenaryTypes.AsReadOnly(); } }

        public Dictionary<string, BasicCharacterObject> BasicCharacterObjects;

        [SaveableField(2)]
        public Dictionary<Town, MercenaryTavernEntity> MercenaryData;
  
        private string errorLog = string.Empty;

        public void LoadData()
        {
            _customMercenaryTypes = new List<CustomMercenaryDataTypeDefinition>();
            BasicCharacterObjects = new Dictionary<string, BasicCharacterObject>();
            if (Directory.Exists(_dataDirectory))
            {
                string[] allDataFiles = Directory.GetFiles(_dataDirectory, "*.xml");
                foreach (string file in allDataFiles)
                {
                    try
                    {
                        XmlSerializer reader = new XmlSerializer(typeof(CustomMercenaryDataTypeDefinition[]));
                        FileStream fileStream = new FileStream(file, FileMode.Open);
                        CustomMercenaryDataTypeDefinition[] fileCustomMercenaryTypes = reader.Deserialize(fileStream) as CustomMercenaryDataTypeDefinition[];
                        foreach (CustomMercenaryDataTypeDefinition custonMerc in fileCustomMercenaryTypes)
                        {
                            _customMercenaryTypes.Add(custonMerc);
                        }
                        fileStream.Close();
                    }
                    catch (Exception ex)
                    {
                        errorLog += $"[Error]: Loading of {file} failed!\n > Exception: {ex.Message}";
                    }
                }

            }
            else
                GenerateDefaultData();

            if (_customMercenaryTypes.Count == 0)
                GenerateDefaultData();

            List<CustomMercenaryDataTypeDefinition> correctedList = new List<CustomMercenaryDataTypeDefinition>();
            foreach (CustomMercenaryDataTypeDefinition typeDef in _customMercenaryTypes)
            {
                CharacterObject characterObject = CharacterObject.Find(typeDef.TroopID);
                if (characterObject == null)
                {
                    //_customMercenaryTypes.Remove(typeDef);
                    errorLog += $"[Error]; Troop ID provided is not found!\n > TroopID: {typeDef.TroopID}\n";
                }
                else
                {
                    correctedList.Add(typeDef);
                    BasicCharacterObjects.Add(typeDef.TroopID, characterObject);
                }
            }
            _customMercenaryTypes = correctedList;

            if (!errorLog.IsEmpty())
            {
                GenerateErrorLog();
            }
        }
        public void GenerateDefaultData()
        {
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("legion_of_the_betrayed_tier_1", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("legion_of_the_betrayed_tier_2", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("legion_of_the_betrayed_tier_3", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("embers_of_flame_tier_1", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("embers_of_flame_tier_2", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("embers_of_flame_tier_3", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("eleftheroi_tier_1", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("eleftheroi_tier_2", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("eleftheroi_tier_3", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("hidden_hand_tier_1", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("hidden_hand_tier_2", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("hidden_hand_tier_3", false, new string[] { "empire" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("ghilman_tier_1", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("ghilman_tier_2", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("ghilman_tier_3", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("jawwal_tier_1", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("jawwal_tier_2", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("jawwal_tier_3", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("beni_zilal_tier_1", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("beni_zilal_tier_2", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("beni_zilal_tier_3", false, new string[] { "aserai" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("skolderbrotva_tier_1", false, new string[] { "sturgia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("skolderbrotva_tier_2", false, new string[] { "sturgia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("skolderbrotva_tier_3", false, new string[] { "sturgia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("lakepike_tier_1", false, new string[] { "sturgia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("lakepike_tier_2", false, new string[] { "sturgia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("lakepike_tier_3", false, new string[] { "sturgia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("forest_people_tier_1", false, new string[] { "battania" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("forest_people_tier_2", false, new string[] { "battania" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("forest_people_tier_3", false, new string[] { "battania" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("company_of_the_boar_tier_1", false, new string[] { "vlandia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("company_of_the_boar_tier_2", false, new string[] { "vlandia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("company_of_the_boar_tier_3", false, new string[] { "vlandia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("brotherhood_of_woods_tier_1", false, new string[] { "vlandia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("brotherhood_of_woods_tier_2", false, new string[] { "vlandia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("brotherhood_of_woods_tier_3", false, new string[] { "vlandia" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("karakhuzaits_tier_1", false, new string[] { "khuzait" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("karakhuzaits_tier_2", false, new string[] { "khuzait" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("karakhuzaits_tier_3", false, new string[] { "khuzait" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("wolfskins_tier_1", false, new string[] { "battania" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("wolfskins_tier_2", false, new string[] { "battania" }));
            _customMercenaryTypes.Add(new CustomMercenaryDataTypeDefinition("wolfskins_tier_3", false, new string[] { "battania" }));

            XmlSerializer writer = new XmlSerializer(typeof(CustomMercenaryDataTypeDefinition[]));
            Directory.CreateDirectory(_dataDirectory);
            FileStream fileStream = new FileStream(Path.Combine(_dataDirectory, "MinorTroops.xml"), FileMode.Create);
            writer.Serialize(fileStream, _customMercenaryTypes.ToArray());
            fileStream.Close();
        }
        public void GenerateErrorLog()
        {
            Directory.CreateDirectory(_errorDirectory);
            File.WriteAllText(Path.Combine(_errorDirectory, "error.txt"), errorLog);
        }

        public DataManager() { }
    }

    public struct CustomMercenaryDataTypeDefinition
    {
        public string TroopID;
        public bool IsGlobalSpawn;
        public string[] CultureSpawns;

        public CustomMercenaryDataTypeDefinition(string TroopID, bool IsGlobalSpawn, string[] CultureSpawns)
        {
            this.TroopID = TroopID;
            this.IsGlobalSpawn = IsGlobalSpawn;
            this.CultureSpawns = CultureSpawns;
        }
    }


    public class MercenaryTavernEntity
    {
        [SaveableField(1)]
        public string TroopID;
        [SaveableField(2)]
        public int Count;

        [SaveableField(3)]
        public LocationCharacter LocationChar;


        public LocationCharacter AddCharacterData(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            SimpleAgentOrigin simpleAgentOrigin = new SimpleAgentOrigin(DataManager.Current.BasicCharacterObjects[TroopID], -1, null, default(UniqueTroopDescriptor));
            AgentData agentData = new AgentData(simpleAgentOrigin);
            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(DataManager.Current.BasicCharacterObjects[TroopID].Race, "_settlement");
            LocationCharacter newLocChar = new LocationCharacter(agentData.Monster(monsterWithSuffix).NoHorses(true), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors), "spawnpoint_cust_mercenary", true, relation, null, false, false, null, false, false, true);
            LocationChar = newLocChar;
            return LocationChar;
        }
    }

}
