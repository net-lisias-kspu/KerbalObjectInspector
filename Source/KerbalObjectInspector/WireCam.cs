﻿/*
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
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    class WireCam : MonoBehaviour
    {
        Camera wireCam;

        void Start()
        {
            GameObject camObj = Camera.main.gameObject;

            wireCam = camObj.AddComponent<Camera>();

            wireCam.cullingMask = 1 << int.MaxValue;
        }

        void OnDestroy()
        {
            Destroy(wireCam);
        }
    }
}
