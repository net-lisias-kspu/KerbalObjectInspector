/*
	This file is part of Kerbal Object Inspector /L Unleashed
		© 2022 LisiasT
		© 2016 IRnifty

	Kerbal Object Inspector /L is double licensed, as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Kerbal Object Inspector /L is distributed in the hope that it will
	be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 2.0
	along with Kerbal Object Inspector /L.
	If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections.Generic;
using UnityEngine;

using GUI = KSPe.UI.GUI;
using GUILayout = KSPe.UI.GUILayout;

namespace KerbalObjectInspector
{
    /// <summary>
    /// The Hierarchy addon. This addon is designed to inspect the scene and list all game objects via Transform searching.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class Hierarchy : MonoBehaviour
    {
        /// <summary>
        /// The number of times per second this addon will attempt to update.
        /// </summary>
        private int updatesPerSecond = 10;

        /// <summary>
        /// The time as a float in seconds this addon will wait until the next update.
        /// </summary>
        private float UpdateTime
        {
            get { return 1f / (float)updatesPerSecond; }
        }

        /// <summary>
        /// The current time since last update.
        /// </summary>
        private float currentTime = 0f;

        /// <summary>
        /// An array of all transforms in the scene.
        /// </summary>
        private Transform[] allTrans;
        /// <summary>
        /// The chain of selected transforms leading to the current selected transform.
        /// </summary>
        private List<Transform> selectionChain;

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

        /// <summary>
        /// Called when this MonoBehaviour starts.
        /// </summary>
        void Start()
        {
            // Instantiate the selection chain.
            selectionChain = new List<Transform>();
            // Create the initial window bounds.
            hierarchyRect = new Rect(50f, 50f, 500f, 1000f);
            // Create the initial scroll position.
            hierarchyScroll = Vector2.zero;
        }

        /// <summary>
        /// Called when this Monobehaviour is updated.
        /// </summary>
        void Update()
        {
            // Increment the current time since updating.
            currentTime += Time.deltaTime;

            // If the current time since updating passes the update threshold,
            if (currentTime >= UpdateTime)
            {
                // Subtract time until it is less than the threshold.
                do
                {
                    currentTime -= UpdateTime;
                } while (currentTime >= UpdateTime);

                // Update the list of transforms.
                allTrans = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
            }
        }

        /// <summary>
        /// Called when it is time for this MonoBehaviour to draw its GUI.
        /// </summary>
        void OnGUI()
        {
            // Draw the Hierarchy window.
            hierarchyRect = GUI.Window(GetInstanceID(), hierarchyRect, HierarchyWindow, "Hierarchy", HighLogic.Skin.window);

            // If there is something in the selection chain,
            if (selectionChain.Count > 0)
            {
                // If the inspector is null,
                if (inspector == null)
                {
                    // Create a new inspector.
                    inspector = new Inspector(GetInstanceID() + 1, hierarchyRect);
                }

                // Draw the inspector GUI.
                inspector.DrawGUI(selectionChain[selectionChain.Count - 1]);
            }
        }

        /// <summary>
        /// Draws the Hierarchy window.
        /// </summary>
        /// <param name="windowID">The window ID.</param>
        void HierarchyWindow(int windowID)
        {
            // Begin a scroll view.
            hierarchyScroll = GUILayout.BeginScrollView(hierarchyScroll, HighLogic.Skin.scrollView);

            // Begin listing all transforms with no parents.
            ListChildren(0, null);

            // End the scroll view.
            GUILayout.EndScrollView();

            // Allow the user to drag the window.
            GUI.DragWindow(new Rect(0f, 0f, 500f, 20f));
        }

        /// <summary>
        /// Draws controls for game objects based on the given parent and depth level.
        /// </summary>
        /// <param name="depth">The depth of the listing iteration.</param>
        /// <param name="parent">The parent to check for children. If null, draws for all root objects.</param>
        void ListChildren(int depth, Transform parent)
        {
            // Iterate through the list of transforms.
            foreach (Transform trans in allTrans)
            {
                // If the current transform's parent is the provided, or if the current transform's parent is null AND the provided parent object is null,
                if (trans.parent == parent)
                {
                    // Begin a horizontal section.
                    GUILayout.BeginHorizontal();

                    // Introduce a space multiplied by the depth, for indenting child controls.
                    GUILayout.Space(10f * (float)depth);

                    // Initialize as false by default.
                    bool isSelected = false;

                    // If the selection chain's list count is greater than the current depth,
                    if (selectionChain.Count > depth)
                    {
                        // And the current transform is the same as the current selection chain object,
                        if (trans == selectionChain[depth])
                        {
                            // The current object is selected at this depth level. It will be searched for children and show as green in the view.
                            isSelected = true;
                        }
                    }

                    // Draw a button. If the button is clicked,
                    if (GUILayout.Button((isSelected ? "" : "<color=#ffffffff>") + trans.gameObject.name + (isSelected ? "" : "</color>"), HighLogic.Skin.label))
                    {
                        // Signal a future selection chain change.
                        OnSelectionAboutToChange();

                        // If the button is closer to the root than the chain is deep,
                        if (selectionChain.Count > depth)
                        {
                            // Cut the chain back down to the correct length.
                            selectionChain = selectionChain.GetRange(0, depth);
                        }

                        // Add the newly selected transform to the possible truncated selection chain.
                        selectionChain.Add(trans);

                        // Signal a change in the selection chain.
                        OnSelectionChanged();
                    }

                    // End the horizontal section.
                    GUILayout.EndHorizontal();

                    // If the current transform is selected,
                    if (isSelected)
                    {
                        // Recursively search it for children and draw their controls.
                        ListChildren(depth + 1, trans);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the selection chain is about to change.
        /// </summary>
        void OnSelectionAboutToChange()
        {
            // For each transform in the chain,
            foreach(Transform trans in selectionChain)
            {
                // Try to remove any WireFrame components found.
                try
                {
                    Destroy(trans.GetComponent<WireFrame>());
                }
                catch { }
            }
        }

        void OnSelectionChanged()
        {
            for (int i = 0; i < selectionChain.Count; i++ )
            {
                // If the transform has some form of mesh renderer,
                if (selectionChain[i].GetComponent<MeshFilter>() || selectionChain[i].GetComponent<SkinnedMeshRenderer>())
                {
                    // Add a WireFrame object to it.
                    WireFrame added = selectionChain[i].gameObject.AddComponent<WireFrame>();

                    // If the current index doesn't point to the last in the chain,
                    if (i < selectionChain.Count - 1)
                    {
                        // Dim the color a bit.
                        added.lineColor = new Color(0.0f, 0.5f, 0.75f);
                    }
                }
            }
        }

        void OnDestroy()
        {
            OnSelectionAboutToChange();
        }
    }
}
