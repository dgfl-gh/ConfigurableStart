namespace ConfigurableStart
{
    public class Scenario
    {
        private string scenarioName = null;
        private string description = null;
        private string startingDate = null;
        private string unlockedTechs = null;
        private bool? unlockPartsInParentNodes = null;
        private bool? unlockPartUpgrades = null;
        private string partUnlockFilters = null;
        private string facilityUpgrades = null;
        private string kctLaunchpads = null;
        private bool? kctRemoveDefaultPads = null;
        private string kctUpgrades = null;
        private int? kctUnspentUpgrades = null;
        private string tfStartingDU = null;
        private string rfUnlockedConfigs = null;
        private string completedContracts = null;
        private string acceptedContracts = null;
        private string completedExperiments = null;
        private float? startingFunds = null;
        private float? startingScience = null;
        private float? startingRep = null;

        public string ScenarioName { get => scenarioName; set => scenarioName = value; }
        public string Description { get => description; set => description = value; }
        public string StartingDate { get => startingDate; set => startingDate = value; }
        public string UnlockedTechs { get => unlockedTechs; set => unlockedTechs = value; }
        public bool UnlockPartsInParentNodes { get => unlockPartsInParentNodes ?? true; set => unlockPartsInParentNodes = value; }
        public bool? UnlockPartUpgrades { get => unlockPartUpgrades; set => unlockPartUpgrades = value; }
        public string PartUnlockFilters { get => partUnlockFilters; set => partUnlockFilters = value; }
        public string FacilityUpgrades { get => facilityUpgrades; set => facilityUpgrades = value; }
        public string KCTLaunchpads { get => kctLaunchpads; set => kctLaunchpads = value; }
        public bool KCTRemoveDefaultPads { get => kctRemoveDefaultPads.GetValueOrDefault(true); set => kctRemoveDefaultPads = value; }
        public string KCTUpgrades { get => kctUpgrades; set => kctUpgrades = value; }
        public int? KCTUnspentUpgrades { get => kctUnspentUpgrades; set => kctUnspentUpgrades = value; }
        public string TFStartingDU { get => tfStartingDU; set => tfStartingDU = value; }
        public string RFUnlockedConfigs { get => rfUnlockedConfigs; set => rfUnlockedConfigs = value; }
        public string CompletedContracts { get => completedContracts; set => completedContracts = value; }
        public string AcceptedContracts { get => acceptedContracts; set => acceptedContracts = value; }
        public float? StartingFunds { get => startingFunds; set => startingFunds = value; }
        public float? StartingScience { get => startingScience; set => startingScience = value; }
        public float? StartingRep { get => startingRep; set => startingRep = value; }
        public long StartingUT => string.IsNullOrEmpty(StartingDate) ? 0 : DateHandler.GetUTFromDate(StartingDate.Trim());

        public Scenario(string name)
        {
            scenarioName = name;
        }

        public Scenario(ConfigNode node)
        {
            node.CSTryGetValue("name", out scenarioName);
            node.CSTryGetValue("description", out description);
            node.CSTryGetValue("startingDate", out startingDate);
            node.CSTryGetValue("unlockedTechs", out unlockedTechs);
            node.CSTryGetValue("unlockPartsInParentNodes", out unlockPartsInParentNodes);
            node.CSTryGetValue("unlockPartUpgrades", out unlockPartUpgrades);
            node.CSTryGetValue("partUnlockFilters", out partUnlockFilters);
            node.CSTryGetValue("facilities", out facilityUpgrades);
            node.CSTryGetValue("kctLaunchpads", out kctLaunchpads);
            node.CSTryGetValue("kctUpgrades", out kctUpgrades);
            node.CSTryGetValue("kctUnspentUpgrades", out kctUnspentUpgrades);
            node.CSTryGetValue("tfStartingDU", out tfStartingDU);
            node.CSTryGetValue("rfUnlockedConfigs", out rfUnlockedConfigs);
            node.CSTryGetValue("completedContracts", out completedContracts);
            node.CSTryGetValue("acceptedContracts", out acceptedContracts);
            node.CSTryGetValue("completedExperiments", out completedExperiments);
            node.CSTryGetValue("startingRep", out startingRep);
            node.CSTryGetValue("startingScience", out startingScience);
            node.CSTryGetValue("startingFunds", out startingFunds);
            if (!node.CSTryGetValue("kctRemoveDefaultPads", out kctRemoveDefaultPads))
                kctRemoveDefaultPads = !string.IsNullOrEmpty(kctLaunchpads);
        }
    }
}
