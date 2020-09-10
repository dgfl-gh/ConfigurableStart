using Contracts;
using Expansions.Serenity.RobotArmFX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Upgradeables;

namespace CustomScenarioManager
{
    public class Scenario : MonoBehaviour
    {
        private string scenarioName = null;
        private string description = null;
        private string startingDate = null;
        private string unlockedTechs = null;
        private string completedContracts = null;
        private string facilityUpgrades = null;
        private string kctLaunchpads = null;
        private bool? kctRemoveDefaultPads = null;
        private string tfStartingDU = null;
        private string rfUnlockedConfigs = null;
        private float? startingFunds = null;
        private float? startingScience = null;
        private float? startingRep = null;

        public string ScenarioName { get => scenarioName; set => scenarioName = value; }
        public string Description { get => description; set => description = value; }
        public string StartingDate { get => startingDate; set => startingDate = value; }
        public string UnlockedTechs { get => unlockedTechs; set => unlockedTechs = value; }
        public string CompletedContracts { get => completedContracts; set => completedContracts = value; }
        public string FacilityUpgrades { get => facilityUpgrades; set => facilityUpgrades = value; }
        public string KCTLaunchpads { get => kctLaunchpads; set => kctLaunchpads = value; }
        public bool KCTRemoveDefaltPads
        {
            get => kctRemoveDefaultPads ?? !string.IsNullOrEmpty(kctLaunchpads);
            set => kctRemoveDefaultPads = value;
        }
        public string TFStartingDU { get => tfStartingDU; set => tfStartingDU = value; }
        public string RFUnlockedConfigs { get => rfUnlockedConfigs; set => rfUnlockedConfigs = value; }
        public float? StartingFunds { get => startingFunds; set => startingFunds = value; }
        public float? StartingScience { get => startingScience; set => startingScience = value; }
        public float? StartingRep { get => startingRep; set => startingRep = value; }

        public static Scenario Create(ConfigNode node, GameObject gameObject = null)
        {
            if (gameObject == null)
                gameObject = new GameObject();

            Scenario x = gameObject.AddComponent<Scenario>();

            node.CSMTryGetValue("name", out x.scenarioName);
            node.CSMTryGetValue("description", out x.description);
            node.CSMTryGetValue("startingDate", out x.startingDate);
            node.CSMTryGetValue("unlockedTechs", out x.unlockedTechs);
            node.CSMTryGetValue("completedContracts", out x.completedContracts);
            node.CSMTryGetValue("facilities", out x.facilityUpgrades);
            node.CSMTryGetValue("kctLaunchpads", out x.kctLaunchpads);
            node.CSMTryGetValue("tfStartingDU", out x.tfStartingDU);
            node.CSMTryGetValue("rfUnlockedConfigs", out x.rfUnlockedConfigs);
            node.CSMTryGetValue("startingRep", out x.startingRep);
            node.CSMTryGetValue("startingScience", out x.startingScience);
            node.CSMTryGetValue("startingFunds", out x.startingFunds);

            if (!node.CSMTryGetValue("kctRemoveDefaultPads", out x.kctRemoveDefaultPads))
                x.kctRemoveDefaultPads = !string.IsNullOrEmpty(x.kctLaunchpads);

            return x;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(scenarioName);
        }

        public void UpdateFromSetings()
        {
            startingDate = ScenarioManagerSettings.startingDate;
            unlockedTechs = ScenarioManagerSettings.unlockedTechs;
            //completedContracts = ScenarioManagerSettings.completedContracts;
            facilityUpgrades = ScenarioManagerSettings.facilityUpgrades;
            kctLaunchpads = ScenarioManagerSettings.kctLaunchpads;
            kctRemoveDefaultPads = ScenarioManagerSettings.kctRemoveDefaultPads;
            tfStartingDU = ScenarioManagerSettings.tfStartingDU;
            rfUnlockedConfigs = ScenarioManagerSettings.rfUnlockedConfigs;
            ScenarioManagerSettings.startingFunds.CSMTryParse(out startingFunds);
            ScenarioManagerSettings.startingScience.CSMTryParse(out startingScience);
            ScenarioManagerSettings.startingRep.CSMTryParse(out startingRep);
        }

