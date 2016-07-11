using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KSP;
using UnityEngine;

using System.IO;
using JsonFx;


namespace KIPC
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Addon : MonoBehaviour
    {
        public static Queue<string> krpcMessageQueue = new Queue<string>();
        void Start()
        {
            DontDestroyOnLoad(this);
            Debug.Log("[KIPCPlugin] Hello, Solar System!");

            GameEvents.onLevelWasLoaded.Add(this.OnLevelWasLoaded);
        }

        void OnLevelWasLoaded(GameScenes scene)
        {
            if(scene == GameScenes.MAINMENU)
            {
                krpcMessageQueue.Clear();
                // Handle the queue?
            }

        }


    }

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.FLIGHT })]
    public class KIPCPluginData : ScenarioModule
    {
        public static System.Runtime.Serialization.ObjectIDGenerator idgen = new System.Runtime.Serialization.ObjectIDGenerator();
        public override void OnAwake()
        {
            bool dummy;
            base.OnAwake();
            Debug.Log(string.Format("[KIPCPlugin] {0} PersistentState OnAwake()", idgen.GetId(this, out dummy)));
        }
        public override void OnSave(ConfigNode node)
        {
            bool dummy;
            node.ClearNodes();
            ConfigNode child = node.AddNode("MESSAGEQUEUE");
            foreach(string message in Addon.krpcMessageQueue)
            {
                child.AddValue("message", message);
                Debug.Log("Saved message " + message);
            }
            base.OnSave(node);
            Debug.Log(string.Format("[KIPCPlugin] {0} PersistentState OnSave()", idgen.GetId(this, out dummy)));
        }
        public override void OnLoad(ConfigNode node)
        {
            Addon.krpcMessageQueue.Clear();
            bool dummy;
            ConfigNode child = node.GetNode("MESSAGEQUEUE");
            if(child != null) {
                foreach(string message in child.GetValues("message"))
                {
                    Addon.krpcMessageQueue.Enqueue(message);
                    Debug.Log("Queued message " + message);
                }
            }

            base.OnLoad(node);
            Debug.Log(string.Format("[KIPCPlugin] {0} PersistentState OnLoad()", idgen.GetId(this, out dummy)));
        }

    }

}
