using System.IO;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using System.Text.RegularExpressions;
using System.Linq;

using ToolbarControl_NS;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using KSP.UI;
//using ClickThroughFix;

namespace KerbalObjectInspector
{
    /// <summary>
    /// The Hierarchy addon. This addon is designed to inspect the scene and list all game objects via Transform searching.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Hierarchy : MonoBehaviour
    {
        internal const string MODID = "KerbalObjectInspector_NS";
        internal const string MODNAME = "Kerbal Object Inspector";

        /// <summary>
        /// The number of times per second this addon will attempt to update.
        /// </summary>
        private const int maxSkippedUpdates = 10;

        /// <summary>
        /// The current time since last update.
        /// </summary>
        private float skippedUpdates = 0;

        /// <summary> all root transforms in the scene + all dontDestroyOnLoad scene root transforms </summary>
        private List<Transform> rootTransforms = new List<Transform>();

        /// <summary>The chain of selected transforms leading to the current selected transform.  </summary>
        private List<Transform> selectionChain = new List<Transform>();

        private List<int> selectionChainDepth = new List<int>();

        /// <summary> The currently selected transform </summary>
        private Transform selectedTransform;

        /// <summary>
        /// The bounds of the Hierarchy window.
        /// </summary>
        private Rect hierarchyRect;
        /// <summary>
        /// The current scroll position of the window's scroll view.
        /// </summary>
        private Vector2 hierarchyScroll;

        /// <summary>
        /// The inspector window.
        /// </summary>
        private Inspector inspector = null;

        public ValueEditor editor = null;

        ToolbarControl toolbarControl = null;

        private bool showUI;

        private enum HoverMode { PART, UI, OBJECT };

        private HoverMode hoverMode = HoverMode.PART;

        private Transform hovered;
        private Transform lastHovered;

        private bool drawWireFrame = true;
        private bool showInactive = false;
        private bool hierarchyLocked = false;
        private string searchFilter = "";
        private bool searchRootOnly = false;
        private bool allAssetsMode = false;

        private static Material glMaterial;

        #region LIFECYCLE

        /// <summary>
        /// Called when this MonoBehaviour starts.
        /// </summary>
        void Start()
        {
            // Instantiate the selection chain.
            selectionChain = new List<Transform>();
            // Create the initial window bounds.
            hierarchyRect = new Rect(50f, 50f, 450f, 800f);
            // Create the initial scroll position.
            hierarchyScroll = Vector2.zero;

            AddButton();

            DontDestroyOnLoad(this);

            glMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            GameEvents.onGameSceneSwitchRequested.Add(OnSceneSwitch);
        }

        void AddButton()
        {
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(ToggleShow, ToggleShow,
                ApplicationLauncher.AppScenes.FLIGHT
                | ApplicationLauncher.AppScenes.MAINMENU
                | ApplicationLauncher.AppScenes.MAPVIEW
                | ApplicationLauncher.AppScenes.SPACECENTER
                | ApplicationLauncher.AppScenes.SPH
                | ApplicationLauncher.AppScenes.TRACKSTATION
                | ApplicationLauncher.AppScenes.VAB,
               MODID,
                "kerbalObjectInspectorButton",
                "KerbalObjectInspector/PluginData/KerbalObjectInspector-38",
                "KerbalObjectInspector/PluginData/KerbalObjectInspector-24",
                MODNAME
            );

        }

        private void OnSceneSwitch(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            rootTransforms.Clear();
            ClearSelection();
        }

        void OnDestroy()
        {
            RemoveWireFrameFromSelection();
            toolbarControl.OnDestroy();
            GameEvents.onGameSceneSwitchRequested.Remove(OnSceneSwitch);
            Destroy(toolbarControl);
        }

        #endregion

        #region CORE UPDATE

        /// <summary>
        /// Called when this Monobehaviour is updated.
        /// </summary>
        void Update()
        {
            if (!showUI)
                return;

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                SelectHoveredTransform();
            }

            if (skippedUpdates < maxSkippedUpdates)
            {
                skippedUpdates++;
                return;
            }

            skippedUpdates = 0;

            if (searchFilter.Length == 0)
            {
                if (!hierarchyLocked)
                {
                    if (allAssetsMode)
                    {
                        SearchAllTransforms(true, true);
                    }
                    else
                    {
                        FindSceneRootTransforms();
                    }
                }
            }
        }

