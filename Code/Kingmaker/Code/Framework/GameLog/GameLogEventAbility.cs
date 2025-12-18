using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventAbility : GameLogEventBrackets<GameLogEventAbility>
{
	[UsedImplicitly]
	private class EventHandle : GameLogController.GameEventsHandler, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber
	{
		public void HandleExecutionProcessStart(AbilityExecutionContext context)
		{
			AddEvent(new GameLogEventAbility(context));
		}

		public void HandleExecutionProcessEnd(AbilityExecutionContext context)
		{
			GetEventFromQueue((GameLogEventAbility i) => i.Context == context)?.MarkReady();
		}
	}

	public readonly AbilityExecutionContext Context;

	public readonly List<GameLogEventAttack> ScatterAttacks = new List<GameLogEventAttack>();

	public AbilityData Ability => Context.Ability;

	public bool IsScatter => Ability.IsBurst;

	public bool IsAoe => Ability.IsAoe;

	public GameLogEventAbility(AbilityExecutionContext context)
	{
		Context = context;
	}

	protected override bool TryHandleInnerEventInternal(GameLogEvent @event)
	{
		IRulebookEvent first = Rulebook.CurrentContext.First;
		if (first == null)
		{
			return false;
		}
		if (((RulebookEvent)first).Reason.Context != Context)
		{
			return false;
		}
		if (@event is GameLogEventAttack item)
		{
			ScatterAttacks.Add(item);
		}
		return true;
	}
}
