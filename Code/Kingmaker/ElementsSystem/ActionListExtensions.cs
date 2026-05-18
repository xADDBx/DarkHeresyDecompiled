using System.Collections.Generic;
using Kingmaker.Designers.EventConditionActionSystem.Actions;

namespace Kingmaker.ElementsSystem;

public static class ActionListExtensions
{
	public static bool ContainsActionOfType<T>(this ActionList list) where T : GameAction
	{
		return ContainsActionOfType<T>(list?.Actions);
	}

	public static void AssembleActionsOfType<T>(this ActionList list, List<T> result) where T : GameAction
	{
		AssembleActionsOfType(list?.Actions, result);
	}

	private static bool ContainsActionOfType<T>(GameAction[] actions) where T : GameAction
	{
		if (actions == null)
		{
			return false;
		}
		foreach (GameAction gameAction in actions)
		{
			if (gameAction is T)
			{
				return true;
			}
			if (gameAction is Conditional conditional && ContainsActionOfType<T>((conditional.ConditionsChecker.Check() ? conditional.IfTrue : conditional.IfFalse)?.Actions))
			{
				return true;
			}
		}
		return false;
	}

	private static void AssembleActionsOfType<T>(GameAction[] actions, List<T> result) where T : GameAction
	{
		if (actions == null || result == null)
		{
			return;
		}
		foreach (GameAction obj in actions)
		{
			if (obj is T item)
			{
				result.Add(item);
			}
			if (obj is Conditional conditional)
			{
				AssembleActionsOfType((conditional.ConditionsChecker.Check() ? conditional.IfTrue : conditional.IfFalse)?.Actions, result);
			}
		}
	}
}
