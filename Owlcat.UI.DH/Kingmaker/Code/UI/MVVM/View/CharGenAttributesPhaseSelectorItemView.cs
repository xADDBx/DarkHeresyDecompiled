using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAttributesPhaseSelectorItemView : CharGenBackgroundBasePhaseSelectorItemView<CharGenAttributesItemVM>, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, IConsoleEntity, INavigationRightDirectionHandler, IHasTooltipTemplate
{
	[SerializeField]
	private TextMeshProUGUI m_StatValue;

	[SerializeField]
	private OwlcatMultiSelectable m_StatValueSelectable;

	[SerializeField]
	private TextMeshProUGUI m_ShortLabel;

	[SerializeField]
	private CharGenAttributesPhasePantographItemView m_PantographItemView;

	[SerializeField]
	private OwlcatMultiButton m_RanksButton;

	[SerializeField]
	private GameObject m_RecommendedMark;

	[SerializeField]
	private List<GameObject> m_FullRanks;

	private readonly ReactiveProperty<string> m_Hint = new ReactiveProperty<string>(string.Empty);

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}

	public bool HandleLeft()
	{
		if (!base.ViewModel.CanRetreat.CurrentValue)
		{
			return false;
		}
		base.ViewModel.RetreatStat();
		return true;
	}

	public bool HandleRight()
	{
		if (!base.ViewModel.CanAdvance.CurrentValue)
		{
			return false;
		}
		base.ViewModel.AdvanceStat();
		return true;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.StatValue.CombineLatest(base.ViewModel.CanRetreat, (int stat, bool canRetreat) => new { stat, canRetreat }).Subscribe(value =>
		{
			m_StatValue.text = value.stat.ToString();
		}));
		AddDisposable(base.ViewModel.DiffValue.Subscribe(UpdateState));
		AddDisposable(base.ViewModel.StatRanks.Subscribe(delegate(int value)
		{
			for (int i = 0; i < m_FullRanks.Count; i++)
			{
				m_FullRanks[i].SetActive(i < value);
			}
			ReactiveProperty<string> hint = m_Hint;
			string text = UIStrings.Instance.CharGen.SkillPointsContainerHint.Text;
			int valuePerRank = base.ViewModel.ValuePerRank;
			hint.Value = string.Format(text, valuePerRank.ToString(), value + " / " + m_FullRanks.Count);
		}));
		m_ShortLabel.text = UIUtilityText.GetStatShortName(base.ViewModel.StatType);
		AddDisposable(m_Button.OnHoverAsObservable().Subscribe(base.ViewModel.OnHovered));
		if ((bool)m_RecommendedMark)
		{
			AddDisposable(base.ViewModel.IsRecommended.Subscribe(m_RecommendedMark.SetActive));
		}
		AddDisposable(m_RanksButton.SetHint(m_Hint));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_RanksButton.OnLeftClickAsObservable(), delegate
		{
			base.PointerLeftClickCommand.Execute();
			OnClick();
		}));
	}

	protected override void SetupPantographConfig()
	{
		base.PantographConfig = new PantographConfig(base.transform, m_PantographItemView, base.ViewModel);
	}

	private void UpdateState(int diffValue)
	{
		string text = ((diffValue < 0) ? "Negative" : ((diffValue <= 0) ? "Default" : "Positive"));
		string activeLayer = text;
		m_StatValueSelectable.SetActiveLayer(activeLayer);
		m_StatValue.fontStyle = ((diffValue != 0) ? FontStyles.Bold : FontStyles.Normal);
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelectedFromView(value);
	}
}
