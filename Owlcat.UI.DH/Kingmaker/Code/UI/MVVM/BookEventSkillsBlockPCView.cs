using System.Linq;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventSkillsBlockPCView : View<BookEventSkillsBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_SkillName;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private BookEventSkillPCView m_BookEventSkillViewPrefab;

	protected override void OnBind()
	{
		m_SkillName.text = base.ViewModel.SkillName;
		DrawEntities();
		HighlightMax();
	}

	private void HighlightMax()
	{
		int maxSkillValue = base.ViewModel.Skills.Max((CharInfoStatVM skill) => skill.StatValue.CurrentValue);
		CharInfoStatVM item = base.ViewModel.Skills.FirstOrDefault((CharInfoStatVM skill) => skill.StatValue.CurrentValue == maxSkillValue);
		int index = base.ViewModel.Skills.IndexOf(item);
		((BookEventSkillPCView)m_WidgetList.Entries[index]).Highlight();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Skills.ToArray(), m_BookEventSkillViewPrefab);
	}

	public void SelectSkill(int? index)
	{
		for (int i = 0; i < m_WidgetList.Entries.Count; i++)
		{
			((BookEventSkillPCView)m_WidgetList.Entries[i]).SetSelected(i == index);
		}
	}
}