        void ToggleShow()
        {
            if (showUI)
            {
                ClearSelection();
                rootTransforms.Clear();
            }

            showUI = !showUI;
        }

        /// <summary>
        /// Called when it is time for this MonoBehaviour to draw its GUI.
        /// </summary>
        void OnGUI()
        {
            if (!showUI)
                return;

            // make sure the objects are still valid
            for (int i = 0; i < selectionChain.Count; i++)
            {
                if (selectionChain[i] == null || selectionChain[i].gameObject == null)
                {
                    ClearSelection();
                    break;
                }
            }

            for (int i = rootTransforms.Count - 1; i >= 0; i--)
            {
                if (rootTransforms[i] == null || rootTransforms[i].gameObject == null)
                {
                    rootTransforms.RemoveAt(i);
                }
            }

            // Draw the Hierarchy window.
            hierarchyRect = GUI.Window(GetInstanceID(), hierarchyRect, HierarchyWindow, "Hierarchy", HighLogic.Skin.window);


            // If there is something in the selection chain,
            if (!ReferenceEquals(selectedTransform, null))
            {
                // If the inspector is null,
                if (inspector == null)
                {
                    // Create a new inspector.
                    inspector = new Inspector(this, GetInstanceID() + 1, hierarchyRect);
                }

                // Draw the inspector GUI.
                inspector.DrawGUI(selectedTransform);

                if (editor != null)
                {
                    editor.DrawGUI();
                }

                DrawSelectionRectTransform();

            }
            else
            {
                if (editor != null)
                {
                    editor = null;
                }
            }
        }

        #endregion

        #region DRAW UI

        /// <summary>
        /// Draws the Hierarchy window.
        /// </summary>
        /// <param name="windowID">The window ID.</param>
        void HierarchyWindow(int windowID)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Root objects : ", GUILayout.ExpandWidth(false));
            if (GUILayout.Toggle(!allAssetsMode, "Current scene", GUILayout.ExpandWidth(false)))
                allAssetsMode = false;
            if (GUILayout.Toggle(allAssetsMode, "All assets (slow !)", GUILayout.ExpandWidth(false)))
                allAssetsMode = true;

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            showInactive = GUILayout.Toggle(showInactive, "Show <color=orange>inactive</color>", GUILayout.Width(125f));

            GUILayout.Space(20f);

            hierarchyLocked = !GUILayout.Toggle(!hierarchyLocked, "Watch changes", GUILayout.Width(125f));

            GUILayout.Space(20f);

