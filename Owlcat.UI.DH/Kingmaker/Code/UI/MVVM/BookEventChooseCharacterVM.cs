using System.Collections.Generic;
using System.Linq;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventChooseCharacterVM : ViewModel
{
	public readonly List<BookEventCharacterVM> BookEventCharacters;

	public readonly List<BookEventSkillsBlockVM> BookEventSkillsBlocks;

	private readonly ReactiveProperty<int?> m_SelectedIndex = new ReactiveProperty<int?>();

	private readonly BlueprintAnswer m_Answer;

	private BaseUnitEntity m_Unit;

	public ReadOnlyReactiveProperty<int?> SelectedIndex => m_SelectedIndex;

	public BookEventChooseCharacterVM(BlueprintAnswer answer)
	{
		m_Answer = answer;
		List<BaseUnitEntity> list = Game.Instance.Player.Party.Where((BaseUnitEntity character) => !character.LifeState.IsFinallyDead).ToList();
		List<StatType> list2 = m_Answer.CharacterSelection.ComparisonStats.ToList();
		BookEventCharacters = new List<BookEventCharacterVM>();
		foreach (BaseUnitEntity item in list)
		{
			BookEventCharacters.Add(new BookEventCharacterVM(item, OnChoose));
		}
		BookEventSkillsBlocks = new List<BookEventSkillsBlockVM>();
		foreach (StatType item2 in list2)
		{
			BookEventSkillsBlocks.Add(new BookEventSkillsBlockVM(list, item2));
		}
	}

	public void ConfirmUnit()
	{
		Game.Instance.Controllers.DialogController.SelectAnswer(m_Answer, m_Unit);
	}

	private void OnChoose(BaseUnitEntity unit)
	{
		m_Unit = unit;
		m_SelectedIndex.Value = ((unit == null) ? null : new int?(BookEventCharacters.FindIndex((BookEventCharacterVM ch) => ch.Unit == unit)));
	}
}
