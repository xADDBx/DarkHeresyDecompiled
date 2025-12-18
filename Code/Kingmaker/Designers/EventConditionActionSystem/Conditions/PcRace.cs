using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("a4b53d63845adf841bc2dd21895bdb6d")]
public class PcRace : Condition
{
	public Race Race;

	protected override string GetConditionCaption()
	{
		return $"PC Race ({Race})";
	}

	protected override bool CheckCondition()
	{
		return Race == Game.Instance.Player.MainCharacterEntity?.ToBaseUnitEntity().Progression.Race?.RaceId;
	}
}
