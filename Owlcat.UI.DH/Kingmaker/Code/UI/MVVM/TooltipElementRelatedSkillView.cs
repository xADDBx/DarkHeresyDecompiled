using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipElementRelatedSkillView : View<TooltipElementRelatedSkillVM>
{
	[SerializeField]
	private TMP_Text m_SkillName;

	[SerializeField]
	private TMP_Text m_AttributeAcronym;

	protected override void OnBind()
	{
		base.OnBind();
		m_SkillName.text = base.ViewModel.SkillName;
		m_AttributeAcronym.text = base.ViewModel.AttributeAcronym;
	}
}
