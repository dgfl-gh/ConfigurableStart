using Contracts;
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
        private bool kctRemoveDefaultPads;
        private float startingFunds;
        private float startingScience;
        private float startingRep;

        public string ScenarioName { get => scenarioName; set => scenarioName = value; }
        public string Description { get => description; set => description = value; }
        public string StartingDate { get => startingDate; set => startingDate = value; }
        public string UnlockedTechs { get => unlockedTechs; set => unlockedTechs = value; }
        public string CompletedContracts { get => completedContracts; set => completedContracts = value; }
        public string FacilityUpgrades { get => facilityUpgrades; set => facilityUpgrades = value; }
        public string KCTLaunchpads { get => kctLaunchpads; set => kctLaunchpads = value; }
        public bool KCTRemoveDefaltPads { get => kctRemoveDefaultPads; set => kctRemoveDefaultPads = value; }
        public float StartingFunds { get => startingFunds; set => startingFunds = value; }
        public float StartingScience { get => startingScience; set => startingScience = value; }
        public float StartingRep { get => startingRep; set => startingRep = value; }

        public static Scenario Create(ConfigNode node, GameObject gameObject = null)
        {
            if (gameObject == null)
                gameObject = new GameObject();

            Scenario x = gameObject.AddComponent<Scenario>();

            node.TryGetValue("name", ref x.scenarioName);
            node.TryGetValue("description", ref x.description);
            node.TryGetValue("startingDate", ref x.startingDate);
            node.TryGetValue("unlockedTechs", ref x.unlockedTechs);
            node.TryGetValue("completedContracts", ref x.completedContracts);
            node.TryGetValue("facilities", ref x.facilityUpgrades);
            node.TryGetValue("kctLaunchpads", ref x.kctLaunchpads);

            if (!node.TryGetValue("kctRemoveDefaultPads", ref x.kctRemoveDefaultPads))
                x.kctRemoveDefaultPads = !string.IsNullOrEmpty(x.kctLaunchpads);

            if (!node.TryGetValue("startingRep", ref x.startingRep))
                x.startingRep = HighLogic.CurrentGame.Parameters.Career.StartingReputation;

            if (!node.TryGetValue("startingScience", ref x.startingScience))
                x.startingScience = HighLogic.CurrentGame.Parameters.Career.StartingScience;

            if (!node.TryGetValue("startingFunds", ref x.startingFunds))
                x.startingFunds = HighLogic.CurrentGame.Parameters.Career.StartingFunds;

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
            float.TryParse(ScenarioManagerSettings.startingFunds, out startingFunds);
            float.TryParse(ScenarioManagerSettings.startingScience, out startingScience);
            float.TryParse(ScenarioManagerSettings.startingRep, out startingRep);
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
                Dictionary<string, int> dict = Utilities.DictionaryFromStringArray(facilities);
                SetFacilityLevels(dict);
            }

            // set KCT launchpads
            if (KCT.Found && !string.IsNullOrEmpty(kctLaunchpads))
            {
                string[] pads = Utilities.ArrayFromCommaSeparatedList(kctLaunchpads);
                Dictionary<string, int> dict = Utilities.DictionaryFromStringArray(pads);
                KCT.CreatePads(dict, kctRemoveDefaultPads);
            }

            // set starting DU for TF

            // set reputation
            SetReputation(startingRep);

            // set science points
            SetScience(startingScience);

            // set funds
            SetFunds(startingFunds);

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
            foreach (Contract contract in ContractSystem.Instance.Contracts)
            {
                if (contractsNames.Contains(contract.Title))
                {
                    CompleteContracts(new string[] { contract.Root.Title });
                    contract.Accept();
                    contract.Complete();

                    break;
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
        public void UnlockTechWithParents(string techID)
        {
            AssetBase.RnDTechTree.FindTech(techID);
            var rdNodes = AssetBase.RnDTechTree.GetTreeNodes().ToList();
            //for some reason, this is not a static method and you need a reference
            var rdNode = rdNodes[0].FindNodeByID(techID, rdNodes);
            foreach (var parentNode in rdNode.parents ?? Enumerable.Empty<ProtoRDNode>())
            {
                UnlockTechWithParents(parentNode);
            }
            UnlockTech(techID);
        }

        /// <summary>
        /// Unlocks a technology and all of its parents.
        /// </summary>
        /// <param name="protoRDNode"></param>
        public void UnlockTechWithParents(ProtoRDNode protoRDNode)
        {
            foreach (var parentNode in protoRDNode.parents ?? Enumerable.Empty<ProtoRDNode>())
            {
                UnlockTechWithParents(parentNode);
            }
            UnlockTech(protoRDNode.tech.techID);
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

            if (HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch)
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

            if (HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch)
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
