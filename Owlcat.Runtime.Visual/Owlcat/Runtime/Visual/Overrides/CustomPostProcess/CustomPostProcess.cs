using System;
using Owlcat.Runtime.Visual.CustomPostProcess;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides.CustomPostProcess;

[Serializable]
[VolumeComponentMenu("Post-processing/Custom Post Process New")]
public class CustomPostProcess : CompositeVolumeComponent<PostProcessCompositeParameter>, IPostProcessComponent
{
	internal bool IsEffectActive(CustomPostProcessEffect effect)
	{
		if (CompositeParameterList.overrideState)
		{
			foreach (PostProcessCompositeParameter item in CompositeParameterList.Value)
			{
				if (item.Effect == effect && item.overrideState)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal void ApplyPropertiesOverride(CustomPostProcessEffect effect, CustomPostProcessEffectPass effectPass, Material material)
	{
		foreach (PostProcessCompositeParameter item in CompositeParameterList.Value)
		{
			if (!(item.Effect == effect))
			{
				continue;
			}
			int num = effect.Passes.IndexOf(effectPass);
			{
				foreach (ShaderPropertyParameter parameter in item.Parameters)
				{
					if (parameter.PassIndex == num)
					{
						parameter.Property.UpdateMaterial(material);
					}
				}
				break;
			}
		}
	}

	public bool IsActive()
	{
		if (CompositeParameterList.overrideState)
		{
			foreach (PostProcessCompositeParameter item in CompositeParameterList.Value)
			{
				if (item.overrideState)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
