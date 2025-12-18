using System;
using System.Collections.Generic;
using Code.GameCore.ElementsSystem;
using Kingmaker.Utility.CodeTimer;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
public class ActionList : ElementsList, IHashable
{
	public enum ExceptionHandlingMode
	{
		DoNotThrow,
		ThrowImmediately,
		ThrowAfterListIsComplete
	}

	[ValidateNotNull]
	[SerializeReference]
	public GameAction[] Actions = new GameAction[0];

	private readonly List<Exception> m_Exceptions = new List<Exception>(8);

	public override IEnumerable<Element> Elements => Actions;

	public bool HasActions
	{
		get
		{
			GameAction[] actions = Actions;
			if (actions != null)
			{
				return actions.Length > 0;
			}
			return false;
		}
	}

	public void Run(ExceptionHandlingMode mode = ExceptionHandlingMode.DoNotThrow, Func<GameAction, bool> filter = null)
	{
		using (ProfileScope.New("ActionList"))
		{
			using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(this);
			m_Exceptions.Clear();
			GameAction[] actions = Actions;
			foreach (GameAction gameAction in actions)
			{
				if (filter != null && !filter(gameAction))
				{
					continue;
				}
				try
				{
					gameAction?.Run(this);
				}
				catch (Exception ex)
				{
					if (m_Exceptions.Count <= 0)
					{
						elementsDebugger?.SetException(ex);
					}
					if (mode == ExceptionHandlingMode.ThrowImmediately)
					{
						throw;
					}
					m_Exceptions.Add(ex);
				}
			}
			if (m_Exceptions.Count <= 0)
			{
				elementsDebugger?.SetResult(1);
			}
			else if (mode == ExceptionHandlingMode.ThrowAfterListIsComplete)
			{
				throw new AggregateException(m_Exceptions);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