        public void SetParameters()
        {
            DontDestroyOnLoad(this);
            Utilities.Log($"Applying scenario {scenarioName}");
            StartCoroutine(CheckKSPIntializationAndSetParameters());
        }

        public IEnumerator CheckKSPIntializationAndSetParameters()
        {
            // make sure that everything has initialized properly first
            yield return WaitForInitialization(() => ScenarioUpgradeableFacilities.Instance);
            yield return WaitForInitialization(() => ResearchAndDevelopment.Instance);
            yield return WaitForInitialization(() => Reputation.Instance);
            yield return WaitForInitialization(() => Funding.Instance);
            yield return WaitForInitialization(() => ContractSystem.Instance);
            yield return WaitForInitialization(() => PartLoader.Instance);
            // just to be even safer
            yield return new WaitForEndOfFrame();

            // set date
            if (!string.IsNullOrEmpty(startingDate))
            {
                startingDate.Trim();
                long UT = DateHandler.GetUTFromDate(startingDate);
                SetDate(UT);
            }

            // unlock technologies
            if (!string.IsNullOrEmpty(unlockedTechs))
            {
                string[] techIDs = Utilities.ArrayFromCommaSeparatedList(unlockedTechs);
                UnlockTechnologies(techIDs);
            }

            // complete contracts
            if (!string.IsNullOrEmpty(completedContracts))
            {
                string[] contracts = Utilities.ArrayFromCommaSeparatedList(completedContracts);
                CompleteContracts(contracts);
            }

            // remove flagged contracts

            // set facility levels
            if (!string.IsNullOrEmpty(facilityUpgrades))
            {
                string[] facilities = Utilities.ArrayFromCommaSeparatedList(facilityUpgrades);
                Dictionary<string, int> dict = Utilities.DictionaryFromStringArray<int>(facilities);
                SetFacilityLevels(dict);
            }

            // set KCT launchpads
            if (KCT.Found && !string.IsNullOrEmpty(kctLaunchpads))
            {
                string[] pads = Utilities.ArrayFromCommaSeparatedList(kctLaunchpads);
                Dictionary<string, int> dict = Utilities.DictionaryFromStringArray<int>(pads);
                KCT.CreatePads(dict, kctRemoveDefaultPads.GetValueOrDefault(true));
            }

            // unlock RF engine configs
            if(RealFuels.Found && !string.IsNullOrEmpty(rfUnlockedConfigs))
            {
                string[] configs = Utilities.ArrayFromCommaSeparatedList(rfUnlockedConfigs);
                RealFuels.UnlockEngineConfigs(configs);
            }

            // set starting DU for TF
            if (TestFlight.Found && !string.IsNullOrEmpty(tfStartingDU))
            {
                string[] engines = Utilities.ArrayFromCommaSeparatedList(tfStartingDU);
                Dictionary<string, float> dict = Utilities.DictionaryFromStringArray<float>(engines);
                TestFlight.SetFlightDataForParts(dict);
            }

            // set reputation
            if (startingRep != null)
                SetReputation(startingRep.GetValueOrDefault(HighLogic.CurrentGame.Parameters.Career.StartingReputation));

            // set science points
            if (startingScience != null)
                SetScience(startingScience.GetValueOrDefault(HighLogic.CurrentGame.Parameters.Career.StartingScience));

            // set funds
            if (startingFunds != null)
                SetFunds(startingFunds.GetValueOrDefault(HighLogic.CurrentGame.Parameters.Career.StartingFunds));

            Utilities.Log("Scenario applied");
            yield break;
        }

        /// <summary>
        /// Coroutine that waits until the obj parameter has initialized before breaking.
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns></returns>
        public IEnumerator WaitForInitialization(Func<object> obj)
        {
            while (obj() == null)
            {
                yield return new WaitForFixedUpdate();
            }
            Utilities.Log($"{obj().GetType().Name} initialized");
        }

