using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathProgressionPCView : CareerPathProgressionCommonView
{
	[Header("Inside Buttons")]
	[SerializeField]
	private OwlcatButton m_ReturnButton;

	[SerializeField]
	private TextMeshProUGUI m_ReturnLabel;

	[Header("ExpandButtons")]
	[ShowIf("m_CanMove")]
	[SerializeField]
	private OwlcatMultiButton m_StatsButton;

	[ShowIf("m_CanMove")]
	[SerializeField]
	private OwlcatMultiButton m_TooltipButton;

	[Header("OutsideButtons")]
	[SerializeField]
	private bool m_HasButtons;

	[SerializeField]
	[ShowIf("m_HasButtons")]
	private CareerButtonsBlock m_ButtonsBlock;

	public override void Initialize(Action<bool> returnAction)
	{
		if (m_HasButtons)
		{
			(m_CareerPathSelectionPartCommonView as CareerPathSelectionTabsPCView)?.SetButtonsBlock(m_ButtonsBlock);
			m_ButtonsBlock.SetActive(state: false);
		}
		base.Initialize(returnAction);
	}

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_ReturnButton.OnLeftClickAsObservable(), delegate
		{
			HandleReturn();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnCommit, delegate
		{
			UpdateButtonsState();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_StatsButton.OnLeftClickAsObservable(), delegate
		{
			SwitchDescriptionShowed(false);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_TooltipButton.OnLeftClickAsObservable(), delegate
		{
			SwitchDescriptionShowed(true);
		}).AddTo(this);
		m_ReturnLabel.text = UIStrings.Instance.CharacterSheet.BackToCareersList;
		m_AttentionSign.SetHint(UIStrings.Instance.CharacterSheet.AlreadyInLevelUp);
		UpdateButtonsState();
		TextHelper.AppendTexts(m_ReturnLabel);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ButtonsBlock.Or(null)?.SetActive(state: false);
	}

	private void UpdateButtonsState()
	{
		m_ButtonsBlock.Or(null)?.SetActive(base.ViewModel.IsInLevelupProcess);
	}
}
