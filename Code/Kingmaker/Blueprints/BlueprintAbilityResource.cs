using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UIDataProvider;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Blueprints;

[ComponentName("Ability/BlueprintAbilityResource")]
[TypeId("973c613b8443cf14495c283e293d35f9")]
public class BlueprintAbilityResource : BlueprintScriptableObject, IUIDataProvider
{
	[Serializable]
	private struct Amount
	{
		[UsedImplicitly]
		public int BaseValue;

		[UsedImplicitly]
		public bool IncreasedByLevel;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevel")]
		public int LevelIncrease;

		[UsedImplicitly]
		public bool IncreasedByLevelStartPlusDivStep;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int StartingLevel;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int StartingIncrease;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int LevelStep;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int PerStepIncrease;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int MinClassLevelIncrease;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public float OtherClassesModifier;

		[UsedImplicitly]
		public bool IncreasedByStat;

		[UsedImplicitly]
		[ShowIf("IncreasedByStat")]
		public StatType ResourceBonusStat;
	}

	[NotNull]
	public LocalizedString LocalizedName;

	[NotNull]
	public LocalizedString LocalizedDescription;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	private Amount m_MaxAmount;

	[SerializeField]
	private bool m_UseMax;

	[SerializeField]
	[ShowIf("m_UseMax")]
	private int m_Max = 10;

	[SerializeField]
	[InfoBox("Resource would be restored to at least this amount (Useful for MaxAmount dependent on stat modifier, that can be negative)")]
	private int m_Min;

	public string Name => LocalizedName;

	public string Description => LocalizedDescription;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => name;

	public int GetMaxAmount(Entity entity)
	{
		int num = m_MaxAmount.BaseValue;
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return num;
		}
		if (m_MaxAmount.IncreasedByStat)
		{
			num += baseUnitEntity.Actor.GetStatBonus(m_MaxAmount.ResourceBonusStat);
		}
		if (m_MaxAmount.IncreasedByLevelStartPlusDivStep)
		{
			int num2 = 0;
			num2 += (int)((float)(baseUnitEntity.Progression.CharacterLevel - num2) * m_MaxAmount.OtherClassesModifier);
			if (m_MaxAmount.StartingLevel <= num2)
			{
				num += Math.Max(m_MaxAmount.StartingIncrease + m_MaxAmount.PerStepIncrease * (num2 - m_MaxAmount.StartingLevel) / m_MaxAmount.LevelStep, m_MaxAmount.MinClassLevelIncrease);
			}
		}
		int bonus = 0;
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<IResourceAmountBonusHandler>)delegate(IResourceAmountBonusHandler h)
		{
			h.CalculateMaxResourceAmount(this, ref bonus);
		}, isCheckRuntime: true);
		return Math.Max(m_Min, ApplyMinMax(num) + bonus);
	}

	private int ApplyMinMax(int result)
	{
		if (m_UseMax)
		{
			result = Math.Min(result, m_Max);
		}
		return result;
	}
}
