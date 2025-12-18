using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Concentration.Events;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class ChannelingAsksController : BaseAsksController, IConcentrationBrokenHandler, ISubscriber<IMechanicEntity>, ISubscriber, IChannellingStart, IChannellingSuccessfulRelease
{
	public void HandleChannelingStart()
	{
		EventInvokerExtensions.AbstractUnitEntity?.View.Asks?.ChannellingOn.Schedule(is2D: false, ReactToConcentrationOn);
	}

	private void ReactToConcentrationOn(AsksContext asksContext)
	{
		List<BaseUnitEntity> party = Game.Instance.Player.Party;
		MechanicEntity caster = asksContext.Caster;
		AbstractUnitEntity abstractUnitEntity = Game.Instance.Player.MainCharacter.Get<AbstractUnitEntity>();
		float num = float.MaxValue;
		foreach (BaseUnitEntity item in party)
		{
			if (item.CanAct)
			{
				float num2 = Vector3.Distance(item.Position, caster.Position);
				if (num2 < num)
				{
					num = num2;
					abstractUnitEntity = item;
				}
			}
		}
		abstractUnitEntity?.View.Asks?.ChannellingReaction.Schedule();
	}

	public void HandleConcentrationBroken(MechanicEntity reason)
	{
		EventInvokerExtensions.AbstractUnitEntity?.View.Asks?.ChannellingOff.Schedule();
	}

	public void HandleSuccessfulRelease()
	{
		EventInvokerExtensions.AbstractUnitEntity?.View.Asks?.ChannellingSuccessfulRelease.Schedule();
	}
}
