using System;
using Kingmaker.View.Mechanics;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventFootStep : AnimationClipEventSound
{
	public AnimationClipEventFootStep()
	{
	}

	public AnimationClipEventFootStep(float time)
		: this(time, null)
	{
	}

	public AnimationClipEventFootStep(float time, string name)
		: base(time, isLooped: false, name, null, 1f)
	{
	}

	public override Action PostSoundEvent(MechanicEntityView view)
	{
		view.PlayFootstep(base.Name);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventFootStep(base.Time, base.Name);
	}

	public override string ToString()
	{
		return $"Footstep event at {base.Time}";
	}
}
