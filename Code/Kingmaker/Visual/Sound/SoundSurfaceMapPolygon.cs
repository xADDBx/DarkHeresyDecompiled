using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Sound;

[ClassInfoBox("Global surface sound override.\nPlacement: Must be a child of the SoundSurfaceMap object in the scene.\nLogic: Checked FIRST. If a point is inside this polygon (XZ plane), all other detection (objects, terrain, water) is ignored.\nPriority: If polygons overlap, the one higher in the hierarchy (lower Sibling Index) takes precedence.")]
public class SoundSurfaceMapPolygon : PolygonComponent
{
	[HideInInspector]
	[FormerlySerializedAs("SoundSwitch")]
	public string SwitchString;

	public SurfaceType Switch;
}
