using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipBuffBlockVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityState;

	private readonly ImportantBuffsVM m_ImportantBuffs;

	public UnitBuffBlockVM UnitBuffBlockVM { get; }

	public ReadOnlyReactiveProperty<bool> IsVisible { get; }

	public OvertipBuffBlockVM(MechanicEntity target)
	{
		m_EntityState = UnitUIStateHolder.Instance.GetOrCreateUnitState(target);
		UnitBuffBlockVM = new UnitBuffBlockVM(target).AddTo(this);
		m_ImportantBuffs = new ImportantBuffsVM(target).AddTo(this);
		IsVisible = m_EntityState.IsInCombat.CombineLatest(m_EntityState.IsDeadOrUnconsciousIsDead, IsBuffsBlockVisible).ToReadOnlyReactiveProperty(initialValue: false).AddTo(this);
	}

	private bool IsBuffsBlockVisible(bool isInCombat, bool isDead)
	{
		if (!isInCombat)
		{
			return false;
		}
		return !isDead;
	}
}
