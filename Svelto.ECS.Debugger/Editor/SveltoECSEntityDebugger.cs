﻿using System;
using System.Collections.Generic;
using System.Linq;
using Svelto.ECS.Debugger.DebugStructure;
using Svelto.ECS.Debugger.Editor.ListViews;
using Svelto.ECS.Debugger.Editor.EntityInspector;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;
using GroupListView = Svelto.ECS.Debugger.Editor.ListViews.GroupListView;

namespace Svelto.ECS.Debugger.Editor
{
    public class SveltoECSEntityDebugger : EditorWindow
    {
        private const float kSystemListWidth = 150f;

        private float CurrentEntityViewWidth =>
            Mathf.Max(100f, position.width - kSystemListWidth);

        [MenuItem("Window/Analysis/Svelto.ECS Debugger", false)]
        private static void OpenWindow()
        {
            GetWindow<SveltoECSEntityDebugger>("Svelto.ECS Entity Debugger");
        }

        private static GUIStyle LabelStyle
        {
            get
            {
                return labelStyle ?? (labelStyle = new GUIStyle(EditorStyles.label)
                {
                    margin = EditorStyles.boldLabel.margin,
                    richText = true
                });
            }
        }

        private static GUIStyle labelStyle;

        private static GUIStyle BoxStyle
        {
            get
            {
                return boxStyle ?? (boxStyle = new GUIStyle(GUI.skin.box)
                {
                    margin = new RectOffset(),
                    padding = new RectOffset(1, 0, 1, 0),
                    overflow = new RectOffset(0, 1, 0, 1)
                });
            }
        }

        private static GUIStyle boxStyle;
        private static SveltoECSEntityDebugger Instance { get; set; }
        
        private const float kLineHeight = 18f;
        const float kChunkInfoButtonWidth = 60f;
        private readonly RepaintLimiter repaintLimiter = new RepaintLimiter();

        private EntitySelectionProxy selectionProxy;
        
        private List<TreeViewState> groupListStates = new List<TreeViewState>();
        private List<string> groupListStateNames = new List<string>();
        public GroupListView groupListView;
        
        [SerializeField] private TreeViewState entityListState = new TreeViewState();
        private EntityListView entityListView;
        internal WorldPopup m_WorldPopup;
        public uint? GroupSelectionId { get; set; }

        public DebugGroup GetSelectionGroup() =>
            RootSelection.DebugGroups.FirstOrDefault(f => f.Id == GroupSelectionId);
        public DebugEntity GetSelectionEntity() =>
            RootSelection.DebugGroups.FirstOrDefault(f => f.DebugEntities.Any(a => a.Id == EntitySelection))?.DebugEntities.FirstOrDefault(f => f.Id == EntitySelection);
        
        public DebugRoot RootSelection { get; set; }

        public void SetGroupSelection(uint? manager)
        {
            GroupSelectionId = manager;
            entityListView.SetSelection(new List<int>());
            ReloadAll();
        }
        
        public uint? EntitySelection;
        internal void SetEntitySelection(uint? entityId)
        {
            EntitySelection = entityId;
            var entity = GetSelectionEntity();
            if (entity == null)
            {
                Selection.activeObject = null;
            }
            else
            {
                selectionProxy.SetEntity(GetSelectionEntity);
                Selection.activeObject = selectionProxy;
            }
            ReloadAll();
        }

        public void SetRootSelection(DebugRoot selection)
        {
            if (RootSelection != selection)
            {
                RootSelection = selection;
                ReloadAll();
            }
        }
        private void CreateEntityListView()
        {
            entityListView = EntityListView.CreateGroupListView(() => GetSelectionGroup(), m => SetEntitySelection(m));
        }
        private void CreateGroupListView()
        {
            groupListView = GroupListView.CreateGroupListView(() => RootSelection, (m) => SetGroupSelection(m));
            groupListView.multiColumnHeader.ResizeToFit();
        }
        private void CreateWorldPopup()
        {
            m_WorldPopup = new WorldPopup(
                () => RootSelection,
                x => SetRootSelection(x),
                () => Data?.DebugRoots
                );
        }
        public DebugTree Data;
        private void OnEnable()
        {
            Instance = this;
            UpdateAll();
            UpdateData();

            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        private void CreateEntitySelectionProxy()
        {
            selectionProxy = ScriptableObject.CreateInstance<EntitySelectionProxy>();
            selectionProxy.hideFlags = HideFlags.HideAndDontSave;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
            if (selectionProxy)
                DestroyImmediate(selectionProxy);

            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
        }

        private void OnPlayModeStateChange(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
                ClearAll();
            if (change == PlayModeStateChange.ExitingPlayMode && Selection.activeObject == selectionProxy)
                Selection.activeObject = null;
        }
        private void Update()
        {
            if (Application.isPlaying)
            {
                UpdateData();
                Repaint();
            }
            else
            {
                Repaint();
            }
        }

        private void UpdateData()
        {
            if (Application.isPlaying)
            {
                if (Data == null)
                {
                    Data = Debugger.Instance.DebugInfo;
                    if (Data.DebugRoots.Count > 0)
                        RootSelection = Data.DebugRoots[0];
                    UpdateAll();
                    Data.OnUpdate += ReloadAll;
                }
            }
            else
            {
                Data = null;
                ClearAll();
            }
        }

        private void ClearAll()
        {
            Data?.Clear();
            UpdateAll();
            ReloadAll();
        }

        private void UpdateAll()
        {
            CreateEntitySelectionProxy();
            CreateWorldPopup();
            CreateGroupListView();
            CreateEntityListView();
        }

        private void ReloadAll()
        {
            groupListView.Reload();
            entityListView.Reload();
        }

        private void ShowWorldPopup()
        {
            m_WorldPopup.OnGui();
        }

        private void GroupList()
        {
            var rect = GUIHelpers.GetExpandingRect();
            groupListView.OnGUI(rect);
        }

        private void GroupsHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Groups", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            ShowWorldPopup();
            GUILayout.EndHorizontal();
        }
        private void EntityHeader()
        {
            if (RootSelection != null || RootSelection != null)
            {
                var rect = new Rect(kSystemListWidth, 3f, CurrentEntityViewWidth, kLineHeight);
                if (GroupSelectionId == null)
                {
                    GUI.Label(rect, "All Entities", EditorStyles.boldLabel);
                }
                else
                {
                    GUI.Label(rect, Debugger.GetNameGroup(GroupSelectionId.Value), LabelStyle);
                }
            }
        }

        void EntityList()
        {
            GUILayout.BeginVertical(BoxStyle);
            entityListView.OnGUI(GUIHelpers.GetExpandingRect());
            GUILayout.EndVertical();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0f, 0f, kSystemListWidth, position.height));
            GroupsHeader();

            GUILayout.BeginVertical(BoxStyle);
            GroupList();
            GUILayout.EndVertical();

            GUILayout.EndArea();

            EntityHeader();

            GUILayout.BeginArea(new Rect(kSystemListWidth, kLineHeight, CurrentEntityViewWidth, position.height - kLineHeight));
            EntityList();
            GUILayout.EndArea();

            repaintLimiter.RecordRepaint();
        }
    }
}
