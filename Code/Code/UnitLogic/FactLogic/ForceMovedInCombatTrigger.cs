using Kingmaker;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Code.UnitLogic.FactLogic;

[TypeId("1b9b9ff888514eecae3e46e84c091f01")]
public class ForceMovedInCombatTrigger : UnitFactComponentDelegate, IUnitGetAbilityPush, ISubscriber
{
	[InfoBox("Current ForceMoved Distance is written into ContextValue1")]
	[SerializeField]
	private ActionList m_Actions;

	public void HandleUnitResultPush(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint)
	{
	}

	public void HandleUnitAbilityPushDidActed(int distanceInCells)
	{
		if (distanceInCells >= 1)
		{
			if (base.Context == null)
			{
				PFLog.Default.Error("ForceMovedInCombatTrigger. DistanceInCells won't be written into Context: can`t find one");
			}
			else
			{
				EvalContext.Current[ContextPropertyName.Value1] = distanceInCells;
			}
			base.Fact.RunActionInContext(m_Actions, base.Owner);
		}
	}

	public void HandleUnitResultPush(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster)
	{
	}
}
