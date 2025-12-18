using System;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.MVVM.Dialog;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Dialog;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class DialogAnswerBaseView : View<AnswerVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, ISettingsFontSizeUIHandler, ISubscriber, IHasBlueprintInfo
{
	[Header("Elements")]
	[SerializeField]
	protected TextMeshProUGUI m_NumberText;

	[SerializeField]
	protected TextMeshProUGUI m_AnswerText;

	[SerializeField]
	protected OwlcatMultiButton m_OwlcatButton;

	[Header("Values")]
	[SerializeField]
	private string m_SpoilerText = string.Empty;

	[SerializeField]
	private TooltipConfig m_TooltipConfig;

	[SerializeField]
	private DialogAnswerGraphicConfig m_DialogAnswerGraphicConfig;

	[SerializeField]
	private TextStyle m_DetectiveRelatedItemsStyle;

	[Space]
	[Header("VotesCoop")]
	[SerializeField]
	protected DialogVotesBlockView m_DialogVotesBlock;

	public BlueprintScriptableObject Blueprint => base.ViewModel?.Blueprint;

	protected override void OnBind()
	{
		EventBus.Subscribe(this).AddTo(this);
		m_AnswerText.styleSheet = m_DetectiveRelatedItemsStyle.StyleSheet;
		base.gameObject.SetActive(value: true);
		base.gameObject.name = $"AnswerView_{base.ViewModel.Index}";
		ObjectExtensions.Or(m_DialogVotesBlock, null)?.ShowHideHover(state: false);
		SetupAnswerText();
		SetupAnswerState();
		SetupKeyBind();
		SetupTooltip();
		base.ViewModel.WasChoose.Subscribe(delegate
		{
			SetupAnswerState();
		}).AddTo(this);
		ObjectExtensions.Or(m_DialogVotesBlock, null)?.Bind(base.ViewModel.DialogVotesBlockVM);
		base.ViewModel.VotedPlayersChanged.Subscribe(CheckCoopPlayersVotes).AddTo(this);
		CheckCoopPlayersVotes();
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		ObjectExtensions.Or(m_DialogVotesBlock, null)?.ShowHideHover(state: false);
	}

	private void SetupTooltip()
	{
		if (base.ViewModel.HasTooltipData)
		{
			m_AnswerText.SetLinkTooltip(base.ViewModel.AllSkillChecks, null, m_TooltipConfig).AddTo(this);
		}
	}

	private void SetupAnswerText()
	{
		if (m_NumberText != null)
		{
			m_NumberText.text = GetBindingText();
		}
		m_AnswerText.text = TryGetConditionText() + TryGetSkillCheckRequirementText() + TryGetSkillCheckText() + TryGetRelatedDetectiveItems() + TryGetExchangeText() + TryGetConvictionRequirementsText() + TryGetConvictionShiftText() + (base.ViewModel.IsRequirementSatisfied ? base.ViewModel.AnswerRawText : m_SpoilerText);
	}

	private string GetBindingText()
	{
		if (!base.ViewModel.IsBookEvent)
		{
			return "-//" + base.ViewModel.BindingDisplayText + "-. ";
		}
		return "-//" + base.ViewModel.BindingDisplayText + " --- ";
	}

	private string TryGetConvictionShiftText()
	{
		if (!base.ViewModel.HasVisibleConvictionShifts)
		{
			return string.Empty;
		}
		return base.ViewModel.ConvictionShiftText;
	}

	private string TryGetConvictionRequirementsText()
	{
		if (!base.ViewModel.HasVisibleConvictionRequirements)
		{
			return string.Empty;
		}
		return string.Format(UIDialog.Instance.AlignmentRequirementLabel, base.ViewModel.ConvictionDirection, base.ViewModel.ConvictionRankText);
	}

	private string TryGetConditionText()
	{
		if (!base.ViewModel.HasConditions)
		{
			return string.Empty;
		}
		string assetGuid = base.ViewModel.AssetGuid;
		string text = (base.ViewModel.CanSelect ? m_DialogAnswerGraphicConfig.ConditionSuccessSpriteID : m_DialogAnswerGraphicConfig.ConditionFailSpriteID);
		string text2 = (base.ViewModel.CanSelect ? m_DialogAnswerGraphicConfig.ConditionSuccessSpriteColor.HTML() : m_DialogAnswerGraphicConfig.ConditionFailSpriteColor.HTML());
		return "<link=\"DialogConditions:" + assetGuid + "><sprite name=\"" + text + "\" color=#" + text2 + "></link>";
	}

	private string TryGetSkillCheckRequirementText()
	{
		if (!base.ViewModel.HasVisibleSkillCheckRequirement)
		{
			return string.Empty;
		}
		StatType type = base.ViewModel.AnswerSkillcheckRequirement.Type;
		string text = m_DialogAnswerGraphicConfig.SkillcheckRequirementSucceededInAnswerColor.HTML();
		string text2 = UtilityLink.PackKeys(EntityLink.Type.SkillcheckDC, type);
		string conditionSuccessSpriteID = m_DialogAnswerGraphicConfig.ConditionSuccessSpriteID;
		string text3 = m_DialogAnswerGraphicConfig.ConditionSuccessSpriteColor.HTML();
		string text4 = LocalizedTexts.Instance.Stats.GetText(type);
		string text5 = UIStrings.Instance.Dialog.Succeeded.Text;
		return "<color=#" + text + "><link=\"" + text2 + "\"><sprite name=\"" + conditionSuccessSpriteID + "\" color=#" + text3 + "> " + text4 + ": " + text5 + " | </link></color> ";
	}

	private string TryGetExchangeText()
	{
		if (!base.ViewModel.HasVisibleExchangeData)
		{
			return string.Empty;
		}
		string exchangeIDText = base.ViewModel.ExchangeIDText;
		string dialogExchangeSpriteID = m_DialogAnswerGraphicConfig.DialogExchangeSpriteID;
		string text = m_DialogAnswerGraphicConfig.DialogExchangeSpriteColor.HTML();
		return "<link=DialogExchange:" + exchangeIDText + "><sprite name=\"" + dialogExchangeSpriteID + "\" color=#" + text + "></link> ";
	}

	private string TryGetRelatedDetectiveItems()
	{
		if (!base.ViewModel.HasRelatedDetectiveItems)
		{
			return string.Empty;
		}
		using (GameLogContext.Scope)
		{
			string assetGuid = base.ViewModel.AssetGuid;
			string dialogDetectiveRelatedItemsSpriteID = m_DialogAnswerGraphicConfig.DialogDetectiveRelatedItemsSpriteID;
			string text = m_DialogAnswerGraphicConfig.DialogExchangeSpriteColor.HTML();
			GameLogContext.TextStyle = m_DetectiveRelatedItemsStyle;
			string text2 = UIStrings.Instance.Dialog.HasRelatedItems.Text;
			return "<link=RelatedDetectiveItems:" + assetGuid + "><sprite name=\"" + dialogDetectiveRelatedItemsSpriteID + "\" color=#" + text + "> " + text2 + " </link> ";
		}
	}

	private void SetupAnswerState()
	{
		AnswerVM viewModel = base.ViewModel;
		if (viewModel.WasChoose.CurrentValue)
		{
			m_OwlcatButton.SetActiveLayer("Choose");
			return;
		}
		if (!viewModel.CanSelect)
		{
			m_OwlcatButton.SetActiveLayer("Forbidden");
			return;
		}
		AnswerVM answerVM = viewModel;
		if (answerVM.IsAlreadySelected && !answerVM.IsCurrentUnselectedWithNewAnswers)
		{
			m_OwlcatButton.SetActiveLayer("AlreadySelected");
		}
		else
		{
			m_OwlcatButton.SetActiveLayer("Normal");
		}
	}

	private string TryGetSkillCheckText()
	{
		if (!base.ViewModel.HasVisibleSkillChecks)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			foreach (SkillCheckDC skillCheck in base.ViewModel.SkillChecks)
			{
				string text = ((skillCheck.StatType == StatType.Unknown) ? m_DialogAnswerGraphicConfig.SkillcheckInAnswerErrorColor.HTML() : m_DialogAnswerGraphicConfig.SkillcheckInAnswerColor.HTML());
				string text2 = UtilityLink.PackKeys(EntityLink.Type.SkillcheckDC, skillCheck.StatType);
				string text3 = LocalizedTexts.Instance.Stats.GetText(skillCheck.StatType);
				string text4 = $"{UtilitySkillcheck.GetSkillCheckChance(skillCheck)}%";
				string diceSpriteID = m_DialogAnswerGraphicConfig.DiceSpriteID;
				stringBuilder.Append("<color=#" + text + "><link=\"" + text2 + "\"><sprite name=\"" + diceSpriteID + "\" color=#" + m_DialogAnswerGraphicConfig.DiceSpriteColor.HTML() + ">" + text3 + ": " + text4 + " | </link></color>");
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Error(ex);
		}
		return stringBuilder.ToString();
	}

	private void SetupKeyBind()
	{
		Game.Instance.Keyboard.Bind(base.ViewModel.BindingName, Confirm).AddTo(this);
		if (base.ViewModel.IsSystem)
		{
			Game.Instance.Keyboard.Bind("NextOrEnd", Confirm).AddTo(this);
		}
	}

	public virtual void UpdateTextSize(float multiplier)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	void IConsoleNavigationEntity.SetFocus(bool value)
	{
		m_OwlcatButton.SetFocus(value);
	}

	bool IConsoleNavigationEntity.IsValid()
	{
		return base.ViewModel != null;
	}

	bool IConfirmClickHandler.CanConfirmClick()
	{
		return m_OwlcatButton.CanConfirmClick();
	}

	void IConfirmClickHandler.OnConfirmClick()
	{
		Confirm();
	}

	string IConfirmClickHandler.GetConfirmClickHint()
	{
		return string.Empty;
	}

	void ISettingsFontSizeUIHandler.HandleChangeFontSizeSettings(float size)
	{
		UpdateTextSize(size);
	}

	public void Confirm()
	{
		if (!base.ViewModel.TryDoCoopPing())
		{
			UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				base.ViewModel?.OnChooseAnswer();
			}).AddTo(this);
		}
	}

	private void CheckCoopPlayersVotes()
	{
		if (!(m_DialogVotesBlock == null))
		{
			m_DialogVotesBlock.CheckVotesPlayers(base.ViewModel.AnswerVotes);
		}
	}
}
