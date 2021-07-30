using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustomScenarioManager
{
    public static class TestFlight
    {
        private static bool? isInstalled = null;
        private static Type FlightManagerScenarioType = null;
        private static object instance = null;

        public static bool Found
        {
            get
            {
                if (!isInstalled.HasValue)
                {
                    AssemblyLoader.loadedAssemblies.TypeOperation(t =>
                    {
                        if (t.FullName == "TestFlightCore.TestFlightManagerScenario")
                        {
                            FlightManagerScenarioType = t;
                            Utilities.Log("TestFlight detected");
                        }
                    });

                    isInstalled = FlightManagerScenarioType != null;
                }

                return isInstalled.Value;
            }
        }

        public static object Instance
        {
            get
            {
                if (Found && instance == null)
                {
                    instance = FlightManagerScenarioType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                }
                return instance;
            }
            set => instance = value;
        }

        public static void SetFlightDataForParts(Dictionary<string, float> data)
        {
            if (Instance == null) return;

            if (FlightManagerScenarioType.GetMethod("SetFlightDataForPartName") is MethodInfo SetFlightDataForPartName)
            {
                foreach (KeyValuePair<string, float> kvp in data)
                {
                    try
                    {
                        //kvp.Key = part name
                        //kvp.Value = flight data
                        SetFlightDataForPartName.Invoke(Instance, new object[] { kvp.Key, kvp.Value });
                        Utilities.Log($"Flight Data for part {kvp.Key} set to {kvp.Value}");
                        CustomScenarioData.tfStartingDU.Append($"{kvp.Key}@{kvp.Value},");
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogErr(ex);
                        Utilities.LogErr("Couldn't set TF flight data");
                        break;
                    }
                }
            }
        }
    }
}
