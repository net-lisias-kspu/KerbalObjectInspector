using UnityEngine;
using ToolbarControl_NS;

namespace KerbalObjectInspector
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(Hierarchy.MODID, Hierarchy.MODNAME);
        }
    }
}