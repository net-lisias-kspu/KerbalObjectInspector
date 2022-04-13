/*
	This file is part of Kerbal Object Inspector /L Unleashed
		© 2022 LisiasT
		© 2016 IRnifty

	Kerbal Object Inspector /L is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Kerbal Object Inspector /L is distributed in the hope that it will
	be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 2.0
	along with Kerbal Object Inspector /L.
	If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using UnityEngine;

namespace KerbalObjectInspector
{
    class WireFrame : MonoBehaviour
    {
        private static Material _material;

        public static Material lineMaterial => _material ?? (_material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended")));
        public Color lineColor = new Color(0.0f, 1.0f, 0.0f);

        private Mesh toRender;

        void Start()
        {
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;

            toRender = new Mesh();
        }

        void OnRenderObject()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter)
            {
                DoRender(filter.mesh);
                return;
            }

            SkinnedMeshRenderer skinned = GetComponent<SkinnedMeshRenderer>();
            if (skinned)
            {
                skinned.BakeMesh(toRender);

                Vector3[] verts = toRender.vertices;

                for (int i = 0; i < verts.Length; i++)
                {
                    float x = verts[i].x;
                    float y = verts[i].y;
                    float z = verts[i].z;
                    verts[i] = new Vector3(x / transform.lossyScale.x, y / transform.lossyScale.y, z / transform.lossyScale.z);
                }

                toRender.vertices = verts;
                
                DoRender(toRender);
            }
        }

        void DoRender(Mesh mesh)
        {
            if (mesh == null ||transform == null || lineMaterial == null)
                return;
            GL.wireframe = true;

            lineMaterial.color = lineColor;

            lineMaterial.SetPass(0);
            try
            {
                Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
            }
            catch (Exception) { }
            GL.wireframe = false;
        }
    }
}
