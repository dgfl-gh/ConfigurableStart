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
        public static StringBuilder tfStartingDU = new StringBuilder("");
        public static StringBuilder rfUnlockedConfigs = new StringBuilder("");
        public static StringBuilder completedContracts = new StringBuilder("");

        [KSPField(isPersistant = true)]
        public bool Initialized = false;
        [KSPField(isPersistant = true)]
        public string CurrentScenarioName;
        [KSPField(isPersistant = true)]
        public string StartingDate;
        [KSPField(isPersistant = true)]
        public string UnlockedTechs;
        [KSPField(isPersistant = true)]
        public string UnlockedParts;
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
        public string TFStartingDU;
        [KSPField(isPersistant = true)]
        public string RFUnlockedConfigs;
        [KSPField(isPersistant = true)]
        public string CompletedContracts;

        public override void OnAwake()
        {
            if (Initialized) return;
            Instance = this;
        }

        public static void UpdateAppliedScenarioFields()
        {
            if (Instance == null) return;

            Instance.CurrentScenarioName = CurrentScenario.ScenarioName;
            Instance.StartingDate = startingDate.ToString();
            Instance.UnlockedTechs = unlockedTechs.ToString();
            //Instance.UnlockedParts = unlockedParts.ToString();
            Instance.FacilityUpgrades = facilitiesUpgraded.ToString();
            Instance.KCTLaunchpads = kctLaunchpads.ToString();
            Instance.TFStartingDU = tfStartingDU.ToString();
            Instance.RFUnlockedConfigs = rfUnlockedConfigs.ToString();
            Instance.StartingFunds = startingFunds.ToString();
            Instance.StartingScience = startingScience.ToString();
            Instance.StartingRep = startingRep.ToString();
            Instance.CompletedContracts = completedContracts.ToString();
            TrimEndingCommas();

            Instance.Initialized = true;
            ResetAppliedScenarioFields();

            Utilities.Log("Updated persistent savegame info");
        }

        private static void TrimEndingCommas()
        {
            Instance.UnlockedTechs = Instance.UnlockedTechs.TrimEnd(',', ' ');
            //Instance.UnlockedParts = Instance.UnlockedParts.TrimEnd(',', ' ');
            Instance.FacilityUpgrades = Instance.FacilityUpgrades.TrimEnd(',', ' ');
            Instance.KCTLaunchpads = Instance.KCTLaunchpads.TrimEnd(',', ' ');
            Instance.TFStartingDU = Instance.TFStartingDU.TrimEnd(',', ' ');
            Instance.RFUnlockedConfigs = Instance.RFUnlockedConfigs.TrimEnd(',', ' ');
            Instance.CompletedContracts = Instance.CompletedContracts.TrimEnd(',', ' ');
        }

        private static void ResetAppliedScenarioFields()
        {
            startingDate.Clear();
            unlockedTechs.Clear();
            //unlockedParts.Clear();
            facilitiesUpgraded.Clear();
            kctLaunchpads.Clear();
            tfStartingDU.Clear();
            rfUnlockedConfigs.Clear();
            startingFunds.Clear();
            startingScience.Clear();
            startingRep.Clear();
            completedContracts.Clear();
        }
    }
}
