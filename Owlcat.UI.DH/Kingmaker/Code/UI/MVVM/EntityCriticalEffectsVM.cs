using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EntityCriticalEffectsVM : ViewModel
{
	private readonly ReactiveProperty<CriticalEffectsUIData> m_CriticalEffectsData;

	private readonly Dictionary<BlueprintBodyPart, int> m_StagesByBodyPart = new Dictionary<BlueprintBodyPart, int>();

	private MechanicEntity m_TargetEntity;

	public ReadOnlyReactiveProperty<CriticalEffectsUIData> CriticalEffectsData => m_CriticalEffectsData;

	public EntityCriticalEffectsVM(MechanicEntity target = null)
	{
		m_CriticalEffectsData = new ReactiveProperty<CriticalEffectsUIData>().AddTo(this);
		if (target != null)
		{
			SetTargetEntity(target);
		}
	}

	public void SetTargetEntity(MechanicEntity target)
	{
		m_TargetEntity = target;
		CollectCriticalEffects(target);
	}

	public void HandleCriticalEffectStageChanged(BlueprintBodyPart bodyPart, int currentStage)
	{
		UpdateCriticalEffect(bodyPart, currentStage);
		int highestRank = GetHighestRank();
		m_CriticalEffectsData.Value = new CriticalEffectsUIData
		{
			Count = m_StagesByBodyPart.Count,
			HighestRank = highestRank
		};
	}

	private void CollectCriticalEffects(MechanicEntity target)
	{
		m_StagesByBodyPart.Clear();
		IEnumerable<BlueprintBodyPart> bodyParts = target.BodyParts;
		int num = 0;
		foreach (BlueprintBodyPart item in bodyParts)
		{
			BpRef<BlueprintBuff> criticalEffect = item.CriticalEffect;
			if ((object)criticalEffect != null)
			{
				int num2 = target.Buffs.Get((BlueprintBuff?)criticalEffect)?.Rank ?? 0;
				num = Mathf.Max(num2, num);
				if (num2 >= 1)
				{
					m_StagesByBodyPart.Add(item, num2);
				}
			}
		}
		m_CriticalEffectsData.Value = new CriticalEffectsUIData
		{
			Count = m_StagesByBodyPart.Count,
			HighestRank = num
		};
	}

	private void UpdateCriticalEffect(BlueprintBodyPart bodyPart, int currentStage)
	{
		if (EventInvokerExtensions.MechanicEntity == m_TargetEntity)
		{
			if (currentStage > 0)
			{
				m_StagesByBodyPart[bodyPart] = currentStage;
			}
			else if (m_StagesByBodyPart.ContainsKey(bodyPart))
			{
				m_StagesByBodyPart.Remove(bodyPart);
			}
		}
	}

	private int GetHighestRank()
	{
		int num = 0;
		foreach (KeyValuePair<BlueprintBodyPart, int> item in m_StagesByBodyPart)
		{
			item.Deconstruct(out var _, out var value);
			num = Mathf.Max(value, num);
		}
		return num;
	}
}
