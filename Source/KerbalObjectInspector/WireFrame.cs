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
using UnityEngine;

namespace KerbalObjectInspector
{
    class WireFrame : MonoBehaviour
    {
        public Material lineMaterial;
        public Color lineColor = new Color(0.0f, 1.0f, 0.0f);

        private Mesh toRender;

        void Start()
        {
            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Self-Illumin/Diffuse"));
            }

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
            GL.wireframe = true;

            lineMaterial.color = lineColor;

            lineMaterial.SetPass(0);

            Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);

            GL.wireframe = false;
        }
    }
}
