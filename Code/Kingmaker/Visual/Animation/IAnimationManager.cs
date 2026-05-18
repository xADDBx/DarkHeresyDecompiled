using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Visual.Animation.Actions;
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

public interface IAnimationManager
{
	bool IsValid { get; }

	DirectorUpdateMode UpdateMode { get; set; }

	float PlayingSpeed { get; }

	UnitAnimationCallbackReceiver CallbackReceiver { get; }

	AnimationSoundEventsManager SoundEventsManager { get; }

	StatefulRandom StatefulRandom { get; }

	GameObject GameObject { get; }

	void CustomUpdate(float deltaTime);

	void AddAnimationClip(AnimationActionHandle animationActionHandle, AnimationClipWrapper clipWrapper, AvatarMask avatarMask, ClipDurationType duration);

	string DebugGetHierarchyPath();
}
