using System;
using System.Text;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
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
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventAnswerView : View<AnswerVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, ISettingsFontSizeUIHandler, ISubscriber, IHasBlueprintInfo
{
	[SerializeField]
	protected TextMeshProUGUI m_AnswerText;

	[SerializeField]
	protected OwlcatMultiButton m_OwlcatButton;

	[SerializeField]
	private string m_SpoilerText = string.Empty;

	[SerializeField]
	private TooltipConfig m_TooltipConfig;

	[SerializeField]
	private DialogAnswerGraphicConfig m_DialogAnswerGraphicConfig;

	[SerializeField]
	private TextStyle m_AlignmentShiftStyle;

	public OwlcatMultiButton Button => m_OwlcatButton;

	public BlueprintScriptableObject Blueprint => base.ViewModel?.Blueprint;

	protected override void OnBind()
	{
		TextMeshProUGUI answerText = m_AnswerText;
		if ((object)answerText.styleSheet == null)
		{
			TMP_StyleSheet tMP_StyleSheet = (answerText.styleSheet = m_AlignmentShiftStyle.StyleSheet);
		}
		EventBus.Subscribe(this).AddTo(this);
		base.gameObject.SetActive(value: true);
		base.gameObject.name = $"AnswerView_{base.ViewModel.Index}";
		SetupAnswerText();
		SetupKeyBind();
		SetupTooltip();
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
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
		m_AnswerText.text = GetBindingText() + TryGetConditionText() + TryGetSkillCheckRequirementText() + TryGetSkillCheckText() + TryGetExchangeText() + TryGetConvictionRequirementsText() + TryGetConvictionShiftText() + (base.ViewModel.ShowSpoiler ? m_SpoilerText : base.ViewModel.AnswerRawText);
	}

	private string GetBindingText()
	{
		if (!base.ViewModel.IsBookEvent)
		{
			return base.ViewModel.BindingDisplayText + ". ";
		}
		return "-//" + base.ViewModel.BindingDisplayText + " --- ";
	}

	private string TryGetConvictionShiftText()
	{
		if (!base.ViewModel.HasVisibleConvictionShifts)
		{
			return string.Empty;
		}
		return UIUtilityAlignment.GetAlignmentText(base.ViewModel.Answer.AlignmentShifts).ApplyStyle(m_AlignmentShiftStyle);
	}

	private string TryGetConvictionRequirementsText()
	{
		if (!base.ViewModel.HasVisibleConvictionRequirements)
		{
			return string.Empty;
		}
		return string.Format(UIStrings.Instance.Dialog.AlignmentRequirementFormat, UIUtilityAlignment.GetAlignmentRequirementText(base.ViewModel.Answer));
	}

	private string TryGetConditionText()
	{
		if (!base.ViewModel.HasConditions)
		{
			return string.Empty;
		}
		string text = EntityLink.Type.DialogConditions.ToString();
		string assetGuid = base.ViewModel.AssetGuid;
		string text2 = (base.ViewModel.CanSelect ? m_DialogAnswerGraphicConfig.ConditionSuccessSpriteID : m_DialogAnswerGraphicConfig.ConditionFailSpriteID);
		string text3 = (base.ViewModel.CanSelect ? m_DialogAnswerGraphicConfig.ConditionSuccessSpriteColor.HTML() : m_DialogAnswerGraphicConfig.ConditionFailSpriteColor.HTML());
		return "<link=\"" + text + ":" + assetGuid + "><sprite name=\"" + text2 + "\" color=#" + text3 + "></link>";
	}

	private string TryGetSkillCheckRequirementText()
	{
		if (!base.ViewModel.HasVisibleSkillCheckRequirement)
		{
			return string.Empty;
		}
		string text = m_DialogAnswerGraphicConfig.SkillcheckRequirementSucceededInAnswerColor.HTML();
		string text2 = UtilityLink.PackKeys(EntityLink.Type.SkillcheckDC, base.ViewModel.AnswerSkillcheckRequirement.Type);
		string conditionSuccessSpriteID = m_DialogAnswerGraphicConfig.ConditionSuccessSpriteID;
		string text3 = m_DialogAnswerGraphicConfig.ConditionSuccessSpriteColor.HTML();
		string text4 = LocalizedTexts.Instance.Stats.GetText(base.ViewModel.AnswerSkillcheckRequirement.Type);
		string text5 = UIStrings.Instance.Dialog.Succeeded.Text;
		return "<color=#" + text + "><link=\"" + text2 + "\"><sprite name=\"" + conditionSuccessSpriteID + "\" color=#" + text3 + "> " + text4 + ": " + text5 + " | </link></color> ";
	}

	private string TryGetExchangeText()
	{
		if (!base.ViewModel.HasVisibleExchangeData)
		{
			return string.Empty;
		}
		string text = EntityLink.Type.DialogExchange.ToString();
		string exchangeIDText = base.ViewModel.ExchangeIDText;
		string dialogExchangeSpriteID = m_DialogAnswerGraphicConfig.DialogExchangeSpriteID;
		string text2 = m_DialogAnswerGraphicConfig.DialogExchangeSpriteColor.HTML();
		return "<link=" + text + ":" + exchangeIDText + "><sprite name=\"" + dialogExchangeSpriteID + "\" color=#" + text2 + "></link> ";
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

	public void Confirm()
	{
		ButtonsSounds.Instance.Default.Click.Play();
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			base.ViewModel?.OnChooseAnswer();
		});
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
}
