using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Visual.Sound;

[ClassInfoBox("Local surface sound zone for complex objects.\nPlacement: Should be located on the 'switch' object or within a prefab containing a SoundSurfaceMapObject.\nLogic: Only processed if a Raycast hits the object's collider first.\nPurpose: Used to specify different surface types for specific parts of a single 3D model (e.g., a rug or a puddle on a stone floor).")]
public class SoundSurfaceMapObjectPolygon : PolygonComponent
{
	public SurfaceType Switch;
}
