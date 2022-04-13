# Kerbal Object Inspector /L Unleashed :: Change Log

* 2022-0413: 1.2.0.1 (LisiasT) for KSP >= 1.3.1
	+ Adding KSPe facilities
		- Log
		- Abstracted GUI
		- Abstracted File System
		- Installment Checks
		- Toolbar
	+ Certified to work on KSP 1.3.1 to 1.12.3! :)
* 2022-0125: 1.2.0 (LinuxGuruGamer) for KSP 1.12.2
	+ Thanks to @gotmachine for all of these changes:
		- Hirearchy : reworked the general UI options
		- Hierarchy : reworked mouse hover selection, now require holding ALT, added UI/object selection capacities
		- Hierarchy : huge performance improvements (doesn't re-parse anymore the whole scene hierarchy every update)
		- Hierarchy : ability to find/show disabled objects
		- Hierarchy : ability to browse/search the whole resource database (prefabs, internal unity objects...), by object name or by component type (a bit laggy but does the job)
		- Hierarchy : now show objects in the right order
		- Hierarchy : removed sorting feature and ability to dump the hierarchy to a text file (could be re-added if necessary but that's a bit useless and I'm lazy)
		- The wireframe viewer is now only added to the selected object and not whole hierarchy (was causing severe perf issue)
		- Added wireframe visualization for RectTransforms
		- Fixed toolbar button not showing with stock toolbar (was working only with blizzy)
		- Inspector : now only show details for only 1 component at a time (fix lag with large/many component), better usability.
		- Inspector : now show the gameobject fields/properties (and not only the components)
		- Inspector : allow to enable/disable gameobjects
		- Inspector : various UI tweaks alignment, colors, handling of long values, fix height flickering...
		- Inspector : option to show non public / static members
		- Added a "value editor", allow setting fields/properties for primitive types and some common Unity/KSP primitives (Color, Vector, Quaternion, etc...)
		- https://user-images.githubusercontent.com/24925209/101765510-b1349d00-3ae1-11eb-8e18-643c0b6ed1f4.png
* 2021-0808: 1.1.8.2 (LinuxGuruGamer) for KSP 1.12.2
	+ Renamed DLL for CKAN compatibility
	+ Added AssemblyFileVersion
	+ Updated version file for 1.12
* 2019-1104: 1.1.8.1 (LinuxGuruGamer) for KSP 1.8.1
	+ Updated shader name with "Legacy Shaders/"
	+ Added checks for obsolete attributes in  Inspector.cs
* 2019-1103: 1.1.8 (LinuxGuruGamer) for KSP 1.8.1
	+ Added Right click on object line will write tree data (descending from that point) to a file:  ObjectInfo.txt
	+ Updated for KSP 1.8
* 2019-0727: 1.1.7.3 (LinuxGuruGamer) for KSP 1.7.3
	+ Added InstallChecker
	+ Updated AssemblyVersion.tt
* 2019-0109: 1.1.7.2 (LinuxGuruGamer) for KSP 1.6.0
* 2018-1028: 1.1.7.1 (LinuxGuruGamer) for KSP 1.5.1
	+ Version bump for 1.5 rebuild
* 2018-1007: 1.1.7 (LinuxGuruGamer) for KSP 1.4.5
	+ Added properties to the component inspector
	+ Component inspector field and properties also show base class properties (BindingFlags.FlattenHierarchy)
* 2018-0418: 1.1.6 (LinuxGuruGamer) for KSP 1.4.2
	+ Updated for KSP 1.4.1+
	+ added support for the Clickthrough Blocker
	+ Added support for the toolbar Controller
	+ Updated button texture with transparancy
* 2017-1010: 1.1.5 (LinuxGuruGamer) for KSP 1.3.1
	+ KSP 1.3.1 update
* 2017-0528: 1.1.4 (LinuxGuruGamer) for KSP 1.3.0
	+ Updated for 1.3
* 2017-0218: 1.1.3 (LinuxGuruGamer) for KSP 1.2.2
	+ Reduced spacing in inspector
* 2017-0105: 1.1.2 (LinuxGuruGamer) for KSP 1.2.2
	+ Adds Sorting, Searching, and to MainMenu
* 2016-1105: 1.1.1 (LinuxGuruGamer) for KSP 1.2.2
	+ Initial release for 1.2
	+ Fixed Nullrefs
	+ Added toolbar button to activate/deactivate it
	+ Made it active only in game scenes
	+ Fixed display of wireframes
	+ Added display of part mouse is over
	+ Added locking of display (so that moving the mouse over something else won't change the display)
* 2016-0328: 1.0 (IRnifty) for KSP 1.0.5
	+ Version 1.0 of Kerbal Object Inspector
		- INITIAL RELEASE
