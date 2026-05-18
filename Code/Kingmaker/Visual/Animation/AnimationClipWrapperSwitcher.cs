using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public abstract class AnimationClipWrapperSwitcher : ScriptableObject
{
	public abstract AnimationClipWrapper GetWrapper(IAnimationManager animationManager);

	public abstract IEnumerable<AnimationClipWrapper> EnumerateClips();
}
