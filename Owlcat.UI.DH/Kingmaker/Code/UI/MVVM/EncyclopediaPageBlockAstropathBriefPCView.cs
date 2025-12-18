using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageBlockAstropathBriefPCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockAstropathBriefVM>
{
	[SerializeField]
	private TextMeshProUGUI m_MessageLocationLabel;

	[SerializeField]
	private TextMeshProUGUI m_MessageDateLabel;

	[SerializeField]
	private TextMeshProUGUI m_MessageSenderLabel;

	[SerializeField]
	private TextMeshProUGUI m_MessageBodyLabel;

	[SerializeField]
	private TextMeshProUGUI m_IsMessageReadLabel;

	[SerializeField]
	private GameObject m_IsMessageReadContainer;

	[SerializeField]
	private Color m_FieldNameColor;

	[SerializeField]
	private float m_DefaultFontSize = 21f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 21f;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.MessageLocation.Subscribe(SetMessageLocationText).AddTo(this);
		base.ViewModel.MessageDate.Subscribe(SetMessageDateText).AddTo(this);
		base.ViewModel.MessageSender.Subscribe(SetMessageSenderText).AddTo(this);
		base.ViewModel.MessageBody.Subscribe(delegate(string value)
		{
			m_MessageBodyLabel.text = value;
		}).AddTo(this);
		base.ViewModel.IsMessageRead.Subscribe(m_IsMessageReadContainer.SetActive).AddTo(this);
		m_IsMessageReadLabel.text = UIStrings.Instance.EncyclopediaTexts.AstropathBriefIsRead;
		SetTextFontSize();
		SetLinks();
	}

	private void SetTextFontSize()
	{
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_MessageLocationLabel.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_MessageDateLabel.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_MessageSenderLabel.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_MessageBodyLabel.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_IsMessageReadLabel.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
	}

	private void SetLinks()
	{
		TooltipConfig config = new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true);
		m_MessageLocationLabel.SetLinkTooltip(null, null, config).AddTo(this);
		m_MessageDateLabel.SetLinkTooltip(null, null, config).AddTo(this);
		m_MessageSenderLabel.SetLinkTooltip(null, null, config).AddTo(this);
		m_MessageBodyLabel.SetLinkTooltip(null, null, config).AddTo(this);
		m_IsMessageReadLabel.SetLinkTooltip(null, null, config).AddTo(this);
	}

	private void SetMessageLocationText(string value)
	{
		m_MessageLocationLabel.text = GetMessageField(UIStrings.Instance.EncyclopediaTexts.AstropathBriefLocation, value);
	}

	private void SetMessageDateText(string value)
	{
		m_MessageDateLabel.text = GetMessageField(UIStrings.Instance.EncyclopediaTexts.AstropathBriefDate, value);
	}

	private void SetMessageSenderText(string value)
	{
		m_MessageSenderLabel.text = GetMessageField(UIStrings.Instance.EncyclopediaTexts.AstropathBriefSender, value);
	}

	private string GetMessageField(string fieldName, string fieldValue)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(m_FieldNameColor) + ">" + fieldName + "</color> " + fieldValue;
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return new List<TextMeshProUGUI> { m_MessageLocationLabel, m_MessageDateLabel, m_MessageSenderLabel, m_MessageBodyLabel, m_IsMessageReadLabel };
	}
}
