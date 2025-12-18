using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("f2edd3ea3b1f38b429fdca720c313e95")]
public class ContextActionDestroyAreaEffect : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("AreaEffect")]
	private BlueprintAreaEffectReference m_AreaEffect;

	public BlueprintAreaEffect AreaEffect => m_AreaEffect?.Get();

	public override string GetCaption()
	{
		string text = ((AreaEffect != null) ? AreaEffect.ToString() : "<undefined>");
		return "Destroy " + text + " ";
	}

	protected override void RunAction()
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Blueprint == AreaEffect)
			{
				areaEffect.ForceEnd();
			}
		}
	}
}
