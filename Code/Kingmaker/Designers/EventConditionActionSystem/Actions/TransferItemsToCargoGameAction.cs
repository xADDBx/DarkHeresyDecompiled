using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[ComponentName("Actions/SellCollectibleItems")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("ba07b06141cb08f44a197690bf49a923")]
public class TransferItemsToCargoGameAction : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("ItemToSell")]
	private BlueprintItemReference m_ItemToSell;

	public BlueprintItem ItemToSell => m_ItemToSell?.Get();

	public override string GetCaption()
	{
		return $"Transfer all ({ItemToSell}) to cargo";
	}

	protected override void RunAction()
	{
	}
}
