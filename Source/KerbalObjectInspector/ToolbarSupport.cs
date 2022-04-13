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
using UnityEngine;
using KSP.UI.Screens;

using KSPe.Annotations;
using Toolbar = KSPe.UI.Toolbar;
using Asset = KSPe.IO.Asset<KerbalObjectInspector.ToolbarController>;
using System.Collections.Generic;

namespace KerbalObjectInspector
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class ToolbarController:MonoBehaviour
	{
		private static ToolbarController instance;
		internal static ToolbarController Instance => instance;
		private Toolbar.Toolbar controller => Toolbar.Controller.Instance.Get<ToolbarController>();

		[UsedImplicitly]
		private void Awake()
		{
			instance = this;
			DontDestroyOnLoad(this);
		}

		[UsedImplicitly]
		private void Start()
		{
			Toolbar.Controller.Instance.Register<ToolbarController>(Version.FriendlyName);
			this.Register();
		}

		// State controller for the toobar button
		private class WindowState:KSPe.UI.Toolbar.State.Status<bool> { protected WindowState(bool v):base(v) { }  public static implicit operator WindowState(bool v) => new WindowState(v);   public static implicit operator bool(WindowState s) => s.v; }
		private Toolbar.Button button = null;

		internal const string ICON_DIR = "Icons";
		private static UnityEngine.Texture2D launcher = null;
		private static UnityEngine.Texture2D toolbar = null;

		internal void Register()
		{
			launcher			= launcher			?? (launcher = Asset.Texture2D.LoadFromFile(ICON_DIR, "toolbar-38"));
			toolbar				= toolbar			?? (toolbar = Asset.Texture2D.LoadFromFile(ICON_DIR, "toolbar-24"));
			this.button = Toolbar.Button.Create(this
					, ApplicationLauncher.AppScenes.FLIGHT
						| ApplicationLauncher.AppScenes.MAINMENU
						| ApplicationLauncher.AppScenes.MAPVIEW
						| ApplicationLauncher.AppScenes.SPACECENTER
						| ApplicationLauncher.AppScenes.SPH
						| ApplicationLauncher.AppScenes.TRACKSTATION
						| ApplicationLauncher.AppScenes.VAB
					, launcher
					, toolbar
					, Version.FriendlyName
				);

			this.button.Mouse.Add(Toolbar.Button.MouseEvents.Kind.Left, this.Button_OnLeftClick);
			this.controller.Add(this.button);
			ToolbarController.Instance.ButtonsActive(true, true);
		}

		internal void ButtonsActive(bool enableBlizzy, bool enableStock)
		{
			this.controller.ButtonsActive(enableBlizzy, enableStock);
		}

		internal void Unregister()
		{
			this.controller.Destroy();
			this.button = null;
		}

		internal void Button_OnLeftClick()
		{
			Log.dbg("Left Click!!!");
			if (null != Hierarchy.Instance) Hierarchy.Instance.ToggleShow();
		}
	}
}