        /// <summary>
        /// Set the starting date.
        /// </summary>
        /// <param name="newUT"> The time in seconds before or after Epoch 0</param>
        public void SetDate(long newUT)
        {
            Planetarium.SetUniversalTime(newUT);
            Utilities.Log($"Set UT: {newUT}");

            if (RP0.Found)
                RP0.ResetLastMaintenanceUpdate(newUT);
        }

        /// <summary>
        /// Set the funds available to the player. Needs to be done as the last thing so none of the other methods affect it.
        /// </summary>
        /// <param name="funds">How much money should the player have</param>
        public void SetFunds(double funds)
        {
            Funding.Instance.SetFunds(funds, TransactionReasons.Progression);
            Utilities.Log($"Set funds: {funds}");
        }

        /// <summary>
        /// Set the Science amount the player has.
        /// </summary>
        /// <param name="science">How much science the player should have</param>
        public void SetScience(float science)
        {
            ResearchAndDevelopment.Instance.SetScience(science, TransactionReasons.Progression);
            Utilities.Log($"Set science: {science}");
        }

        /// <summary>
        /// Set the amout of Reputation the player has
        /// </summary>
        /// <param name="rep">How much reputation the player should have</param>
        public void SetReputation(float rep)
        {
            Reputation.Instance.SetReputation(rep, TransactionReasons.Progression);
            Utilities.Log($"Set reputation: {rep}");
        }

        /// <summary>
        /// Complete each contract specified in the config.
        /// </summary>
        /// <param name="contractsNames"></param>
        public void CompleteContracts(string[] contractsNames)
        {
            foreach (var node in GameDatabase.Instance.GetConfigNodes("CONTRACT_TYPE"))
            {
                string title = null;
                if (node.TryGetValue("title", ref title) && contractsNames.Contains(title))
                {
                    foreach (var reqNode in node.GetNodes("REQUIREMENT") ?? new ConfigNode[] { })
                    {
                        string type = null;
                        if (reqNode.TryGetValue("type", ref type) && type == "CompleteContract")
                        {
                            CompleteContracts(new string[] { reqNode.GetValue("contractType") });
                        }
                    }
                    // actually complete the contract (needs CC?)
                }
            }
        }

        /// <summary>
        /// Set the level of each facility specified in the config.
        /// </summary>
        /// <param name="facilityKeyValuePairs"></param>
        public void SetFacilityLevels(Dictionary<string, int> facilityKeyValuePairs)
        {
            foreach (var facility in facilityKeyValuePairs.Keys)
            {
                int level = facilityKeyValuePairs[facility];
                SetFacilityLevel(new KeyValuePair<string, int>(facility, level));
            }
        }

        /// <summary>
        /// Set the level of a facility.
        /// </summary>
        /// <param name="kvp">key = Facility Name <br> value = Facility Level</br></param>
        public void SetFacilityLevel(KeyValuePair<string, int> kvp)
        {
            foreach (UpgradeableFacility facility in FindObjectsOfType<UpgradeableFacility>())
            {
                string id = kvp.Key.ToUpper() switch
                {
                    "VAB" => "SpaceCenter/VehicleAssemblyBuilding",
                    "SPH" => "SpaceCenter/SpaceplaneHangar",
                    "RUNWAY" => "SpaceCenter/Runway",
                    "R&D" => "SpaceCenter/ResearchAndDevelopment",
                    "RD" => "SpaceCenter/ResearchAndDevelopment",
                    "RESEARCH" => "SpaceCenter/ResearchAndDevelopment",
                    "ASTRONAUT" => "SpaceCenter/AstronautComplex",
                    "TRACKING" => "SpaceCenter/TrackingStation",
                    "MISSION" => "SpaceCenter/MissionControl",
                    "PAD" => "SpaceCenter/LaunchPad",
                    "LAUNCHPAD" => "SpaceCenter/LaunchPad",
                    "ADMIN" => "SpaceCenter/Administration",
                    _ => "SpaceCenter/" + kvp.Key,
                };

                if (id != null && facility.id == id)
                {
                    int level = kvp.Value;
                    level = Mathf.Clamp(level, 0, facility.MaxLevel);
                    facility.SetLevel(level);
                    Utilities.Log($"Set {facility.name} level: {level}");
                    break;
                }
            }
        }

