using System;
using Kingmaker.ResourceLinks;

namespace Kingmaker.Blueprints.Root.Fx;

[Serializable]
public class HologramEntry
{
	public PrefabLink MainFx;

	public PrefabLink ThreateningMainFx;

	public MaterialLink HoloMaterial;

	public MaterialLink ThreateningHoloMaterial;
}
