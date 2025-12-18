using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/CurrentPartySize")]
[AllowMultipleComponents]
[TypeId("594eb1641121be9419363a2f68ab0d7d")]
public class CurrentPartySize : IntEvaluator
{
	protected override int GetValueInternal()
	{
		return Game.Instance.Player.Party.Count;
	}

	public override string GetCaption()
	{
		return "Current Party Size";
	}
}
