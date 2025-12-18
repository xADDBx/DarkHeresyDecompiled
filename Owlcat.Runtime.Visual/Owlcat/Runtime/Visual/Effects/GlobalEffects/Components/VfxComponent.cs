using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;

[Serializable]
[GlobalEffectComponentMenu("Particles/VFX")]
public class VfxComponent : ComponentBase
{
	public string CameraTag = "MainCamera";

	public List<VfxEffect> VfxEffects = new List<VfxEffect>();

	public override ControllerBase CreateController()
	{
		return new VfxController(this);
	}
}
