using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Obsolete]
[ComponentName("Evaluators/MaxPartySize")]
[AllowMultipleComponents]
[TypeId("896d9214a03d0a54b88ff6848d05b33d")]
public class MaxPartySize : IntEvaluator
{
	protected override int GetValueInternal()
	{
		return Game.Instance.Player.MaxPartySize;
	}

	public override string GetCaption()
	{
		return "Max Party Size";
	}
}
