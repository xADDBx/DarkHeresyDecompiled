using Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;

public abstract class OverridableControllerBase<ComponentType, OverrideType> : ControllerBase<ComponentType> where ComponentType : ComponentBase where OverrideType : VolumeComponent
{
	public OverrideType VolumeOverride { get; private set; }

	public OverridableControllerBase(ComponentType component)
		: base(component)
	{
	}

	private OverrideType FindVolumeOverride(VolumeStack stack)
	{
		if (stack != null)
		{
			return stack.GetComponent<OverrideType>();
		}
		return null;
	}

	internal override void UpdateInternal(GlobalEffectContext context)
	{
		VolumeOverride = FindVolumeOverride(context.VolumeStack);
		Update(context);
	}
}