            if (drawWireFrame != GUILayout.Toggle(drawWireFrame, "Draw wireframe", GUILayout.Width(125f)))
            {
                RemoveWireFrameFromSelection();
                if (!drawWireFrame)
                {
                    drawWireFrame = true;
                    AddWireFrameToSelection();
                }
                else
                {
                    drawWireFrame = false;
                }
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();

            GUILayout.Label("Alt + mouse hover selection mode : ", GUILayout.ExpandWidth(false));
            if (GUILayout.Toggle(hoverMode == HoverMode.PART, "Part", GUILayout.ExpandWidth(false)))
                hoverMode = HoverMode.PART;
            if (GUILayout.Toggle(hoverMode == HoverMode.UI, "UI", GUILayout.ExpandWidth(false)))
                hoverMode = HoverMode.UI;
            if (GUILayout.Toggle(hoverMode == HoverMode.OBJECT, "GameObject", GUILayout.ExpandWidth(false)))
                hoverMode = HoverMode.OBJECT;

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Search scene and assets ", GUILayout.ExpandWidth(false));

            string newFilter = GUILayout.TextField(searchFilter);

            if (newFilter != searchFilter)
            {
                searchFilter = newFilter;
                ClearSelection();
                if (searchFilter.Length > 2)
                {
                    SearchAllTransforms(searchRootOnly);
                }
            }

            searchRootOnly = GUILayout.Toggle(searchRootOnly, "Root only", GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();

            // Begin a scroll view.
            hierarchyScroll = GUILayout.BeginScrollView(hierarchyScroll, HighLogic.Skin.scrollView);

            DrawHierarchy();

            // End the scroll view.
            GUILayout.EndScrollView();

            // Allow the user to drag the window.
            GUI.DragWindow();
        }

        private void DrawHierarchy()
        {
            if (rootTransforms.Count == 0)
                return;

            bool hasDrawnSelection = false;

            foreach (Transform root in rootTransforms)
            {
                if (selectionChain.Count == 0 || root != selectionChain[0])
                {
                    DrawTransform(root, 0, false);
                    continue;
                }

                hasDrawnSelection = true;
                DrawSelectionChain();
            }

            if (!hasDrawnSelection && selectionChain.Count > 0)
            {
                DrawSelectionChain();
            }
        }

        private void DrawSelectionChain()
        {
            for (int i = 0; i < selectionChain.Count; i++)
            {
                Transform child = selectionChain[i];
                bool isSelected;
                if (i + 1 < selectionChain.Count && selectionChain[i + 1].parent == child)
                    isSelected = true;
                else if (child == selectedTransform)
                    isSelected = true;
                else
                    isSelected = false;

                DrawTransform(child, selectionChainDepth[i], isSelected);
            }
        }

        private void DrawTransform(Transform transform, int depth, bool isSelected)
        {
            if (!showInactive && !transform.gameObject.activeInHierarchy)
                return;

            string label;
            if (transform == selectedTransform)
                label = "<color=white><b>" + transform.gameObject.name + "</b></color>";
            else if (isSelected)
                label = "<color=white>" + transform.gameObject.name + "</color>";
            else if (!transform.gameObject.activeInHierarchy)
                label = "<color=orange>" + transform.gameObject.name + "</color>";
            else
                label = transform.gameObject.name;

            GUILayout.BeginHorizontal();
            GUILayout.Space(10f * depth);
            if (GUILayout.Button(label, HighLogic.Skin.label))
            {
                if (transform == selectedTransform)
                {
                    if (transform.parent == null)
                    {
                        ClearSelection();
                    }
                    else if (rootTransforms.Contains(transform.parent) || selectionChain.Contains(transform.parent))
                    {
                        SelectTransform(transform.parent);
                    }
                }
                else
                {
                    SelectTransform(transform);
                }
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        #region TRANSFORM FINDING

        private void SelectHoveredTransform()
        {
            if (hoverMode == HoverMode.UI)
            {
                var pointer = new PointerEventData(EventSystem.current);
                pointer.position = Input.mousePosition;

                var raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);

                if (raycastResults.Count > 0)
                    hovered = raycastResults[0].gameObject.transform;
                else
                    hovered = null;
            }
            else if (hoverMode == HoverMode.PART)
            {
                if (Mouse.HoveredPart != null)
                    hovered = Mouse.HoveredPart.gameObject.transform;
                else
                    hovered = null;
            }
            else if (hoverMode == HoverMode.OBJECT)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                int layerMask = ~0;

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue, layerMask))
                    hovered = hit.collider.gameObject.transform;
                else
                    hovered = null;
            }

