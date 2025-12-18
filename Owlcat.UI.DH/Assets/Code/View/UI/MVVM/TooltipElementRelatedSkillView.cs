using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class TooltipElementRelatedSkillView : View<TooltipElementRelatedSkillVM>
{
	[SerializeField]
	private TextMeshProUGUI m_SkillName;

	[SerializeField]
	private TextMeshProUGUI m_AttributeAcronym;

	protected override void OnBind()
	{
		base.OnBind();
		m_SkillName.text = base.ViewModel.SkillName;
		m_AttributeAcronym.text = base.ViewModel.AttributeAcronym;
	}
}
