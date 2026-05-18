using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public abstract class CasterBuffsInfoBaseVM<TBuffInfo> : ViewModel
{
	private readonly ObservableList<TBuffInfo> m_Buffs;

	private TargetWrapper m_Target;

	private ReadOnlyReactiveProperty<AbilityData> m_TargetedAbility;

	private IDisposable m_Disposable;

	protected CasterBuffsInfoBaseVM()
	{
		m_Buffs = new ObservableList<TBuffInfo>();
		EventBus.Subscribe(this).AddTo(this);
	}

	public void SetTargetEntity(MechanicEntityUIState entityUIState)
	{
		m_Disposable?.Dispose();
		MechanicEntity mechanicEntity = entityUIState?.MechanicEntity.MechanicEntity;
		if (mechanicEntity == null)
		{
			m_Target = null;
			m_TargetedAbility = null;
			m_Disposable = null;
			return;
		}
		m_Target = mechanicEntity;
		m_TargetedAbility = entityUIState.Ability;
		m_Disposable = m_TargetedAbility.Subscribe(delegate(AbilityData ability)
		{
			UpdateCasterBuffs(ability, m_Target);
		}).AddTo(this);
	}

	public Observable<CollectionAddEvent<TBuffInfo>> ObserveBuffAdded()
	{
		return m_Buffs.ObserveAdd();
	}

	public Observable<Unit> ObserveBuffsCleared()
	{
		return m_Buffs.ObserveReset();
	}

	protected abstract void AddBuffIfRelevant(Buff buff, IEvalContext context, ICollection<TBuffInfo> buffInfos);

	protected override void OnDispose()
	{
		m_Disposable?.Dispose();
		m_Buffs.Clear();
	}

	private void UpdateCasterBuffs(AbilityData abilityData, TargetWrapper target)
	{
		m_Buffs.Clear();
		BuffCollection buffCollection = abilityData?.Caster?.Buffs;
		if (target == null || buffCollection == null)
		{
			return;
		}
		Vector3 position = abilityData.Caster.Position;
		IEvalContext ctx;
		using (EvalContext.PushAbility(abilityData, target, null, position).Get(out ctx))
		{
			foreach (Buff item in buffCollection)
			{
				AddBuffIfRelevant(item, ctx, m_Buffs);
			}
		}
	}
}
