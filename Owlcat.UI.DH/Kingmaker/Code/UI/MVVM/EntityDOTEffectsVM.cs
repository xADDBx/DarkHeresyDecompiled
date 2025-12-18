using System.Collections.Generic;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EntityDOTEffectsVM : ViewModel
{
	private readonly ReactiveProperty<DOTEffectsUIData> m_DotEffects;

	private readonly List<(DOT dotType, int rank)> m_DotEffectsList;

	private MechanicEntity m_TargetEntity;

	public ReadOnlyReactiveProperty<DOTEffectsUIData> DOTEffects => m_DotEffects;

	public EntityDOTEffectsVM(MechanicEntity target = null)
	{
		m_DotEffectsList = new List<(DOT, int)>(3);
		m_DotEffects = new ReactiveProperty<DOTEffectsUIData>(new DOTEffectsUIData
		{
			DotEffects = m_DotEffectsList
		}).AddTo(this);
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
		if (buff.Owner == m_TargetEntity && IsDoT(buff, out var dotType))
		{
			AddDoT(dotType, buff.Rank);
		}
	}

	public void HandleBuffRemoved(Buff buff)
	{
		if (buff.Owner == m_TargetEntity && IsDoT(buff, out var dotType))
		{
			RemoveDoT(dotType);
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
		if (buff.Owner == m_TargetEntity && IsDoT(buff, out var dotType))
		{
			AddDoT(dotType, buff.Rank);
		}
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
		if (buff.Owner == m_TargetEntity && IsDoT(buff, out var dotType))
		{
			AddDoT(dotType, buff.Rank);
		}
	}

	private void CollectBuffs(MechanicEntity target)
	{
		m_DotEffectsList.Clear();
		foreach (Buff buff in target.Buffs)
		{
			if (IsDoT(buff, out var dotType))
			{
				AddDoT(dotType, buff.Rank, notify: false);
			}
		}
		m_DotEffects.ForceNotify();
	}

	private void AddDoT(DOT dotType, int rank, bool notify = true)
	{
		int num = m_DotEffectsList.FindIndex(((DOT dotType, int rank) i) => i.dotType == dotType);
		if (num < 0)
		{
			m_DotEffectsList.Add((dotType, rank));
		}
		else
		{
			m_DotEffectsList[num] = (dotType, rank);
		}
		if (notify)
		{
			m_DotEffects.ForceNotify();
		}
	}

	private void RemoveDoT(DOT dotType)
	{
		int num = m_DotEffectsList.FindIndex(((DOT dotType, int rank) i) => i.dotType == dotType);
		if (num >= 0)
		{
			m_DotEffectsList.RemoveAt(num);
			m_DotEffects.ForceNotify();
		}
	}

	private static bool IsDoT(Buff buff, out DOT dotType)
	{
		DOTLogic dOTLogic = buff.Blueprint?.GetComponent<DOTLogic>();
		dotType = dOTLogic?.Type ?? DOT.Bleeding;
		return dOTLogic != null;
	}
}
