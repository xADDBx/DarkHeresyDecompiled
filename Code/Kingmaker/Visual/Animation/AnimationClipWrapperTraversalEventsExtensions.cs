using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Events;

namespace Kingmaker.Visual.Animation;

public static class AnimationClipWrapperTraversalEventsExtensions
{
	public static float GetStartTraverseTime(this AnimationClipWrapper clip)
	{
		if (!clip.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventLinkTraversal animationClipEventLinkTraversal && animationClipEventLinkTraversal.Type == TraversalAnimationEventType.StartTraverse, out var result))
		{
			return 0f;
		}
		return result.Time;
	}

	public static float GetEndTraverseTime(this AnimationClipWrapper clip)
	{
		if (!clip.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventLinkTraversal animationClipEventLinkTraversal && animationClipEventLinkTraversal.Type == TraversalAnimationEventType.EndTraverse, out var result))
		{
			return clip.Length;
		}
		return result.Time;
	}

	public static float GetMoveToTraverseTime(this AnimationClipWrapper clip)
	{
		if (!clip.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventLinkTraversal animationClipEventLinkTraversal && animationClipEventLinkTraversal.Type == TraversalAnimationEventType.MoveToTraverse, out var result))
		{
			return 0f;
		}
		return result.Time;
	}

	public static float GetTraverseToMoveTime(this AnimationClipWrapper clip)
	{
		if (!clip.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventLinkTraversal animationClipEventLinkTraversal && animationClipEventLinkTraversal.Type == TraversalAnimationEventType.TraverseToMove, out var result))
		{
			return clip.Length;
		}
		return result.Time;
	}
}
