using System.Linq;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides.Vfx;

[VolumeComponentMenu("Global Effects/Particles/Vfx Override")]
public class VfxGraphOverride : CompositeVolumeComponent<VfxCompositeParameter>
{
	protected override void OnEnable()
	{
		base.OnEnable();
		InitVfxParameters();
	}

	internal void Apply(VisualEffect vfxInstance)
	{
		VfxCompositeParameter vfxCompositeParameter = CompositeParameterList.Value.FirstOrDefault((VfxCompositeParameter p) => p.VfxAsset == vfxInstance.visualEffectAsset);
		if (vfxCompositeParameter == null)
		{
			return;
		}
		foreach (VfxPropertyParameter parameter in vfxCompositeParameter.Parameters)
		{
			parameter.Apply(vfxInstance);
		}
	}

	private void InitVfxParameters()
	{
		foreach (VfxCompositeParameter item in CompositeParameterList.Value)
		{
			foreach (VfxPropertyParameter parameter in item.Parameters)
			{
				parameter.InitNameId();
				parameter.ResetToDefault();
			}
		}
	}
}
