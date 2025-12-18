using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c44f8f7b59c1b9145af6c8d5e4481a8d")]
public class CreateCustomCompanion : GameAction
{
	[SerializeReference]
	public LocatorEvaluator Locator;

	public bool ForFree;

	public bool MatchPlayerXpExactly;

	public CharGenCompanionType CompanionType;

	public ActionList OnCreate = new ActionList();

	public override string GetDescription()
	{
		return $"Вызывает окно создания компаньона, если достаточно профит фактора. \nКомпаньона можно сделать бесплатным галкой ForFree ({ForFree})\n Можно подтянуть опыт компаньона под опыт персонажа галкой MatchPlayerXpExactly ({MatchPlayerXpExactly})\n Можно выполнить экшены при создании";
	}

	public override string GetCaption()
	{
		return "Create custom companion";
	}

	protected override void RunAction()
	{
		throw new NotImplementedException();
	}
}
