using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.ApplyTriggerForTargets;

[Serializable]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dd3968a7603141b69735cedad21f7df1")]
public class ApplyTriggerForTargets : UnitFactComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public enum Selectors
	{
		Min,
		Max
	}

	[SerializeField]
	private Selectors Selector;

	[SerializeField]
	private PropertyCalculator TargetProperty;

	[SerializeField]
	private RestrictionCalculator Restriction;

	[SerializeField]
	private ActionList Actions;

	public bool OnlyOwnerTurn;

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased || (OnlyOwnerTurn && EventInvokerExtensions.MechanicEntity != base.Owner))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = null;
		int num = 0;
		List<BaseUnitEntity> allBaseAwakeUnits = Game.Instance.EntityPools.AllBaseAwakeUnits;
		for (int i = 0; i < allBaseAwakeUnits.Count; i++)
		{
			BaseUnitEntity baseUnitEntity2 = allBaseAwakeUnits[i];
			PropertyContext context = new PropertyContext(baseUnitEntity2);
			if (baseUnitEntity2 == base.Owner || !baseUnitEntity2.IsInCombat || !Restriction.IsPassed(context))
			{
				continue;
			}
			int value = TargetProperty.GetValue(context);
			if (baseUnitEntity == null)
			{
				baseUnitEntity = baseUnitEntity2;
				num = value;
				continue;
			}
			switch (Selector)
			{
			case Selectors.Min:
				if (value < num)
				{
					baseUnitEntity = baseUnitEntity2;
					num = value;
				}
				break;
			case Selectors.Max:
				if (value > num)
				{
					baseUnitEntity = baseUnitEntity2;
					num = value;
				}
				break;
			}
		}
		if (baseUnitEntity != null)
		{
			base.Fact.RunActionInContext(Actions, baseUnitEntity.ToITargetWrapper());
		}
	}
}
