using System;
using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("431090ddff9f4c01901bdee140fdcb1a")]
public class ContextActionChangeTurnOrderWithinRound : ContextAction
{
	[SerializeField]
	private int m_Steps;

	public int Steps => m_Steps;

	public override string GetCaption()
	{
		return $"Changes target unit turn order {m_Steps:+#;-#;0} steps within round";
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null || m_Steps == 0)
		{
			return;
		}
		List<MechanicEntity> list = Game.Instance.Controllers.TurnController.TurnOrder.UnitsOrder.ToTempList();
		int num = list.IndexOf(entity);
		if (num < 0)
		{
			return;
		}
		int num2 = Mathf.Clamp(num + m_Steps, 0, list.Count - 1);
		int num3 = ((m_Steps > 0) ? 1 : (-1));
		Initiative.BulkSwapFluent bulkSwapFluent = entity.Initiative.BulkSwapStart();
		while (num != num2)
		{
			num += num3;
			bulkSwapFluent.Swap(list[num].Initiative);
		}
		bulkSwapFluent.Finish();
		AbilityExecutionContext abilityContext = base.AbilityContext;
		if (abilityContext != null && abilityContext.Ability.IsPrecise)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Target.Entity, (Action<IBodyPartHitAdditionalEffect>)delegate(IBodyPartHitAdditionalEffect h)
			{
				h.HandleBodyPartHitChangeTurn(base.AbilityContext?.Ability.PreciseBodyPart);
			}, isCheckRuntime: true);
		}
	}
}
