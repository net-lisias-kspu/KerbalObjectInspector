
set H=R:\KSP_1.3.1_dev
echo %H%


copy Source\KerbalObjectInspector\bin\Debug\KerbalObjectInspector.dll GameData\KerbalObjectInspector\Plugins
mkdir %H%\GameData\KerbalObjectInspector
xcopy GameData\KerbalObjectInspector %H%\GameData\KerbalObjectInspector\  /E /Y
