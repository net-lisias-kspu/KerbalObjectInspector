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
