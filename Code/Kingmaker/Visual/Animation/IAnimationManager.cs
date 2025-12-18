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

	void CustomUpdate(float deltaTime);

	void AddAnimationClip(AnimationActionHandle animationActionHandle, AnimationClipWrapper clipWrapper, AvatarMask avatarMask, ClipDurationType duration);

	string DebugGetHierarchyPath();
}
