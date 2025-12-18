using System;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using R3;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("31d6da0fb0a880f4d85844cea65b02b0")]
public class DropLootAndDestroyOnDeactivate : UnitBuffComponentDelegate
{
	private IDisposable m_Coroutine;

	protected override void OnActivate()
	{
		Buff buff = base.Buff;
		m_Coroutine = ObservableSubscribeExtensions.Subscribe(Observable.Timer(base.Buff.ExpirationInRounds.Rounds().Seconds), delegate
		{
			buff.Remove();
		});
		if (!base.Owner.Faction.IsPlayer)
		{
			base.Owner.Inventory.DropLootToGround(dismember: true);
		}
	}

	protected override void OnDeactivate()
	{
		m_Coroutine?.Dispose();
		m_Coroutine = null;
		if ((bool)base.Owner.GetOptional<UnitPartSummonedMonster>())
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(base.Owner);
		}
		else if (!base.Owner.Faction.IsPlayer)
		{
			base.Owner.IsInGame = false;
		}
	}
}
