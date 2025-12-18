using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventSkillsBlockVM : ViewModel
{
	public readonly List<CharInfoStatVM> Skills;

	public readonly StatType StatType;

	public string SkillName => LocalizedTexts.Instance.Stats.GetText(StatType);

	public BookEventSkillsBlockVM(IEnumerable<BaseUnitEntity> units, StatType statType)
	{
		StatType = statType;
		Skills = new List<CharInfoStatVM>();
		foreach (BaseUnitEntity unit in units)
		{
			CharInfoStatVM item = new CharInfoStatVM(unit.Stats.GetStat(statType), showPermanentValue: false).AddTo(this);
			Skills.Add(item);
		}
	}
}
