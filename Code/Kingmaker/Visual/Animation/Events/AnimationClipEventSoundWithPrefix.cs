using System;
using Kingmaker.View.Mechanics;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventSoundWithPrefix : AnimationClipEventSound
{
	public AnimationClipEventSoundWithPrefix()
	{
	}

	public AnimationClipEventSoundWithPrefix(float time)
		: this(time, isLooped: false, null, 1f)
	{
	}

	public AnimationClipEventSoundWithPrefix(float time, bool isLooped, string name, float volume)
		: base(time, isLooped, name, null, volume)
	{
	}

	public override Action PostSoundEvent(MechanicEntityView view)
	{
		uint uniqueId = view.PostSoundEventWithPrefix(base.Name, base.Volume);
		if (!base.IsLooped)
		{
			return null;
		}
		return delegate
		{
			view.StopPlayingSoundById(uniqueId);
		};
	}

	public override object Clone()
	{
		return new AnimationClipEventSoundWithPrefix(base.Time, base.IsLooped, base.Name, base.Volume);
	}

	public override string ToString()
	{
		return $"{base.Name} sound event with prefix at {base.Time}";
	}
}
