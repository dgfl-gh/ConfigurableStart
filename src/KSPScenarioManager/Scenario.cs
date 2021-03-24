using ContractConfigurator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private bool? unlockPartsInParentNodes = null;
        private bool? unlockPartUpgrades = null;
        private string partUnlockFilters = null;
        private string facilityUpgrades = null;
        private string kctLaunchpads = null;
        private bool? kctRemoveDefaultPads = null;
        private string tfStartingDU = null;
        private string rfUnlockedConfigs = null;
        private string completedContracts = null;
        private float? startingFunds = null;
        private float? startingScience = null;
        private float? startingRep = null;

        public string ScenarioName { get => scenarioName; set => scenarioName = value; }
        public string Description { get => description; set => description = value; }
        public string StartingDate { get => startingDate; set => startingDate = value; }
        public string UnlockedTechs { get => unlockedTechs; set => unlockedTechs = value; }
        public bool UnlockPartsInParentNodes
        {
            get => unlockPartsInParentNodes ?? true;
            set => unlockPartsInParentNodes = value;
        }
        public string PartUnlockFilters { get => partUnlockFilters; set => partUnlockFilters = value; }
        public string FacilityUpgrades { get => facilityUpgrades; set => facilityUpgrades = value; }
        public string KCTLaunchpads { get => kctLaunchpads; set => kctLaunchpads = value; }
        public bool KCTRemoveDefaltPads
        {
            get => kctRemoveDefaultPads ?? !string.IsNullOrEmpty(kctLaunchpads);
            set => kctRemoveDefaultPads = value;
        }
        public string TFStartingDU { get => tfStartingDU; set => tfStartingDU = value; }
        public string RFUnlockedConfigs { get => rfUnlockedConfigs; set => rfUnlockedConfigs = value; }
        public string CompletedContracts { get => completedContracts; set => completedContracts = value; }
        public float? StartingFunds { get => startingFunds; set => startingFunds = value; }
        public float? StartingScience { get => startingScience; set => startingScience = value; }
        public float? StartingRep { get => startingRep; set => startingRep = value; }

        public long StartingUT => string.IsNullOrEmpty(startingDate) ? 0 : DateHandler.GetUTFromDate(startingDate);

        // cache the contract nodes
        private static readonly Dictionary<string, ConfigNode> contractNodes = new Dictionary<string, ConfigNode>();

        public static Scenario Create(ConfigNode node, GameObject gameObject = null)
        {
            if (gameObject == null)
                gameObject = new GameObject();

            Scenario x = gameObject.AddComponent<Scenario>();

            node.CSMTryGetValue("name", out x.scenarioName);
            node.CSMTryGetValue("description", out x.description);
            node.CSMTryGetValue("startingDate", out x.startingDate);
            node.CSMTryGetValue("unlockedTechs", out x.unlockedTechs);
            node.CSMTryGetValue("unlockPartsInParentNodes", out x.unlockPartsInParentNodes);
            node.CSMTryGetValue("unlockPartUpgrades", out x.unlockPartUpgrades);
            node.CSMTryGetValue("partUnlockFilters", out x.partUnlockFilters);
            node.CSMTryGetValue("facilities", out x.facilityUpgrades);
            node.CSMTryGetValue("kctLaunchpads", out x.kctLaunchpads);
            node.CSMTryGetValue("tfStartingDU", out x.tfStartingDU);
            node.CSMTryGetValue("rfUnlockedConfigs", out x.rfUnlockedConfigs);
            node.CSMTryGetValue("completedContracts", out x.completedContracts);
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

        public void UpdateFromSettings()
        {
            startingDate = ScenarioEditorGUI.startingDate;
            unlockedTechs = ScenarioEditorGUI.unlockedTechs;
            unlockPartsInParentNodes = ScenarioEditorGUI.unlockPartsInParentNodes;
            partUnlockFilters = ScenarioEditorGUI.partUnlockFilters;
            facilityUpgrades = ScenarioEditorGUI.facilityUpgrades;
            kctLaunchpads = ScenarioEditorGUI.kctLaunchpads;
            kctRemoveDefaultPads = ScenarioEditorGUI.kctRemoveDefaultPads;
            tfStartingDU = ScenarioEditorGUI.tfStartingDU;
            rfUnlockedConfigs = ScenarioEditorGUI.rfUnlockedConfigs;
            ScenarioEditorGUI.startingFunds.CSMTryParse(out startingFunds);
            ScenarioEditorGUI.startingScience.CSMTryParse(out startingScience);
            ScenarioEditorGUI.startingRep.CSMTryParse(out startingRep);
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
            yield return WaitForInitialization(() => PartLoader.Instance);
            yield return WaitForInitialization(() => Contracts.ContractSystem.Instance);
            if (RP0.Found) yield return WaitForInitialization(() => RP0.Instance);
            if (TestFlight.Found) yield return WaitForInitialization(() => TestFlight.Instance);

            // just to be even safer
            yield return new WaitForEndOfFrame();

            // complete contracts
            if (!string.IsNullOrEmpty(completedContracts))
            {
                string[] contractNames = Utilities.ArrayFromCommaSeparatedList(completedContracts);
                CompleteContracts(contractNames);
            }

            // set date
            if (!string.IsNullOrEmpty(startingDate))
            {
                startingDate = startingDate.Trim();
                long UT = DateHandler.GetUTFromDate(startingDate);
                SetDate(UT);
            }

            // unlock technologies
            if (!string.IsNullOrEmpty(unlockedTechs))
            {
                Dictionary<string, bool> techIDs = Utilities.DictionaryFromString(unlockedTechs, defaultValue: true);
                
                Dictionary<string, string> unlockFilters;
                unlockFilters = string.IsNullOrEmpty(partUnlockFilters) ? 
                    null : Utilities.DictionaryFromString<string>(partUnlockFilters, defaultValue: null);

                UnlockTechnologies(techIDs, unlockFilters, unlockPartUpgrades);
            }

            // set facility levels
            if (!string.IsNullOrEmpty(facilityUpgrades))
            {
                Dictionary<string, int> facilities = Utilities.DictionaryFromString<int>(facilityUpgrades);
                SetFacilityLevels(facilities);
            }

            // set KCT launchpads
            if (KCT.Found && !string.IsNullOrEmpty(kctLaunchpads))
            {
                Dictionary<string, int> pads = Utilities.DictionaryFromString<int>(kctLaunchpads);
                KCT.CreatePads(pads, kctRemoveDefaultPads.GetValueOrDefault(true));
            }

            // unlock RF engine configs
            if (RealFuels.Found && !string.IsNullOrEmpty(rfUnlockedConfigs))
            {
                string[] configs = Utilities.ArrayFromCommaSeparatedList(rfUnlockedConfigs);
                RealFuels.UnlockEngineConfigs(configs);
            }

            // set starting DU for TF
            if (TestFlight.Found && !string.IsNullOrEmpty(tfStartingDU))
            {
                Dictionary<string, float> engines = Utilities.DictionaryFromString<float>(tfStartingDU);
                TestFlight.SetFlightDataForParts(engines);
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

            CustomScenarioData.UpdateAppliedScenarioFields();
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
            CustomScenarioData.startingDate.Append(DateHandler.GetFormattedDateString(newUT));
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
            CustomScenarioData.startingFunds.Append(funds);
        }

        /// <summary>
        /// Set the Science amount the player has.
        /// </summary>
        /// <param name="science">How much science the player should have</param>
        public void SetScience(float science)
        {
            ResearchAndDevelopment.Instance.SetScience(science, TransactionReasons.Progression);
            Utilities.Log($"Set science: {science}");
            CustomScenarioData.startingScience.Append(science);
        }

        /// <summary>
        /// Set the amout of Reputation the player has
        /// </summary>
        /// <param name="rep">How much reputation the player should have</param>
        public void SetReputation(float rep)
        {
            Reputation.Instance.SetReputation(rep, TransactionReasons.Progression);
            Utilities.Log($"Set reputation: {rep}");
            CustomScenarioData.startingRep.Append(rep);
        }

        /// <summary>
        /// Set the level of each facility specified in the config.
        /// </summary>
        /// <param name="facilityKeyValuePairs"></param>
        public void SetFacilityLevels(Dictionary<string, int> facilityKeyValuePairs)
        {
            foreach (var facility in facilityKeyValuePairs.Keys)
            {
                int level = facilityKeyValuePairs[facility] - 1;
                SetFacilityLevel(facility, level);
            }
        }

        /// <summary>
        /// Set the level of a facility.
        /// </summary>
        /// <param name="id"> the id of the facility</param>
        /// <param name="level"> the new level of the facility</param>
        public void SetFacilityLevel(string id, int level)
        {
            if (id == null) return;

            id = id.ToUpper() switch
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
                _ => "SpaceCenter/" + id,
            };

            foreach (UpgradeableFacility facility in FindObjectsOfType<UpgradeableFacility>())
            {
                if (facility.id == id)
                {
                    level = Mathf.Clamp(level, 0, facility.MaxLevel);
                    facility.SetLevel(level);
                    Utilities.Log($"Upgraded {facility.name} to level {++level}");
                    CustomScenarioData.facilitiesUpgraded.Append($"{facility.name}@{level},");
                    break;
                }
            }
        }

        /// <summary>
        /// Iterates through each tech that is defined in the Config and calls UnlockTech()
        /// </summary>
        /// <param name="techIDs"></param>
        public void UnlockTechnologies(Dictionary<string, bool> techIDs, Dictionary<string, string> unlockFilters, bool? unlockPartUpgrades)
        {
            var researchedNodes = new List<ProtoRDNode>();

            AssetBase.RnDTechTree.ReLoad();
            foreach (string tech in techIDs.Keys)
            {
                UnlockTechFromTechID(tech, researchedNodes, techIDs[tech], unlockPartUpgrades, false, unlockFilters);
            }
        }

        /// <summary>
        /// Find a ProtoRDNode from its techID and unlocks it, along with all its parents.
        /// </summary>
        /// <param name="techID"></param>
        public void UnlockTechFromTechID(string techID, List<ProtoRDNode> researchedNodes, bool unlockParts, bool? unlockPartUpgrades, bool isRecursive, Dictionary<string, string> unlockFilters)
        {
            if (string.IsNullOrEmpty(techID)) return;

            //for some reason, FindNodeByID is not a static method and you need a reference
            List<ProtoRDNode> rdNodes = AssetBase.RnDTechTree.GetTreeNodes().ToList();
            if (rdNodes[0].FindNodeByID(techID, rdNodes) is ProtoRDNode rdNode)
            {
                UnlockTechWithParents(rdNode, researchedNodes, unlockParts, unlockPartUpgrades, isRecursive, unlockFilters);
            }
            else
                Utilities.LogWrn($"{techID} node not found");
        }

        /// <summary>
        /// Unlock a technology and all of its parents.
        /// </summary>
        /// <param name="protoRDNode"></param>
        public void UnlockTechWithParents(ProtoRDNode protoRDNode, List<ProtoRDNode> researchedNodes, bool unlockParts, bool? unlockPartUpgrades, bool isRecursive, Dictionary<string, string> partUnlockFilters)
        {
            foreach (var parentNode in protoRDNode.parents ?? Enumerable.Empty<ProtoRDNode>())
            {
                if (!researchedNodes.Contains(parentNode))
                    UnlockTechWithParents(parentNode, researchedNodes, unlockParts, unlockPartUpgrades, true, partUnlockFilters);
            }

            bool b = unlockParts && (!isRecursive || (isRecursive && UnlockPartsInParentNodes));
            UnlockTech(protoRDNode.tech, b, unlockPartUpgrades, partUnlockFilters);
            researchedNodes.Add(protoRDNode);
        }

        /// <summary>
        /// Unlock a technology and all of the parts inside of the node. Includes handling for Part Unlock Costs. Code derived from Contract Configurator.
        /// Unlock parts if
        /// </summary>
        /// <param name="ptn"> The node to unlock</param>
        /// <param name="unlockFilters"> The dict of fields to either unlock or non-unlock parts from the node, according to unlockFilters.</param>
        /// <param name="unlockParts"> The bool that dictates the default unlock behaviour. If true, unlock all parts except those that match unlockFilters
        /// <param name="unlockPartUpgrades"> Whether to unlock part upgrades in the tech node</param>
        /// If false, only unlock the parts that match unlockFilters. </param>
        public void UnlockTech(ProtoTechNode ptn, bool unlockParts, bool? unlockPartUpgrades, Dictionary<string, string> unlockFilters)
        {
            ptn.state = RDTech.State.Available;
            string techID = ptn.techID;

            if (!HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch)
            {
                ptn.partsPurchased = MatchingParts(techID, unlockParts, unlockFilters);
            }
            else
            {
                ptn.partsPurchased = new List<AvailablePart>();
            }

            ResearchAndDevelopment.Instance.SetTechState(techID, ptn);
            CustomScenarioData.unlockedTechs.Append(techID + ",");
            Utilities.Log($"Unlocked tech: {techID}");

            //ptn.partsPurchased.ForEach(p => CustomScenarioData.unlockedParts.Append(p.title + ","));

            if(unlockPartUpgrades.HasValue ? unlockPartUpgrades.Value : unlockParts)
            {
                var upgrades = PartUpgradeManager.Handler.GetUpgradesForTech(techID);
                foreach(var upgrd in upgrades)
                {
                    PartUpgradeManager.Handler.SetUnlocked(upgrd.name, true);
                }
            }
        }

        /// <summary>
        /// Unlock the parts contained in a tech node, blacklisting or whitelisting them based on input arguments.
        /// </summary>
        /// <param name="techID"> The techID of the node containing the parts.</param>
        /// <param name="defaultUnlockParts"> Wheter the default behaviour is to buy or not buy parts.</param>
        /// <param name="unlockFilters"> The dictionary&lt;string, string&gt; of field,value kvp that either selects the parts to buy or to not buy,
        /// depending on default behaviour.</param>
        /// <returns></returns>
        public List<AvailablePart> MatchingParts(string techID, bool defaultUnlockParts, Dictionary<string, string> unlockFilters)
        {
            var parts = new List<AvailablePart>();
            unlockFilters ??= new Dictionary<string, string>(0);

            if (defaultUnlockParts)
            {
                parts = PartLoader.Instance.loadedParts.Where(p => p.TechRequired == techID).ToList();

                foreach (var filterName in unlockFilters.Keys)
                {
                    string fieldValue = unlockFilters[filterName];

                    if (fieldValue != null)
                        parts.RemoveAll(p => p.GetType().GetField(filterName)?.GetValue(p).ToString() == fieldValue);
                    else
                        parts.RemoveAll(p => p.partConfig.HasNode(filterName));
                }
            }
            else
            {
                foreach (var filterName in unlockFilters.Keys)
                {
                    string fieldValue = unlockFilters[filterName];

                    if (fieldValue != null)
                        parts.AddRange(PartLoader.Instance.loadedParts
                            .Where(p => p.TechRequired == techID)
                            .Where(p => p.GetType().GetField(filterName)?.GetValue(p).ToString() == fieldValue));
                    else
                        parts.AddRange(PartLoader.Instance.loadedParts
                            .Where(p => p.TechRequired == techID)
                            .Where(p => p.partConfig.HasNode(filterName)));
                }
            }

            return parts;
        }

        /// <summary>
        /// Generates and completes an array of ContractConfigurator contracts
        /// </summary>
        /// <param name="names"> Array of contract names that will be completed.</param>
        public void CompleteContracts(string[] names)
        {
            List<ContractType> contractTypes = ContractType.AllValidContractTypes.ToList();

            foreach (var subType in contractTypes)
            {
                foreach (string contractName in names)
                {
                    if (contractName == subType.name)
                    {
                        var contract = ForceGenerate(subType, 0, new System.Random().Next(), Contracts.Contract.State.Active);

                        if (contract is null)
                        {
                            Utilities.LogWrn($"Couldn't complete contract {contractName}");
                            continue;
                        }

                        StartCoroutine(CompleteContractCoroutine(contract));
                    }
                }
            }
        }

        private static ConfiguredContract ForceGenerate(ContractType contractType, Contracts.Contract.ContractPrestige difficulty, int seed, Contracts.Contract.State state)
        {
            var contract = (ConfiguredContract)Activator.CreateInstance(typeof(ConfiguredContract));

            Type baseT = contract.GetType().BaseType;
            FieldInfo[] fields = baseT.GetFields(BindingFlags.FlattenHierarchy
                | BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.Public);

            // generate and set guid
            foreach (var f in fields)
            {
                if (f.FieldType == typeof(Guid))
                {
                    f.SetValue(contract, Guid.NewGuid());
                    break;
                }
            }
            // set necessary base contract fields
            if (baseT.GetField("prestige", BindingFlags.NonPublic | BindingFlags.Instance) is FieldInfo fi)
                fi.SetValue(contract, difficulty);
            if ((fi = baseT.GetField("state", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, state);
            if ((fi = baseT.GetField("agent", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, Contracts.Agents.AgentList.Instance.GetSuitableAgentForContract(contract));
            contract.FundsFailure = Math.Max(contract.FundsFailure, contract.FundsAdvance);
            contract.GetType().GetMethod("SetupID", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(contract, null);

            // set CC contract subtype
            contract.contractType = contractType;
            contract.subType = contractType.name;

            // Copy text from contract type
            Type t = contract.GetType();
            if ((fi = t.GetField("title", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, contractType.title);
            if ((fi = t.GetField("synopsis", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, contractType.synopsis);
            if ((fi = t.GetField("completedMessage", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, contractType.completedMessage);
            if ((fi = t.GetField("notes", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, contractType.notes);

            return contract;
        }

        private IEnumerator CompleteContractCoroutine(ConfiguredContract c)
        {
            // cache contract nodes
            if(contractNodes.Count == 0)
            {
                var cfgNodes = GameDatabase.Instance.GetConfigNodes("CONTRACT_TYPE");

                foreach (var node in cfgNodes)
                {
                    if(node.GetValue("name") is string subT)
                        contractNodes.Add(subT, node);
                }
            }

            // load behaviours so that they're correctly fired
            if(contractNodes.ContainsKey(c.subType))
            {
                var cfgNode = contractNodes[c.subType];

                if(cfgNode.GetNodes("BEHAVIOUR") is var bNodes)
                {
                    var behaviourFactories = new List<BehaviourFactory>();

                    foreach (var bNode in bNodes)
                    {
                        BehaviourFactory behaviourFactory;
                        BehaviourFactory.GenerateBehaviourFactory(bNode, c.contractType, out behaviourFactory);
                        if (behaviourFactory != null)
                        {
                            behaviourFactories.Add(behaviourFactory);
                        }
                    }

                    if(BehaviourFactory.GenerateBehaviours(c, behaviourFactories))
                        Utilities.Log($"Generated Behaviours for contract {c.subType}");
                }
            }

            // now, complete the contract step by step
            yield return new WaitForFixedUpdate();
            if(c.Offer())
                yield return new WaitForFixedUpdate();
            
            if(c.Accept())
                yield return new WaitForFixedUpdate();

            c.Complete();
            yield return new WaitForFixedUpdate();

            Contracts.ContractSystem.Instance.ContractsFinished.Add(c);
            CustomScenarioData.completedContracts.Append(c.subType);
            Utilities.Log($"Completed contract {c.subType}");
        }
    }
}