        /// <summary>
        /// Iterates through each tech that is defined in the Config and calls UnlockTech()
        /// </summary>
        /// <param name="techID"></param>
        public void UnlockTechnologies(IEnumerable<string> techID)
        {
            AssetBase.RnDTechTree.ReLoad();
            foreach (string tech in techID)
            {
                UnlockTechWithParents(tech);
            }
        }

        /// <summary>
        /// Unlocks a technology and all of its parents from its techID.
        /// </summary>
        /// <param name="techID"></param>
        public void UnlockTechWithParents(string techID, List<ProtoRDNode> researchedNodes = null)
        {
            researchedNodes ??= new List<ProtoRDNode>();
            var rdNodes = AssetBase.RnDTechTree.GetTreeNodes().ToList();

            //for some reason, this is not a static method and you need a reference
            var rdNode = rdNodes[0].FindNodeByID(techID, rdNodes);

            foreach (var parentNode in rdNode.parents ?? Enumerable.Empty<ProtoRDNode>())
            {
                if (!researchedNodes.Contains(parentNode))
                    UnlockTechWithParents(parentNode, researchedNodes);
            }

            UnlockTech(techID);
            researchedNodes.Add(rdNode);
        }

        /// <summary>
        /// Unlocks a technology and all of its parents.
        /// </summary>
        /// <param name="protoRDNode"></param>
        public void UnlockTechWithParents(ProtoRDNode protoRDNode, List<ProtoRDNode> researchedNodes = null)
        {
            researchedNodes ??= new List<ProtoRDNode>();

            foreach (var parentNode in protoRDNode.parents ?? Enumerable.Empty<ProtoRDNode>())
            {
                if (!researchedNodes.Contains(parentNode))
                    UnlockTechWithParents(parentNode, researchedNodes);
            }

            UnlockTech(protoRDNode.tech);
            researchedNodes.Add(protoRDNode);
        }

        /// <summary>
        /// Unlocks a technology and all of the parts inside of the node. Includes handling for Part Unlock Costs. Code derived from Contract Configurator.
        /// </summary>
        /// <param name="techID"> The techID of the node to unlock</param>
        public void UnlockTech(string techID)
        {
            ProtoTechNode ptn = new ProtoTechNode();
            ptn.state = RDTech.State.Available;
            ptn.techID = techID;
            ptn.scienceCost = 9999;

            if (!HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch)
            {
                ptn.partsPurchased = PartLoader.Instance.loadedParts.Where(p => p.TechRequired == techID).ToList();
            }
            else
            {
                ptn.partsPurchased = new List<AvailablePart>();
            }

            ResearchAndDevelopment.Instance.SetTechState(techID, ptn);
            Utilities.Log($"Unlocked tech: {techID}");
        }

        /// <summary>
        /// Unlocks a technology and all of the parts inside of the node. Includes handling for Part Unlock Costs. Code derived from Contract Configurator.
        /// </summary>
        /// <param name="ptn"> The node to unlock</param>
        public void UnlockTech(ProtoTechNode ptn)
        {
            ptn.state = RDTech.State.Available;
            string techID = ptn.techID;

            if (!HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch)
            {
                ptn.partsPurchased = PartLoader.Instance.loadedParts.Where(p => p.TechRequired == techID).ToList();
            }
            else
            {
                ptn.partsPurchased = new List<AvailablePart>();
            }

            ResearchAndDevelopment.Instance.SetTechState(techID, ptn);
            Utilities.Log($"Unlocked tech: {techID}");
        }
    }
}
