using System.Text;

namespace CustomScenarioManager
{
    [KSPScenario(ScenarioCreationOptions.AddToNewCareerGames | ScenarioCreationOptions.AddToNewScienceSandboxGames, GameScenes.SPACECENTER)]
    public class CustomScenarioData : ScenarioModule
    {
        public static CustomScenarioData Instance;
        public static Scenario CurrentScenario => ScenarioLoader.CurrentScenario;
        public static StringBuilder startingDate = new StringBuilder("");
        public static StringBuilder unlockedTechs = new StringBuilder("");
        //public static StringBuilder unlockedParts = new StringBuilder("");
        public static StringBuilder facilitiesUpgraded = new StringBuilder("");
        public static StringBuilder startingFunds = new StringBuilder("");
        public static StringBuilder startingScience = new StringBuilder("");
        public static StringBuilder startingRep = new StringBuilder("");
        public static StringBuilder kctLaunchpads = new StringBuilder("");
        public static StringBuilder kctUpgrades = new StringBuilder("");
        public static StringBuilder kctUnspentUpgrades = new StringBuilder("");
        public static StringBuilder tfStartingDU = new StringBuilder("");
        public static StringBuilder rfUnlockedConfigs = new StringBuilder("");
        public static StringBuilder completedContracts = new StringBuilder("");
        public static StringBuilder acceptedContracts = new StringBuilder("");

        [KSPField(isPersistant = true)]
        public bool Initialized = false;
        [KSPField(isPersistant = true)]
        public string CurrentScenarioName;
        [KSPField(isPersistant = true)]
        public string StartingDate;
        [KSPField(isPersistant = true)]
        public string UnlockedTechs;
        //[KSPField(isPersistant = true)]
        //public string UnlockedParts;
        [KSPField(isPersistant = true)]
        public string FacilityUpgrades;
        [KSPField(isPersistant = true)]
        public string StartingFunds;
        [KSPField(isPersistant = true)]
        public string StartingScience;
        [KSPField(isPersistant = true)]
        public string StartingRep;
        [KSPField(isPersistant = true)]
        public string KCTLaunchpads;
        [KSPField(isPersistant = true)]
        public string KCTUpgrades;
        [KSPField(isPersistant = true)]
        public string KCTUnspentUpgrades;
        [KSPField(isPersistant = true)]
        public string TFStartingDU;
        [KSPField(isPersistant = true)]
        public string RFUnlockedConfigs;
        [KSPField(isPersistant = true)]
        public string CompletedContracts;
        [KSPField(isPersistant = true)]
        public string AcceptedContracts;

        public override void OnAwake()
        {
            if (Initialized) return;
            Instance = this;
        }

        public static void UpdateAppliedScenarioFields()
        {
            if (Instance == null) return;

            Instance.CurrentScenarioName = !string.IsNullOrWhiteSpace(CurrentScenario.ScenarioName) ?
                CurrentScenario.ScenarioName : "null";
            Instance.StartingDate = !string.IsNullOrWhiteSpace(startingDate.ToString()) ?
                startingDate.ToString() : "null";
            Instance.UnlockedTechs = !string.IsNullOrWhiteSpace(unlockedTechs.ToString()) ?
                unlockedTechs.ToString() : "null";
            //Instance.UnlockedParts = !string.IsNullOrWhiteSpace(unlockedParts.ToString();
            //    unlockedParts.ToString() : "null";
            Instance.FacilityUpgrades = !string.IsNullOrWhiteSpace(facilitiesUpgraded.ToString()) ?
                facilitiesUpgraded.ToString() : "null";
            Instance.KCTLaunchpads = !string.IsNullOrWhiteSpace(kctLaunchpads.ToString()) ?
                kctLaunchpads.ToString() : "null";
            Instance.KCTUpgrades = !string.IsNullOrWhiteSpace(kctUpgrades.ToString()) ?
                kctUpgrades.ToString() : "null";
            Instance.KCTUnspentUpgrades = !string.IsNullOrWhiteSpace(kctUnspentUpgrades.ToString()) ?
                kctUnspentUpgrades.ToString() : "null";
            Instance.TFStartingDU = !string.IsNullOrWhiteSpace(tfStartingDU.ToString()) ?
                tfStartingDU.ToString() : "null";
            Instance.RFUnlockedConfigs = !string.IsNullOrWhiteSpace(rfUnlockedConfigs.ToString()) ?
                rfUnlockedConfigs.ToString() : "null";
            Instance.StartingFunds = !string.IsNullOrWhiteSpace(startingFunds.ToString()) ?
                startingFunds.ToString() : "null";
            Instance.StartingScience = !string.IsNullOrWhiteSpace(startingScience.ToString()) ?
                startingScience.ToString() : "null";
            Instance.StartingRep = !string.IsNullOrWhiteSpace(startingRep.ToString()) ?
                startingRep.ToString() : "null";
            Instance.CompletedContracts = !string.IsNullOrWhiteSpace(completedContracts.ToString()) ?
                completedContracts.ToString() : "null";
            Instance.AcceptedContracts = !string.IsNullOrWhiteSpace(acceptedContracts.ToString()) ?
                acceptedContracts.ToString() : "null";

            TrimEndingCommas();

            Instance.Initialized = true;

            Utilities.Log("Updated persistent savegame info");
        }

        private static void TrimEndingCommas()
        {
            Instance.UnlockedTechs = Instance.UnlockedTechs.TrimEnd(',', ' ');
            //Instance.UnlockedParts = Instance.UnlockedParts.TrimEnd(',', ' ');
            Instance.FacilityUpgrades = Instance.FacilityUpgrades.TrimEnd(',', ' ');
            Instance.KCTLaunchpads = Instance.KCTLaunchpads.TrimEnd(',', ' ');
            Instance.KCTUpgrades = Instance.KCTUpgrades.TrimEnd(',', ' ');
            Instance.TFStartingDU = Instance.TFStartingDU.TrimEnd(',', ' ');
            Instance.RFUnlockedConfigs = Instance.RFUnlockedConfigs.TrimEnd(',', ' ');
            Instance.CompletedContracts = Instance.CompletedContracts.TrimEnd(',', ' ');
            Instance.AcceptedContracts = Instance.AcceptedContracts.TrimEnd(',', ' ');
        }
    }
}
