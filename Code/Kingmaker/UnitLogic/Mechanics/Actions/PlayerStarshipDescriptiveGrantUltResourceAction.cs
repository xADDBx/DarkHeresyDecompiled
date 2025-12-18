using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[ComponentName("Actions/PlayerStarshipDescriptiveGrantUltResourceAction")]
[AllowMultipleComponents]
[TypeId("75e3029993d72e845986afa840aa24bc")]
public class PlayerStarshipDescriptiveGrantUltResourceAction : GameAction
{
	private enum AmountValue
	{
		Low,
		Average,
		High
	}

	[SerializeField]
	private AmountValue m_AmountValue = AmountValue.Average;

	[SerializeField]
	private RestoreResourcesSet.RestoreMode m_RestoreMode;

	[SerializeField]
	private bool LoseInstead;

	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		string arg = (LoseInstead ? "Make player starship lose" : "Grant player starship");
		return $"{arg} \"{m_AmountValue}\" amount of ultimate abilities resources in \"{m_RestoreMode}\" mode";
	}
}
