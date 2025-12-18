using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c920786099320fb4bb9c3947accc0a64")]
public class ShowMultiEntrance : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintMultiEntranceReference m_Map;

	public BlueprintMultiEntrance Entrance => m_Map?.Get();

	public override string GetCaption()
	{
		return "Show entrance map " + Entrance;
	}

	protected override void RunAction()
	{
		if ((bool)Entrance)
		{
			EventBus.RaiseEvent(delegate(IUIMultiEntranceHandler h)
			{
				h.HandleMultiEntranceUI(Entrance);
			});
		}
	}
}
