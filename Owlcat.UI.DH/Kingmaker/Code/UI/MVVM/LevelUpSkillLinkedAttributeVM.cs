using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LevelUpSkillLinkedAttributeVM : ViewModel
{
	private StatType m_Stat;

	public readonly bool HasBackground;

	public readonly string Acronym;

	public readonly int ChildStatsCount;

	public LevelUpSkillLinkedAttributeVM(StatType stat, int childCount, bool hasBackground)
	{
		m_Stat = stat;
		HasBackground = hasBackground;
		Acronym = LocalizedTexts.Instance.Stats.GetShortText(m_Stat);
		ChildStatsCount = childCount;
	}
}
