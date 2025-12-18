using Kingmaker.Blueprints.Base;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("17b2f07a8f4a08441b95a6f177937451")]
public class PcMale : Condition
{
	protected override string GetConditionCaption()
	{
		return "PC Male";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.MainCharacter.Entity.Gender == Gender.Male;
	}
}
