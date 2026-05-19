using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class LevelUpSkillLinkedAttributeView : View<LevelUpSkillLinkedAttributeVM>
{
	private const float ITEM_HEIGHT = 41f;

	private const float SPACE = 3.4f;

	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	private GameObject m_Background;

	[SerializeField]
	private LayoutElement m_LayoutGroup;

	protected override void OnBind()
	{
		base.OnBind();
		m_Acronym.text = base.ViewModel.Acronym;
		m_Background.SetActive(base.ViewModel.HasBackground);
		m_LayoutGroup.preferredHeight = (float)base.ViewModel.ChildStatsCount * 41f + (float)(base.ViewModel.ChildStatsCount - 1) * 3.4f;
	}
}
