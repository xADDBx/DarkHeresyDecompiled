using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpCharacteristicsItemView : CharGenLevelUpSelectorBaseItemView<CharGenLevelUpCharacteristicsItemVM>
{
	private const string SIGN_BUTTON_LAYER_FADED = "Faded";

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_AddedValue;

	[SerializeField]
	private Image m_BattleSkillMark;

	[SerializeField]
	private GameObject m_AttributeMark;

	[SerializeField]
	private OwlcatMultiSelectable m_BonusPenaltyState;

	[SerializeField]
	private TextMeshProUGUI m_MaxText;

	[SerializeField]
	private GameObject m_ChangeButtonsContainer;

	[SerializeField]
	private OwlcatMultiButton m_AddPointButton;

	[SerializeField]
	private OwlcatMultiButton m_RemovePointButton;

	private bool m_IsHovered;

	private readonly string[] hexColors = new string[9] { "#d8dad7", "#cddfcc", "#bae5b8", "#a7eca2", "#97f091", "#84f57e", "#67f963", "#47fd44", "#00ff12" };

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_BattleSkillMark != null)
		{
			m_BattleSkillMark.gameObject.SetActive(base.ViewModel.IsBattleStat);
			m_BattleSkillMark.SetHint(UIStrings.Instance.CharGen.LevelUpBattleSkillHint).AddTo(this);
		}
		if (m_Value != null)
		{
			base.ViewModel.Points.Subscribe(delegate(int p)
			{
				PaintValue(p);
			}).AddTo(this);
		}
		if (m_AddedValue != null)
		{
			base.ViewModel.AddedPoints.Subscribe(delegate(int p)
			{
				m_AddedValue.text = "+" + p;
			}).AddTo(this);
		}
		if (m_AttributeMark != null)
		{
			m_AttributeMark.SetActive(base.ViewModel.HasAttributeMark);
		}
		if (m_BonusPenaltyState != null)
		{
			m_BonusPenaltyState.SetActiveLayer(base.ViewModel.BonusPenaltyState.ToString());
		}
		if (m_MaxText != null)
		{
			m_MaxText.text = (base.ViewModel.HasSeveralPoints ? string.Empty : ((string)UIStrings.Instance.CharGen.SkillMaxValue));
		}
		if (base.ViewModel.HasSeveralPoints)
		{
			ObservableSubscribeExtensions.Subscribe(m_AddPointButton.OnLeftClickAsObservable(), delegate
			{
				ChangePoints(isAdding: true);
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_RemovePointButton.OnLeftClickAsObservable(), delegate
			{
				ChangePoints(isAdding: false);
			}).AddTo(this);
		}
		m_ChangeButtonsContainer.SetActive(base.ViewModel.HasSeveralPoints);
		base.ViewModel.HasPointsToSpend.Subscribe(delegate
		{
			UpdateChangePointButtons(m_IsHovered);
		}).AddTo(this);
	}

	protected override void UpdateAccessibility()
	{
		OwlcatMultiButton button = m_Button;
		button.SetActiveLayer(base.ViewModel.State.CurrentValue switch
		{
			LEVEL_UP_ITEM_STATE.Available => (base.ViewModel.IsSelected.Value && !base.ViewModel.HasSeveralPoints) ? "Chosen" : "Normal", 
			LEVEL_UP_ITEM_STATE.NotAvailable => "NotAvailable", 
			LEVEL_UP_ITEM_STATE.AlreadyExist => "NotAvailableTaken", 
			_ => "Normal", 
		});
		UpdateChangePointButtons(m_IsHovered);
	}

	protected override void OnHover(bool value)
	{
		base.OnHover(value);
		UpdateChangePointButtons(value);
		m_IsHovered = value;
		m_Button.SetFocused(value);
		UpdateAccessibility();
	}

	private void ChangePoints(bool isAdding)
	{
		base.ViewModel.ChangePoints(isAdding);
		base.ViewModel.SetSelectedFromView(state: true);
		UpdateChangePointButtons(isHovered: true);
	}

	private void UpdateChangePointButtons(bool isHovered)
	{
		if (base.ViewModel.HasSeveralPoints)
		{
			m_ChangeButtonsContainer.SetActive(isHovered);
			m_AddPointButton.gameObject.SetActive(base.ViewModel.HasPointsToSpend.CurrentValue);
			m_RemovePointButton.gameObject.SetActive(base.ViewModel.HasJustSpentPoints);
		}
	}

	public void PaintValue(int number)
	{
		if (!(m_Value == null))
		{
			m_Value.text = number.ToString();
		}
	}
}
