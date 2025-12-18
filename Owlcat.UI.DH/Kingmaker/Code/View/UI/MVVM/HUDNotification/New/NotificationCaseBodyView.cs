using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationCaseBodyView : View<NotificationCaseBodyVM>
{
	[Header("Elements")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Description;

	protected override void OnBind()
	{
		TooltipConfig config = new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, GetComponent<RectTransform>(), 0, 0, 0, new List<Vector2>
		{
			new Vector2(0f, 0.5f)
		});
		UIDetectiveJournal detectiveJournal = UIStrings.Instance.DetectiveJournal;
		if (base.ViewModel.Case == null)
		{
			m_Icon.sprite = UIConfig.Instance.DetectiveConfig.UnknownCluesIcon;
			m_Description.text = detectiveJournal.UnknownCluesDescription.Text;
			TooltipTemplateSimple template = new TooltipTemplateSimple(detectiveJournal.UnknownCluesHeader.Text, detectiveJournal.UnknownCluesDescription.Text);
			m_Description.SetTooltip(template, config);
			m_Icon.SetTooltip(template, config);
		}
		else
		{
			m_Icon.sprite = base.ViewModel.Case.Icon;
			m_Description.text = base.ViewModel.Case.Description.Text;
			TooltipTemplateSimple template2 = new TooltipTemplateSimple(base.ViewModel.Case.Name.Text, base.ViewModel.Case.Description.Text);
			m_Description.SetTooltip(template2, config);
			m_Icon.SetTooltip(template2, config);
		}
	}
}
