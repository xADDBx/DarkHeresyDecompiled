using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Events;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[TypeId("9c5060991a3e328459fcb883218e5052")]
public class BlueprintAnimationActionExternalSimpleComponent : BlueprintAnimationActionExternalBaseComponent
{
	public override void Handle(AnimationManager animationManager, ClipEventType сlipEventType, int id, object userData)
	{
		ObjectExtensions.Or(animationManager.GetComponentInParent<ClipEventExternalReceiver>() ?? animationManager.GetComponent<ClipEventExternalReceiver>(), null)?.StartEvent(сlipEventType, animationManager, id, userData);
	}
}
