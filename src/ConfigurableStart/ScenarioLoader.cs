using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UniLinq;
using Upgradeables;
using ContractConfigurator;

namespace ConfigurableStart
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class ScenarioLoader : MonoBehaviour
    {
        public static Dictionary<string, Scenario> LoadedScenarios { get; } = new Dictionary<string, Scenario>();
        private static bool mainMenuVisited;
        private static string curScenarioName;
        // to cache the contract nodes
        private static readonly Dictionary<string, ConfigNode> contractNodes = new Dictionary<string, ConfigNode>();
        private static uint runningContractCoroutines = 0;
        private static bool contractsIterated         = false;
        public static bool ContractsInitialized => runningContractCoroutines == 0 && contractsIterated;
        public static Scenario CurrentScenario
        {
            get
            {
                if (LoadedScenarios != null && curScenarioName != null)
                    return LoadedScenarios[curScenarioName];
                
                return null;
            }
            set
            {
                curScenarioName = value.ScenarioName;
                EditorGUI.Instance.UpdateFromScenario(value);
            }
        }
        public static void SetCurrentScenarioFromName(string name)
        {
            if(!string.IsNullOrEmpty(name) && LoadedScenarios.ContainsKey(name))
                curScenarioName = name;
            EditorGUI.Instance.UpdateFromScenario(CurrentScenario);
        }
        
        public void Start()
        {
            Utilities.Log("Start called");

            // don't destroy on scene switch
            DontDestroyOnLoad(this);
            
            if (!mainMenuVisited)
            {
                GameEvents.onGameNewStart.Add(OnGameNewStart);
                mainMenuVisited = true;
            }

            LoadPresetsFromConfig();
            Utilities.Log("Start finished");
        }

        public void LoadPresetsFromConfig()
        {
            Utilities.Log("Loading Scenarios");
            LoadedScenarios.Clear();
            LoadedScenarios["None"] = new Scenario("None");
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("CUSTOMSCENARIO");

            foreach (var scenarioNode in nodes)
            {
                if (scenarioNode == null)
                    return;
                try
                {
                    var s = new Scenario(scenarioNode);
                    LoadedScenarios[s.ScenarioName] = s;
                }
                catch (Exception ex)
                {
                    Utilities.Log($"{ex}");
                }
            }
            
            int actualCount = LoadedScenarios.Count - 1;
            MainGUI.Instance?.Setup(LoadedScenarios.Keys.ToArray());
            Utilities.Log($"Found {actualCount} scenario{(actualCount > 1 ? "s" : "")}");
        }
        
        public void OnGameNewStart()
        {
            MainGUI.ShowSelectionWindow(false);
            EditorGUI.ShowEditorWindow(false);

            switch (HighLogic.CurrentGame.Mode)
            {
                case Game.Modes.CAREER:
                    Utilities.Log("Career Detected");
                    ApplyScenarioToCareer(LoadedScenarios[curScenarioName]);
                    break;
                case Game.Modes.SANDBOX:
                    Utilities.Log("Sandbox Detected");
                    ApplyScenarioToSandbox(LoadedScenarios[curScenarioName]);
                    break;
                case Game.Modes.SCIENCE_SANDBOX:
                    Utilities.Log("Science Mode Detected");
                    ApplyScenarioToSandbox(LoadedScenarios[curScenarioName]);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Validates the scenario and applies the date, if defined
        /// </summary>
        /// <param name="scn"></param>
        public void ApplyScenarioToSandbox(Scenario scn)
        {
            if (scn == null)
            {
                Utilities.LogWrn($"Selected Scenario doesn't exist, destroying");
                Destroy(this);
            }
            else
            {
                Utilities.Log($"Applying date from scenario {scn.ScenarioName}");
                
                if (scn.StartingUT != 0)
                {
                    SetDate(scn.StartingUT);
                }
            }
        }

        /// <summary>
        /// Validates the scenario and applies every defined field
        /// </summary>
        /// <param name="scn"></param>
        public void ApplyScenarioToCareer(Scenario scn)
        {
            if (scn == null)
            {
                Utilities.LogWrn($"Selected Scenario doesn't exist, destroying");
                Destroy(this);
            }
            else
            {
                Utilities.Log($"Applying scenario {scn.ScenarioName}");
                StartCoroutine(CheckKSPIntializationAndSetParameters(scn));
            }
        }

        private IEnumerator CheckKSPIntializationAndSetParameters(Scenario scn)
        {
            // make sure that everything has initialized properly first
            yield return WaitForInitialization(() => ScenarioUpgradeableFacilities.Instance);
            yield return WaitForInitialization(() => ResearchAndDevelopment.Instance);
            yield return WaitForInitialization(() => Reputation.Instance);
            yield return WaitForInitialization(() => Funding.Instance);
            yield return WaitForInitialization(() => PartLoader.Instance);
            yield return WaitForInitialization(() => Contracts.ContractSystem.Instance);
            if (RP0.Found) yield return WaitForInitialization(() => RP0.MaintenanceHandler);
            if (TestFlight.Found) yield return WaitForInitialization(() => TestFlight.FlightManagerScenarioInstance);

            // just to be even safer
            yield return new WaitForEndOfFrame();

            // set date
            if (scn.StartingUT != 0)
            {
                SetDate(scn.StartingUT);
            }

            // complete contracts
            if (!string.IsNullOrEmpty(scn.CompletedContracts))
            {
                string[] contractNames = Utilities.ArrayFromCommaSeparatedList(scn.CompletedContracts);
                HandleContracts(contractNames, complete: true);
            }
            // accept contracts
            if (!string.IsNullOrEmpty(scn.AcceptedContracts))
            {
                string[] contractNames = Utilities.ArrayFromCommaSeparatedList(scn.AcceptedContracts);
                HandleContracts(contractNames, complete: false);
            }
            contractsIterated = true;

            // unlock technologies
            if (!string.IsNullOrEmpty(scn.UnlockedTechs))
            {
                Dictionary<string, bool> techIDs = Utilities.DictionaryFromCommaSeparatedString(scn.UnlockedTechs, defaultValue: true);

                Dictionary<string, string> unlockFilters;
                unlockFilters = string.IsNullOrEmpty(scn.PartUnlockFilters) ?
                    null : Utilities.DictionaryFromCommaSeparatedString<string>(scn.PartUnlockFilters, defaultValue: null);

                UnlockTechnologies(techIDs, unlockFilters, scn.UnlockPartUpgrades, scn.UnlockPartsInParentNodes);
            }

            // set facility levels
            if (!string.IsNullOrEmpty(scn.FacilityUpgrades))
            {
                Dictionary<string, int> facilities = Utilities.DictionaryFromCommaSeparatedString<int>(scn.FacilityUpgrades);
                SetFacilityLevels(facilities);
            }

            // set KCT launchpads
            if (KCT.Found && !string.IsNullOrEmpty(scn.KCTLaunchpads))
            {
                Dictionary<string, int> pads = Utilities.DictionaryFromCommaSeparatedString<int>(scn.KCTLaunchpads);
                KCT.CreatePads(pads, scn.KCTRemoveDefaultPads);
            }

            //set KCT upgrade points
            if (KCT.Found && scn.KCTUnspentUpgrades != null)
            {
                KCT.SetUnspentPoints(scn.KCTUnspentUpgrades.GetValueOrDefault(-1));
            }
            if(KCT.Found && !string.IsNullOrEmpty(scn.KCTUpgrades))
            {
                KCT.SetUpgradePoints(scn.KCTUpgrades);
            }

            // unlock RF engine configs
            if (RealFuels.Found && !string.IsNullOrEmpty(scn.RFUnlockedConfigs))
            {
                string[] configs = Utilities.ArrayFromCommaSeparatedList(scn.RFUnlockedConfigs);
                RealFuels.UnlockEngineConfigs(configs);
            }

            // set starting DU for TF
            if (TestFlight.Found && !string.IsNullOrEmpty(scn.TFStartingDU))
            {
                Dictionary<string, float> engines = Utilities.DictionaryFromCommaSeparatedString<float>(scn.TFStartingDU);
                TestFlight.SetFlightDataForParts(engines);
            }

            // set reputation
            if (scn.StartingRep != null)
            {
                SetReputation(scn.StartingRep.GetValueOrDefault(HighLogic.CurrentGame.Parameters.Career.StartingReputation));
            }

            // set science points
            if (scn.StartingScience != null)
            {
                SetScience(scn.StartingScience.GetValueOrDefault(HighLogic.CurrentGame.Parameters.Career.StartingScience));
            }

            // set funds
            if (scn.StartingFunds != null)
            {
                SetFunds(scn.StartingFunds.GetValueOrDefault(HighLogic.CurrentGame.Parameters.Career.StartingFunds));
            }

            StartCoroutine(CompleteScenarioInitialization());
            yield break;
        }

        private IEnumerator CompleteScenarioInitialization()
        {
            while (!ContractsInitialized)
                yield return new WaitForFixedUpdate();
            
            Utilities.Log("Scenario applied");
            Utilities.Log("Destroying ScenarioLoader...");
            Destroy(this);
        }

        /// <summary>
        /// Coroutine that waits until the obj parameter has initialized before breaking.
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns></returns>
        private static IEnumerator WaitForInitialization(Func<object> obj)
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
        /// Set the amount of Reputation the player has
        /// TODO: investigate if it works as intended
        /// </summary>
        /// <param name="rep">How much reputation the player should have</param>
        public void SetReputation(float rep)
        {
            Reputation.Instance.SetReputation(rep, TransactionReasons.Progression);
            Utilities.Log($"Set reputation: {rep}");
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
                    break;
                }
            }
        }

        /// <summary>
        /// Iterates through each tech that is defined in the Config and calls UnlockTech()
        /// </summary>
        /// <param name="techIDs"></param>
        public void UnlockTechnologies(Dictionary<string, bool> techIDs, Dictionary<string, string> unlockFilters, bool? unlockPartUpgrades, bool unlockPartsInParents)
        {
            var researchedNodes = new List<ProtoRDNode>();

            AssetBase.RnDTechTree.ReLoad();
            foreach (string tech in techIDs.Keys)
            {
                UnlockTechFromTechID(tech, researchedNodes, techIDs[tech], unlockPartsInParents, unlockPartUpgrades, false, unlockFilters);
            }
        }

        /// <summary>
        /// Find a ProtoRDNode from its techID and unlocks it, along with all its parents.
        /// </summary>
        /// <param name="techID"></param>
        public void UnlockTechFromTechID(string techID, List<ProtoRDNode> researchedNodes, bool unlockParts, bool unlockPartsInParents, bool? unlockPartUpgrades, bool isRecursive, Dictionary<string, string> unlockFilters)
        {
            if (string.IsNullOrEmpty(techID)) return;

            //for some reason, FindNodeByID is not a static method and you need a reference
            List<ProtoRDNode> rdNodes = AssetBase.RnDTechTree.GetTreeNodes().ToList();
            if (rdNodes[0].FindNodeByID(techID, rdNodes) is ProtoRDNode rdNode)
            {
                UnlockTechWithParents(rdNode, researchedNodes, unlockParts, unlockPartsInParents, unlockPartUpgrades, isRecursive, unlockFilters);
            }
            else
                Utilities.LogWrn($"{techID} node not found");
        }

        /// <summary>
        /// Unlock a technology and all of its parents.
        /// </summary>
        /// <param name="protoRDNode"></param>
        public void UnlockTechWithParents(ProtoRDNode protoRDNode, List<ProtoRDNode> researchedNodes, bool unlockParts, bool unlockPartsInParents, bool? unlockPartUpgrades, bool isRecursive, Dictionary<string, string> partUnlockFilters)
        {
            foreach (var parentNode in protoRDNode.parents ?? Enumerable.Empty<ProtoRDNode>())
            {
                if (!researchedNodes.Contains(parentNode))
                    UnlockTechWithParents(parentNode, researchedNodes, unlockParts, unlockPartsInParents, unlockPartUpgrades, true, partUnlockFilters);
            }

            bool b = unlockParts && (!isRecursive || (isRecursive && unlockPartsInParents));
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
            Utilities.Log($"Unlocked tech: {techID}");
            
            if (unlockPartUpgrades ?? unlockParts)
            {
                var upgrades = PartUpgradeManager.Handler.GetUpgradesForTech(techID);
                foreach (var up in upgrades)
                {
                    PartUpgradeManager.Handler.SetUnlocked(up.name, true);
                }
            }
        }

        /// <summary>
        /// Unlock the parts contained in a tech node, blacklisting or whitelisting them based on input arguments.
        /// </summary>
        /// <param name="techID"> The techID of the node containing the parts.</param>
        /// <param name="defaultUnlockParts"> Whether the default behaviour is to buy or not buy parts.</param>
        /// <param name="unlockFilters"> The dictionary&lt;string, string&gt; of field,value kvp that either selects the parts to buy or to not buy,
        /// depending on default behaviour.</param>
        /// <returns></returns>
        public List<AvailablePart> MatchingParts(string techID, bool defaultUnlockParts, Dictionary<string, string> unlockFilters)
        {
            var parts = new List<AvailablePart>();
            var apType = typeof(AvailablePart);
            unlockFilters ??= new Dictionary<string, string>(0);

            if (defaultUnlockParts)
            {
                parts = PartLoader.Instance.loadedParts.Where(p => p.TechRequired == techID).ToList();

                foreach (var filterName in unlockFilters.Keys)
                {
                    string fieldValue = unlockFilters[filterName];

                    if (fieldValue != null)
                    {
                        if(filterName=="tags")
                            parts.RemoveAll(p => (apType.GetField(filterName)?.GetValue(p)
                            .ToString()
                            .Contains(fieldValue.ToLower())) ?? false);
                        else
                            parts.RemoveAll(p => (apType.GetField(filterName)?.GetValue(p)
                            .ToString()
                            .Contains(fieldValue)) ?? false);
                    }
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
                    {
                        if(filterName == "tags")
                            parts.AddRange(PartLoader.Instance.loadedParts
                            .Where(p => p.TechRequired == techID)
                            .Where(p => apType.GetField(filterName)?.GetValue(p).ToString()
                            .Contains(fieldValue.ToLower()) ?? false));
                        else
                            parts.AddRange(PartLoader.Instance.loadedParts
                            .Where(p => p.TechRequired == techID)
                            .Where(p => apType.GetField(filterName)?.GetValue(p).ToString()
                            .Contains(fieldValue) ?? false));
                    }
                    else
                        parts.AddRange(PartLoader.Instance.loadedParts
                            .Where(p => p.TechRequired == techID)
                            .Where(p => p.partConfig.HasNode(filterName)));
                }
            }

            return parts;
        }

        /// <summary>
        /// Generates and accepts/completes an array of ContractConfigurator contracts
        /// </summary>
        /// <param name="names"> Array of contract names that will be completed.</param>
        /// <param name="complete"> Whether to complete or just accept the contracts.</param>
        public void HandleContracts(string[] names, bool complete)
        {
            List<ContractType> contractTypes = ContractType.AllValidContractTypes.ToList();

            foreach (var subType in contractTypes)
            {
                foreach (string contractName in names)
                {
                    if (contractName == subType.name)
                    {
                        var contract = ForceGenerate(subType, 0, new System.Random().Next(), Contracts.Contract.State.Generated);

                        if (contract is null)
                        {
                            Utilities.LogWrn($"Couldn't complete contract {contractName}");
                            continue;
                        }

                        StartCoroutine(HandleContractCoroutine(contract, complete));
                        Utilities.Log($"{(complete ? "Completed" : "Accepted" )} contract {contractName}");
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
            if ((fi = baseT.GetField("seed", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, seed);
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

            // set expiry and deadline type to none
            if ((fi = t.GetField("expiryType", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, Contracts.Contract.DeadlineType.Floating);
            if ((fi = t.GetField("deadlineType", BindingFlags.NonPublic | BindingFlags.Instance)) is FieldInfo)
                fi.SetValue(contract, Contracts.Contract.DeadlineType.Floating);

            contract.TimeDeadline = contractType.deadline;

            return contract;
        }

        private IEnumerator HandleContractCoroutine(ConfiguredContract c, bool complete)
        {
            // tell RP-1 career log to ignore us
            object RP0scope =RP0.InvokeCareerEventScope();
            
            runningContractCoroutines++;
            
            // cache contract nodes
            if (contractNodes.Count == 0)
            {
                var cfgNodes = GameDatabase.Instance.GetConfigNodes("CONTRACT_TYPE");

                foreach (var node in cfgNodes)
                {
                    if (node.GetValue("name") is string subT)
                        contractNodes[subT] = node;
                }
            }

            // load behaviours so that they're correctly fired
            if (contractNodes.ContainsKey(c.subType))
            {
                var cfgNode = contractNodes[c.subType];

                if (cfgNode.GetNodes("BEHAVIOUR") is var bNodes)
                {
                    var behaviourFactories = new List<BehaviourFactory>();

                    foreach (var bNode in bNodes)
                    {
                        BehaviourFactory.GenerateBehaviourFactory(bNode, c.contractType, out var behaviourFactory);
                        if (behaviourFactory != null)
                        {
                            behaviourFactories.Add(behaviourFactory);
                        }
                    }

                    if (BehaviourFactory.GenerateBehaviours(c, behaviourFactories))
                        Utilities.Log($"Generated Behaviours for contract {c.subType}");
                }
            }

            // now, complete the contract step by step
            if (c.Offer())
                yield return new WaitForFixedUpdate();

            if (c.Accept())
            {
                yield return new WaitForFixedUpdate();
            }

            if (complete)
            {
                if (c.Complete())
                {
                    //yield return new WaitForFixedUpdate();
                    Contracts.ContractSystem.Instance.ContractsFinished.Add(c);
                }
                else
                    Utilities.LogWrn($"Couldn't complete contract {c.subType}");
            }
            else
            {
                Contracts.ContractSystem.Instance.Contracts.Add(c);
            }

            runningContractCoroutines--;
            RP0.DisposeCareerEventScope(RP0scope);
        }

        public void CompleteExperiments(Dictionary<string,int> completedSubjects)
        {
            List<ScienceSubject> subjects = ResearchAndDevelopment.GetSubjects();

            foreach(var subject in subjects)
            {
                foreach(string pid in completedSubjects.Keys)
                {
                    if(subject.HasPartialIDstring(pid))
                    {
                        subject.science = subject.scienceCap * completedSubjects[pid];
                    }
                }
            }
        }
    }
}
