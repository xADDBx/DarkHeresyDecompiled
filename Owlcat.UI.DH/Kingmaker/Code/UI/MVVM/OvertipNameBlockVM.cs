using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipNameBlockVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityState;

	private readonly ReactiveProperty<bool> m_IsVisible;

	public ReadOnlyReactiveProperty<string> Name { get; }

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<bool> IsPlayer => m_EntityState.IsPlayer;

	public ReadOnlyReactiveProperty<bool> IsEnemy => m_EntityState.IsEnemy;

	public OvertipNameBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		m_EntityState = mechanicEntityUIState;
		Name = mechanicEntityUIState.Name;
		m_IsVisible = new ReactiveProperty<bool>().AddTo(this);
		m_EntityState.IsVisibleForPlayer.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
		m_EntityState.ForceHotKeyPressed.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
		m_EntityState.IsMouseOverUnit.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
		m_EntityState.HideOvertip.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
		m_EntityState.IsTBM.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
	}

	private void UpdateVisibility()
	{
		m_IsVisible.Value = IsNameVisible();
	}

	private bool IsNameVisible()
	{
		if (!m_EntityState.MechanicEntity.IsVisibleForPlayer || m_EntityState.HideOvertip.CurrentValue || m_EntityState.IsTBM.CurrentValue)
		{
			return false;
		}
		bool currentValue = m_EntityState.IsMouseOverUnit.CurrentValue;
		if (m_EntityState.IsDestructible.CurrentValue && !m_EntityState.IsDestructibleNotCover.CurrentValue && !currentValue)
		{
			return false;
		}
		if (m_EntityState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.Name))
		{
			return false;
		}
		if (UtilityUnit.GetUnitInteractionFrom(m_EntityState.MechanicEntity.MechanicEntity) == null)
		{
			return false;
		}
		if (!currentValue)
		{
			return m_EntityState.ForceHotKeyPressed.CurrentValue;
		}
		return true;
	}
}
