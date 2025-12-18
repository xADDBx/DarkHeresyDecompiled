using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[TypeId("1f6e13c5c1d94b6786ccb4aa8585cb3e")]
[PlayerUpgraderAllowed(false)]
public class AddVendorDiscount : GameAction
{
	[SerializeField]
	private FactionType m_Faction;

	[SerializeField]
	[Range(0f, 100f)]
	[Tooltip("Скидка в % относительно указанной в блюпринте предмета стоимости, стакается")]
	private int m_Discount;

	public override string GetCaption()
	{
		return $"Add {m_Discount} to all deals for {m_Faction}";
	}

	protected override void RunAction()
	{
		Game.Instance.VendorsManager.AddDiscount(m_Faction, m_Discount);
	}
}
