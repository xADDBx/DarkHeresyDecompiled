using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[AllowMultipleComponents]
[TypeId("ec82ae3ae961f344db74d76bff432d11")]
public class ReloadUI : GameAction
{
	public override string GetCaption()
	{
		return "Reload UI";
	}

	protected override void RunAction()
	{
		Game.Instance.RootUIContext.ResetUI();
	}
}
