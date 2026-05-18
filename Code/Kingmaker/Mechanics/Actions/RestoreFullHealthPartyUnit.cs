using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("41287fc5bca34087b6ccf71bef420196")]
public class RestoreFullHealthPartyUnit : ContextAction
{
	[SerializeField]
	[HideIf("Evaluate")]
	private BlueprintUnitReference? m_TargetPartyUnit;

	[ShowIf("Evaluate")]
	[SerializeReference]
	public AbstractUnitEvaluator? UnitEvaluator;

	[SerializeField]
	public bool Evaluate;

	public override string GetCaption()
	{
		string text = ((!Evaluate) ? m_TargetPartyUnit?.Get()?.CharacterName : UnitEvaluator?.GetCaption());
		return "Restore full health for " + (text ?? "<null>");
	}

	protected override void RunAction()
	{
		BlueprintUnit blueprintUnit = ((!Evaluate) ? ((BlueprintUnit)m_TargetPartyUnit) : UnitEvaluator?.GetValue().Blueprint);
		if (blueprintUnit == null)
		{
			return;
		}
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.Blueprint == blueprintUnit)
			{
				item.GetHealthOptional()?.HealDamageAll();
				break;
			}
		}
	}
}
