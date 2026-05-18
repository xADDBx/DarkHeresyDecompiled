using System.Linq;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Events;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[TypeId("d206814c13d04e8c8c1f241143c9f8b3")]
public class BlueprintAnimationActionExternalHandler : BlueprintScriptableObject
{
	private BlueprintAnimationActionExternalBaseComponent m_BlueprintAnimationActionExternalBaseComponent;

	public void Handle(AnimationManager animationManager, ClipEventType сlipEventType, int id, object userData)
	{
		if (m_BlueprintAnimationActionExternalBaseComponent == null)
		{
			if (base.ComponentsArray.Length == 0)
			{
				m_BlueprintAnimationActionExternalBaseComponent = new BlueprintAnimationActionExternalSimpleComponent();
			}
			else
			{
				m_BlueprintAnimationActionExternalBaseComponent = (BlueprintAnimationActionExternalBaseComponent)base.ComponentsArray.FirstOrDefault();
			}
		}
		m_BlueprintAnimationActionExternalBaseComponent?.Handle(animationManager, сlipEventType, id, userData);
	}
}
