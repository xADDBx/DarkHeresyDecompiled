using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;

namespace Kingmaker.Visual.GlobalEffects;

[GlobalEffectComponentMenu("SoundComponent")]
public class SoundComponent : ComponentBase
{
	[AkEventReference]
	public string SoundEventName;

	public override ControllerBase CreateController()
	{
		return new SoundController(this);
	}
}
