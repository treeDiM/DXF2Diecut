DXF2Diecut
==========

Simple command line executable that helps converting a dxf file to either *.cf2 or *.ai (format used for diecut documents)
It is written in C#.

Command line examples:
1. Convert an AutoCAD dxf file to Common File Format (*.cf2) :
 	'DXF2DieCut.exe --i "K:\GitHub\DXF2Diecut\Samples\2429.dxf" --o "K:\GitHub\DXF2Diecut\Samples\2429.cf2" --v'

2. Convert an AutoCAD dxf file to Adobe Illustrator format(*.ai) : 
	'DXF2DieCut.exe --i "K:\GitHub\DXF2Diecut\Samples\2429.dxf" --o "K:\GitHub\DXF2Diecut\Samples\2429.ai" --v'
