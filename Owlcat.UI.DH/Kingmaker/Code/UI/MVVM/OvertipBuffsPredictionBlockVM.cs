using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipBuffsPredictionBlockVM : ViewModel
{
	public struct BuffPrediction
	{
		public int ApplyChance;

		public Sprite Icon;
	}

	private readonly MechanicEntityUIState m_EntityUIState;

	private readonly List<BuffPrediction> m_BuffPredictions;

	private readonly ReactiveProperty<IReadOnlyList<BuffPrediction>> m_Buffs;

	private readonly ReactiveProperty<bool> m_IsVisible;

	public ReadOnlyReactiveProperty<IReadOnlyList<BuffPrediction>> Buffs => m_Buffs;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public OvertipBuffsPredictionBlockVM(MechanicEntityUIState entityUIState)
	{
		m_BuffPredictions = new List<BuffPrediction>();
		m_Buffs = new ReactiveProperty<IReadOnlyList<BuffPrediction>>(m_BuffPredictions).AddTo(this);
		m_IsVisible = new ReactiveProperty<bool>().AddTo(this);
		m_EntityUIState = entityUIState;
		entityUIState.AbilityTargetUIData.CombineLatest(entityUIState.IsVisibleForPlayer, entityUIState.IsMouseOverUnit, entityUIState.IsInCombat, (AbilityTargetUIData ability, bool isVisible, bool isHovered, bool isInCombat) => (ability: ability, isHovered && isInCombat && isVisible)).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(HandleBuffsStateChanged)
			.AddTo(this);
	}

	private void HandleBuffsStateChanged((AbilityTargetUIData abilityData, bool isVisible) data)
	{
		IReadOnlyList<(BlueprintBuff, int)> predictedBuffs = data.abilityData.Buffs.PredictedBuffs;
		bool flag = predictedBuffs != null && predictedBuffs.Count > 0;
		bool flag2 = data.isVisible && flag;
		m_IsVisible.Value = flag2 && !m_EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.BuffPrediction);
		if (!flag2)
		{
			return;
		}
		m_BuffPredictions.Clear();
		foreach (var (blueprintBuff, applyChance) in data.abilityData.Buffs.PredictedBuffs)
		{
			m_BuffPredictions.Add(new BuffPrediction
			{
				Icon = blueprintBuff.Icon,
				ApplyChance = applyChance
			});
		}
		m_Buffs.ForceNotify();
	}
}
