using System.Collections.Generic;

namespace Kingmaker.Visual.Animation.WeaponStyles;

public interface IWeaponStyleAnimationClipsProvider
{
	IEnumerable<AnimationClipWrapper> EnumerateClips();
}
