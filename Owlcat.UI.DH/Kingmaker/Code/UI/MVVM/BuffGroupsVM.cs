using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BuffGroupsVM : ViewModel
{
	private readonly List<Buff> m_StatusEffectsList = new List<Buff>();

	private readonly List<Buff> m_CriticalEffectsList = new List<Buff>();

	private readonly List<Buff> m_DotEffectsList = new List<Buff>();

	private readonly List<Buff> m_NegativeEffectsList = new List<Buff>();

	private readonly List<Buff> m_PositiveEffectsList = new List<Buff>();

	private readonly ReactiveProperty<IReadOnlyList<Buff>> m_StatusEffects;

	private readonly ReactiveProperty<IReadOnlyList<Buff>> m_CriticalEffects;

	private readonly ReactiveProperty<IReadOnlyList<Buff>> m_DotEffects;

	private readonly ReactiveProperty<IReadOnlyList<Buff>> m_NegativeEffects;

	private readonly ReactiveProperty<IReadOnlyList<Buff>> m_PositiveEffects;

	public ReadOnlyReactiveProperty<IReadOnlyList<Buff>> StatusEffects => m_StatusEffects;

	public ReadOnlyReactiveProperty<IReadOnlyList<Buff>> CriticalEffects => m_CriticalEffects;

	public ReadOnlyReactiveProperty<IReadOnlyList<Buff>> DotEffects => m_DotEffects;

	public ReadOnlyReactiveProperty<IReadOnlyList<Buff>> NegativeEffects => m_NegativeEffects;

	public ReadOnlyReactiveProperty<IReadOnlyList<Buff>> PositiveEffects => m_PositiveEffects;

	public BuffGroupsVM(MechanicEntity unit)
		: this()
	{
		UnitBuffBlockVM unitBuffBlockVM = new UnitBuffBlockVM(unit).AddTo(this);
		SubscribeToBuffs(unitBuffBlockVM.Buffs);
	}

	public BuffGroupsVM(IObservableCollection<BuffVM> buffs)
		: this()
	{
		SubscribeToBuffs(buffs);
	}

	private BuffGroupsVM()
	{
		m_StatusEffects = new ReactiveProperty<IReadOnlyList<Buff>>(m_StatusEffectsList).AddTo(this);
		m_CriticalEffects = new ReactiveProperty<IReadOnlyList<Buff>>(m_CriticalEffectsList).AddTo(this);
		m_DotEffects = new ReactiveProperty<IReadOnlyList<Buff>>(m_DotEffectsList).AddTo(this);
		m_NegativeEffects = new ReactiveProperty<IReadOnlyList<Buff>>(m_NegativeEffectsList).AddTo(this);
		m_PositiveEffects = new ReactiveProperty<IReadOnlyList<Buff>>(m_PositiveEffectsList).AddTo(this);
	}

	private void SubscribeToBuffs(IObservableCollection<BuffVM> buffs)
	{
		foreach (BuffVM buff in buffs)
		{
			HandleBuffAdded(buff);
		}
		buffs.ObserveAdd().Subscribe(HandleBuffAdded).AddTo(this);
		buffs.ObserveRemove().Subscribe(HandleBuffRemoved).AddTo(this);
		buffs.ObserveReset().Subscribe(HandleBuffsCleared).AddTo(this);
	}

	private void HandleBuffAdded(CollectionAddEvent<BuffVM> evt)
	{
		HandleBuffAdded(evt.Value);
	}

	private void HandleBuffAdded(BuffVM buffVM)
	{
		if (IsPositiveBuff(buffVM))
		{
			AddBuff(buffVM.Buff, m_PositiveEffectsList, m_PositiveEffects);
		}
		if (IsNegativeBuff(buffVM))
		{
			AddBuff(buffVM.Buff, m_NegativeEffectsList, m_NegativeEffects);
		}
		if (buffVM.Buff.IsDoT(out var _))
		{
			AddBuff(buffVM.Buff, m_DotEffectsList, m_DotEffects);
		}
		if (buffVM.Buff.IsCriticalEffect())
		{
			AddBuff(buffVM.Buff, m_CriticalEffectsList, m_CriticalEffects);
		}
		if (IsStatusEffect(buffVM.Buff))
		{
			AddBuff(buffVM.Buff, m_StatusEffectsList, m_StatusEffects);
		}
	}

	private void HandleBuffRemoved(CollectionRemoveEvent<BuffVM> evt)
	{
		if (IsPositiveBuff(evt.Value))
		{
			RemoveBuff(evt.Value.Buff, m_PositiveEffectsList, m_PositiveEffects);
		}
		if (IsNegativeBuff(evt.Value))
		{
			RemoveBuff(evt.Value.Buff, m_NegativeEffectsList, m_NegativeEffects);
		}
		if (evt.Value.Buff.IsDoT(out var _))
		{
			RemoveBuff(evt.Value.Buff, m_DotEffectsList, m_DotEffects);
		}
		if (evt.Value.Buff.IsCriticalEffect())
		{
			RemoveBuff(evt.Value.Buff, m_CriticalEffectsList, m_CriticalEffects);
		}
		if (IsStatusEffect(evt.Value.Buff))
		{
			RemoveBuff(evt.Value.Buff, m_StatusEffectsList, m_StatusEffects);
		}
	}

	private void AddBuff(Buff buffVM, List<Buff> list, ReactiveProperty<IReadOnlyList<Buff>> reactiveProperty)
	{
		list.Add(buffVM);
		reactiveProperty.ForceNotify();
	}

	private void RemoveBuff(Buff buffVM, List<Buff> list, ReactiveProperty<IReadOnlyList<Buff>> reactiveProperty)
	{
		if (list.Remove(buffVM))
		{
			reactiveProperty.ForceNotify();
		}
	}

	private void HandleBuffsCleared()
	{
		m_PositiveEffectsList.Clear();
		m_NegativeEffectsList.Clear();
		m_CriticalEffectsList.Clear();
		m_StatusEffectsList.Clear();
		m_DotEffectsList.Clear();
		m_PositiveEffects.ForceNotify();
		m_NegativeEffects.ForceNotify();
		m_CriticalEffects.ForceNotify();
		m_StatusEffects.ForceNotify();
		m_DotEffects.ForceNotify();
	}

	private bool IsPositiveBuff(BuffVM buffVM)
	{
		if (IsSpecialCategoryBuff(buffVM.Buff))
		{
			return false;
		}
		BuffUISettings buffUISettings = buffVM.Buff.Blueprint.BuffUISettings;
		if (buffUISettings != null && buffUISettings.BuffCategoryOverriden)
		{
			return buffUISettings.HasFlag(BuffUIFlags.Positive);
		}
		BaseUnitEntity owner = buffVM.Buff.Owner;
		if (owner == null)
		{
			return false;
		}
		return !owner.IsEnemy(buffVM.Buff.Context.MaybeCaster);
	}

	private bool IsNegativeBuff(BuffVM buffVM)
	{
		if (IsSpecialCategoryBuff(buffVM.Buff))
		{
			return false;
		}
		BuffUISettings buffUISettings = buffVM.Buff.Blueprint.BuffUISettings;
		if (buffUISettings != null && buffUISettings.BuffCategoryOverriden)
		{
			return buffUISettings.HasFlag(BuffUIFlags.Negative);
		}
		return buffVM.Buff.Owner?.IsEnemy(buffVM.Buff.Context.MaybeCaster) ?? false;
	}

	private bool IsSpecialCategoryBuff(Buff buff)
	{
		if (!buff.IsDoT(out var _) && !buff.IsCriticalEffect())
		{
			return IsStatusEffect(buff);
		}
		return true;
	}

	private bool IsStatusEffect(Buff buff)
	{
		BuffUISettings buffUISettings = buff.Blueprint.BuffUISettings;
		if (buffUISettings == null)
		{
			return false;
		}
		if (!buffUISettings.HasFlag(BuffUIFlags.LightStatusEffect) && !buffUISettings.HasFlag(BuffUIFlags.ModerateStatusEffect))
		{
			return buffUISettings.HasFlag(BuffUIFlags.SevereStatusEffect);
		}
		return true;
	}
}
