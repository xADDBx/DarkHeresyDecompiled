using Kingmaker.Utility.CodeTimer;
using UnityEngine;
using UnityEngine.Animations;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipWrapperStateMachineBehaviour : StateMachineBehaviour
{
	public AnimationClipWrapper AnimationClipWrapper;

	private AnimationManager m_AnimationManagerCached;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		using (ProfileScope.New("AnimationClipWrapperStateMachineBehaviour.OnStateEnter"))
		{
			_ = AnimationClipWrapper == null;
		}
	}
}
