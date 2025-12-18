using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Actions;

[TypeId("9fa60a45d7bb49c8b66643edeb2fd087")]
public class ExecuteFirstAvailableAction : GameAction
{
	[Serializable]
	private class ActionWithCondition
	{
		public ConditionsChecker ShowConditions;

		[ValidateNotNull]
		[SerializeReference]
		public GameAction Action;
	}

	[SerializeField]
	[UsedImplicitly]
	private List<ActionWithCondition> m_Actions = new List<ActionWithCondition>();

	public override string GetCaption()
	{
		return "Execute first action passed by conditions";
	}

	protected override void RunAction()
	{
		m_Actions.FirstOrDefault((ActionWithCondition a) => a.ShowConditions.Check() && a.Action != null)?.Action.Run();
	}
}
