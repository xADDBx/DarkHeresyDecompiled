using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UnitLogic.Levelup.Selections.CharacterName;

public class SelectionStateCharacterName : SelectionState
{
	private bool m_Selected;

	public string CharacterName { get; private set; }

	public SelectionStateCharacterName([NotNull] LevelUpManager manager, [NotNull] BlueprintSelection blueprint, [NotNull] BlueprintPath path, int pathRank)
		: base(manager, blueprint, path, pathRank)
	{
	}

	protected override bool IsMadeInternal()
	{
		return m_Selected;
	}

	protected override bool IsValidInternal()
	{
		return !string.IsNullOrEmpty(CharacterName);
	}

	protected override bool CanSelectAnyInternal()
	{
		return true;
	}

	public void SelectName(string name)
	{
		CharacterName = name;
		m_Selected = true;
		NotifySelectionChanged();
	}

	protected override void ApplyInternal(BaseUnitEntity unit)
	{
		unit.Description.SetCustomName(CharacterName);
	}

	protected override void InvalidateInternal()
	{
	}
}
