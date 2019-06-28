using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HKAdditionalSkips
{
    public class HKAdditionalSkips : Mod
    {
        /// <summary>
        /// Represents this Mod's instance.
        /// </summary>
        internal static HKAdditionalSkips Instance;

        /// <summary>
        /// Fetches the Mod Version From AssemblyInfo.AssemblyVersion
        /// </summary>
        /// <returns>Mod's Version</returns>
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        internal Dictionary<string, List<(float, float, float)>> spikeDict;
        internal GameObject spikes6Prefab;

        public HKAdditionalSkips() : base("HKAdditionalSkips")
        {
        }


        public override List<(string, string)> GetPreloadNames()
        {
            var x = new List<(string, string)>();
            x.Add(("Tutorial_01", "_Props"));

            return x;
        }

        /// <summary>
        /// Called after the class has been constructed.
        /// </summary>
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            //Assign the Instance to the instantiated mod.
            Instance = this;
            // Initialize spike data dictionary
            spikeDict = new Dictionary<string, List<(float, float, float)>>();
            FillSpikeDict(HKASData.Tutorial_01, "Tutorial_01");
            FillSpikeDict(HKASData.Cliffs_02, "Cliffs_02");
            FillSpikeDict(HKASData.Crossroads_18, "Crossroads_18");


            Log("Initializing HKAS");

            //ModHooks.Instance.AttackHook += OnAttack;

            // Create duplicate of Cave Spikes (6)
            var spikes6 = preloadedObjects["Tutorial_01"]["_Props"].transform.Find("Cave Spikes (6)").gameObject;
            spikes6Prefab = GameObject.Instantiate(spikes6);
            spikes6Prefab.SetActive(false);
            var b = spikes6Prefab.GetComponent<BoxCollider2D>();
            b.size = new Vector2(2.0f, 1.0f);
            b.offset = Vector2.zero;
            GameObject.DontDestroyOnLoad(spikes6Prefab);

            On.GameManager.FinishedEnteringScene += GameManager_FinishedEnteringScene;
            Log("Initialized HKAS");
        }

        private void FillSpikeDict(string spikeData, string sceneName)
        {
            var parsedData = new List<(float, float, float)>();

            var spikesInfo = spikeData.Split(';');
            for (int i = 0; i < spikesInfo.Length; i++)
            {
                var spikeInfo = spikesInfo[i];
                var spike = spikeInfo.Split(',');
                var x = float.Parse(spike[0]);
                var y = float.Parse(spike[1]);
                var angle = float.Parse(spike[2]);
                parsedData.Add((x, y, angle));
            }

            spikeDict.Add(sceneName, parsedData);
        }


        private void GameManager_FinishedEnteringScene(On.GameManager.orig_FinishedEnteringScene orig, GameManager self)
        {
            orig.Invoke(self);
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (spikeDict.ContainsKey(sceneName))
            {
                Log("HKAS: Loaded Scene " + sceneName);
                var spikes = spikeDict[sceneName];
                foreach (var spike in spikes)
                {
                    InstantiateSpike(spike.Item1, spike.Item2, spike.Item3);
                }
            }
        }

        private void InstantiateSpike(float x, float y, float angle)
        {
            var go = GameObject.Instantiate(spikes6Prefab, new Vector3(x, y, 0), Quaternion.Euler(0, 0, angle));
            go.SetActive(true);
        }
    }
}