            if (hovered != lastHovered)
            {
                lastHovered = hovered;
                if (hovered != null)
                {
                    SelectTransform(hovered);
                }
            }
        }

        private void FindSceneRootTransforms()
        {
            rootTransforms.Clear();

            GameObject temp = null;
            Scene? dontDestroyOnLoadScene = null;
            try
            {
                temp = new GameObject();
                DontDestroyOnLoad(temp);
                dontDestroyOnLoadScene = temp.scene;
                DestroyImmediate(temp);
                temp = null;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (temp != null)
                    DestroyImmediate(temp);
            }

            if (dontDestroyOnLoadScene != null)
            {
                foreach (GameObject rootGO in dontDestroyOnLoadScene?.GetRootGameObjects())
                {
                    rootTransforms.Add(rootGO.transform);
                }
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                foreach (GameObject rootGO in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    rootTransforms.Add(rootGO.transform);
                }
            }
        }

        private void SearchAllTransforms(bool rootOnly, bool noFilter = false)
        {
            rootTransforms.Clear();

            foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (noFilter)
                {
                    if (transform.parent == null)
                    {
                        rootTransforms.Add(transform);
                    }
                    continue;
                }
                else if (rootOnly && transform.parent != null)
                {
                    continue;
                }

                if (transform.gameObject.name.IndexOf(searchFilter, System.StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    rootTransforms.Add(transform);
                }
                else
                {
                    foreach (Component component in transform.gameObject.GetComponents<Component>())
                    {
                        if (component.GetType().Name.IndexOf(searchFilter, System.StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            rootTransforms.Add(component.gameObject.transform);
                        }
                    }
                }
            }
        }

        #endregion

        #region SELECTION MANAGEMENT

        private void SelectTransform(Transform selected)
        {
            RemoveWireFrameFromSelection();

            selectedTransform = selected;
            selectionChain.Clear();
            selectionChainDepth.Clear();

            if (selectedTransform == null)
                return;

            List<Transform> directChain = new List<Transform>();

            directChain.Add(selected);
            while (directChain[directChain.Count - 1].parent != null)
            {
                directChain.Add(directChain[directChain.Count - 1].parent);
            }

            directChain.Reverse();

            selectionChain.Add(directChain[0]);
            selectionChainDepth.Add(0);

            Transform[] allChilds = directChain[0].GetComponentsInChildren<Transform>(showInactive);

            FindChainChildsRecursive(0, allChilds, directChain);

            AddWireFrameToSelection();
        }

        private void FindChainChildsRecursive(int minLevel, Transform[] allChilds, List<Transform> directChain)
        {
            int maxLevel = directChain.Count - 1;
            for (int i = minLevel; i <= maxLevel; i++)
            {
                foreach (Transform transform in allChilds)
                {
                    if (transform.parent == directChain[i])
                    {
                        selectionChain.Add(transform);
                        selectionChainDepth.Add(i + 1);

                        if (i + 1 < directChain.Count && transform == directChain[i + 1])
                        {
                            FindChainChildsRecursive(i + 1, allChilds, directChain);
                            maxLevel = i;
                        }
                    }
                }
            }
        }

        private void ClearSelection()
        {
            RemoveWireFrameFromSelection();
            selectionChain.Clear();
            selectionChainDepth.Clear();
            selectedTransform = null;
        }

        /// <summary>
        /// Called when the selection chain is about to change.
        /// </summary>
        private void RemoveWireFrameFromSelection()
        {
            // Try to remove any WireFrame components found.
            try
            {
                Destroy(selectedTransform.gameObject.GetComponent<WireFrame>());
            }
            catch { }
        }

        private void AddWireFrameToSelection()
        {
            if (!drawWireFrame)
                return;

            // If the transform has some form of mesh renderer,
            if (selectedTransform.GetComponent<MeshFilter>() || selectedTransform.GetComponent<SkinnedMeshRenderer>())
            {
                // Add a WireFrame object to it.
                selectedTransform.gameObject.AddComponent<WireFrame>();
            }
        }

        private void DrawSelectionRectTransform()
        {
            if (!drawWireFrame || selectedTransform == null || !(selectedTransform is RectTransform rectTransform))
                return;

            if (UIMasterController.Instance == null)
                return;

            Canvas canvas = UIMasterController.Instance.appCanvas;

            Vector3[] corners = new Vector3[4];
            Vector3[] screenCorners = new Vector3[2];

            rectTransform.GetWorldCorners(corners);

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
            }
            else
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
            }

            GL.PushMatrix();
            glMaterial.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.LINES);

            GL.Color(Color.green);

            GL.Vertex3(screenCorners[0].x, screenCorners[0].y, 0);
            GL.Vertex3(screenCorners[0].x, screenCorners[1].y, 0);

            GL.Vertex3(screenCorners[0].x, screenCorners[1].y, 0);
            GL.Vertex3(screenCorners[1].x, screenCorners[1].y, 0);

            GL.Vertex3(screenCorners[1].x, screenCorners[1].y, 0);
            GL.Vertex3(screenCorners[1].x, screenCorners[0].y, 0);

            GL.Vertex3(screenCorners[1].x, screenCorners[0].y, 0);
            GL.Vertex3(screenCorners[0].x, screenCorners[0].y, 0);

            GL.End();
            GL.PopMatrix();
        }

        #endregion
    }
}
