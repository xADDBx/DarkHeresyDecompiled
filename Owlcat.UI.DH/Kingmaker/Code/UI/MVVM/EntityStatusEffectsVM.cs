using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EntityStatusEffectsVM : ViewModel
{
	private readonly Dictionary<Buff, BuffVM> m_StatusEffectsDictionary = new Dictionary<Buff, BuffVM>();

	private readonly ReactiveProperty<StatusEffectsUIData> m_StatusEffects;

	private MechanicEntity m_TargetEntity;

	public ReadOnlyReactiveProperty<StatusEffectsUIData> StatusEffects => m_StatusEffects;

	public EntityStatusEffectsVM(MechanicEntity target = null)
	{
		m_StatusEffects = new ReactiveProperty<StatusEffectsUIData>().AddTo(this);
		if (target != null)
		{
			SetTargetEntity(target);
		}
	}

	public void SetTargetEntity(MechanicEntity target)
	{
		m_TargetEntity = target;
		CollectBuffs(target);
	}

	public void HandleBuffAdded(Buff buff)
	{
		if (buff.Owner == m_TargetEntity && IsStatusEffect(buff) && TryAddBuff(buff))
		{
			UpdateReactiveProperty();
		}
	}

	public void HandleBuffRemoved(Buff buff)
	{
		if (buff.Owner == m_TargetEntity && IsStatusEffect(buff) && TryRemoveBuff(buff))
		{
			UpdateReactiveProperty();
		}
	}

	protected override void OnDispose()
	{
		foreach (KeyValuePair<Buff, BuffVM> item in m_StatusEffectsDictionary)
		{
			item.Deconstruct(out var _, out var value);
			value.Dispose();
		}
		m_StatusEffectsDictionary.Clear();
	}

	private void CollectBuffs(MechanicEntity target)
	{
		m_StatusEffectsDictionary.Clear();
		foreach (Buff buff in target.Buffs)
		{
			if (IsStatusEffect(buff))
			{
				TryAddBuff(buff);
			}
		}
		UpdateReactiveProperty();
	}

	private bool TryAddBuff(Buff buff)
	{
		if (m_StatusEffectsDictionary.ContainsKey(buff))
		{
			return false;
		}
		m_StatusEffectsDictionary.Add(buff, new BuffVM(buff));
		return true;
	}

	private bool TryRemoveBuff(Buff buff)
	{
		if (!m_StatusEffectsDictionary.TryGetValue(buff, out var value))
		{
			return false;
		}
		m_StatusEffectsDictionary.Remove(buff);
		value.Dispose();
		return true;
	}

	private void UpdateReactiveProperty()
	{
		int num = 0;
		foreach (KeyValuePair<Buff, BuffVM> item in m_StatusEffectsDictionary)
		{
			item.Deconstruct(out var key, out var _);
			num = Mathf.Max((int)GetBuffSeverity(key.Blueprint), num);
		}
		m_StatusEffects.Value = new StatusEffectsUIData
		{
			Count = m_StatusEffectsDictionary.Count,
			HighestSeverity = (StatusEffectSeverity)num
		};
		static StatusEffectSeverity GetBuffSeverity(BlueprintBuff blueprintBuff)
		{
			BuffUISettings buffUISettings = blueprintBuff.BuffUISettings;
			if (buffUISettings.HasFlag(BuffUIFlags.LightStatusEffect))
			{
				return StatusEffectSeverity.Light;
			}
			if (buffUISettings.HasFlag(BuffUIFlags.ModerateStatusEffect))
			{
				return StatusEffectSeverity.Moderate;
			}
			if (buffUISettings.HasFlag(BuffUIFlags.SevereStatusEffect))
			{
				return StatusEffectSeverity.Severe;
			}
			return StatusEffectSeverity.None;
		}
	}

	private static bool IsStatusEffect(Buff buff)
	{
		BuffUISettings buffUISettings = buff.Blueprint?.BuffUISettings;
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
