using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ActionBarBasePartVM : ViewModel
{
	public EntityRef<BaseUnitEntity> Unit;

	protected readonly ReactiveCommand<Unit> m_UnitChanged = new ReactiveCommand<Unit>();

	public Observable<Unit> UnitChanged => m_UnitChanged;

	protected override void OnDispose()
	{
		ClearSlots();
	}

	public void SetUnit(EntityRef<BaseUnitEntity> unit)
	{
		Unit = unit;
		if (unit != null)
		{
			OnUnitChanged();
			m_UnitChanged.Execute();
		}
		else
		{
			ClearSlots();
		}
	}

	protected abstract void OnUnitChanged();

	protected abstract void ClearSlots();
}
