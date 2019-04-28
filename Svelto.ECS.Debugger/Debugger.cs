using System;
using System.Collections.Generic;
using System.Linq;
using Svelto.ECS.Debugger.DebugStructure;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Svelto.ECS.Debugger
{
    public class Debugger : MonoBehaviour
    {
        public static Debugger Instance;
        private static Dictionary<uint, string> GroupDebugNames = new Dictionary<uint, string>();
        private static Dictionary<EnginesRoot, string> EnginesRootDebugNames = new Dictionary<EnginesRoot, string>();
        [NonSerialized]
        public DebugTree DebugInfo = new DebugTree();
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            DebugInfo.Update();
        }

        public void AddEnginesRoot(EnginesRoot root)
        {
            if (root is EnginesRootNamed named)
            {
                EnginesRootDebugNames[root] = named.Name;
            }
            DebugInfo.AddRootToTree(root);
        }

        public static void RegisterNameGroup(uint id, string name)
        {
            GroupDebugNames[id] = name;
        }
        public static string GetNameGroup(uint id)
        {
            if (!GroupDebugNames.ContainsKey(id))
                return $"Unknown group: {id}";
            return GroupDebugNames[id];
        }
        public static string GetNameRoot(DebugRoot root)
        {
            var engroot = Instance?.DebugInfo.DebugRoots.FirstOrDefault(r => root == r)?.EnginesRoot;
            if (engroot == null || !EnginesRootDebugNames.ContainsKey(engroot))
                return $"Unknown root";
            return EnginesRootDebugNames[engroot];
        }
    }
}