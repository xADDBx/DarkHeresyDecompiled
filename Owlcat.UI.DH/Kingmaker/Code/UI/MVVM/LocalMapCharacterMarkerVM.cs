using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapCharacterMarkerVM : LocalMapMarkerVM
{
	private readonly BaseUnitEntity m_Unit;

	public LocalMapCharacterMarkerVM(BaseUnitEntity unit)
	{
		m_Unit = unit;
		base.MarkerType = LocalMapMarkType.PlayerCharacter;
		m_IsVisible.Value = true;
		m_Description.Value = unit.CharacterName;
		m_Position.Value = unit.Position;
		m_Portrait.Value = unit.Portrait.SmallPortrait;
		m_IsSelected.Value = Game.Instance.Controllers.SelectionCharacter.IsSelected(unit);
	}

	protected override void OnUpdateHandler()
	{
		BaseUnitEntity unit = m_Unit;
		if (unit != null && !unit.IsDisposed)
		{
			m_Position.Value = m_Unit.Position;
		}
	}

	public override Entity GetEntity()
	{
		return null;
	}
}
