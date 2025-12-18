using Kingmaker.Utility.DotNetExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickEventsView : TooltipBaseBrickView<TooltipBrickEventVM>
{
	[SerializeField]
	private TextMeshProUGUI m_EventName;

	[SerializeField]
	private TextMeshProUGUI m_EventDescription;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private EventRelationTypeParams[] EventRelationTypeParams;

	[SerializeField]
	private float m_DefaultFontSizeName = 24f;

	[SerializeField]
	private float m_DefaultFontSizeDescription = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeName = 24f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeDescription = 18f;

	protected override void OnBind()
	{
		base.OnBind();
		m_EventName.text = base.ViewModel.EventName;
		m_EventDescription.text = base.ViewModel.EventDescription;
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_EventName.fontSize = (isControllerMouse ? m_DefaultFontSizeName : m_DefaultConsoleFontSizeName) * m_FontMultiplier;
		m_EventDescription.fontSize = (isControllerMouse ? m_DefaultFontSizeDescription : m_DefaultConsoleFontSizeDescription) * m_FontMultiplier;
		ApplyType(base.ViewModel.Type);
	}

	private void ApplyType(EventRelationType type)
	{
		EventRelationTypeParams paramsByType = GetParamsByType(type);
		m_EventDescription.color = paramsByType.TypeColor;
		m_Icon.sprite = paramsByType.Icon;
		m_Background.color = paramsByType.TypeColor;
	}

	private EventRelationTypeParams GetParamsByType(EventRelationType type)
	{
		return EventRelationTypeParams.FirstItem((EventRelationTypeParams i) => i.Type == type);
	}
}
