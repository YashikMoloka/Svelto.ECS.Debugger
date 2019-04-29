﻿using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Svelto.Svelto.ECS.Debugger.Editor.EntityInspector;
using UnityEditor;
using UnityEngine;

namespace Svelto.ECS.Debugger.Editor.EntityInspector
{
    [CustomEditor(typeof(EntitySelectionProxy))]
    internal class EntitySelectionProxyEditor : UnityEditor.Editor
    {
        private readonly RepaintLimiter repaintLimiter = new RepaintLimiter();
        private JsonSerializerSettings settings;

        void OnEnable()
        {
            settings = new JsonSerializerSettings();
            settings.ContractResolver = new IgnoreParentPropertiesResolver(true);
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Converters.Add(new VectorConverter(true, true, true));
            settings.Converters.Add(new QuaternionConverter());
            settings.Converters.Add(new Matrix4x4Converter());
            settings.Converters.Add(new ResolutionConverter());
            settings.Converters.Add(new ColorConverter());
            settings.Converters.Add(new KeyValuePairConverter());
            settings.TypeNameHandling = TypeNameHandling.All;
            //settings.Converters.Add(new UnityConverter());
        }

        private void Update()
        {
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            var targetProxy = (EntitySelectionProxy) target;
            if (targetProxy == null)
                return;
            if (!targetProxy.Exists)
                return;

            var container = targetProxy.Entity?.Invoke();
            if (container == null)
                return;
            var debugStructs = container.DebugStructs.Select(s => s.Value);
            
            GUI.enabled = true;
            var text = string.Empty;
            try
            {
                text = JsonConvert.SerializeObject(debugStructs, Formatting.Indented, settings);
            }
            catch (Exception e)
            {
                text = "Error in serialization to JSON. Maybe list of entity do not update or this is bug.";
            }
            EditorGUILayout.TextArea(text);

            repaintLimiter.RecordRepaint();
        }

        public override bool RequiresConstantRepaint()
        {
            return true; //(repaintLimiter.SimulationAdvanced() || !Application.isPlaying);
        }
    }
}
