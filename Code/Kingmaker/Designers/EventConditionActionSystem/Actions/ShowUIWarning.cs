using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("62afa88a2a2107349814e8c9f57c410c")]
public class ShowUIWarning : GameAction
{
	public WarningNotificationType Type;

	[HideIf("HasType")]
	public LocalizedString String;

	private bool HasType => Type != WarningNotificationType.None;

	public override string GetCaption()
	{
		return "Show notification (" + (HasType ? Type.ToString() : String.ToString()) + ")";
	}

	protected override void RunAction()
	{
		if (HasType)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(Type);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(String.ToString());
			});
		}
	}
}
