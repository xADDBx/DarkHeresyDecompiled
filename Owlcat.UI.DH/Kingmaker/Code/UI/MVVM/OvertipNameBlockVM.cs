using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipNameBlockVM : ViewModel
{
	private readonly MechanicEntityUIState m_entityState;

	private readonly ReactiveProperty<bool> m_IsVisible;

	public ReadOnlyReactiveProperty<string> Name { get; }

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<bool> IsPlayer => m_entityState.IsPlayer;

	public ReadOnlyReactiveProperty<bool> IsEnemy => m_entityState.IsEnemy;

	public OvertipNameBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		m_entityState = mechanicEntityUIState;
		Name = mechanicEntityUIState.Name;
		m_IsVisible = new ReactiveProperty<bool>().AddTo(this);
		m_entityState.IsVisibleForPlayer.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
		m_entityState.ForceHotKeyPressed.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
		m_entityState.IsMouseOverUnit.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
		m_entityState.HasHiddenCondition.Subscribe(delegate
		{
			UpdateVisibility();
		}).AddTo(this);
		m_entityState.IsTBM.Subscribe(delegate
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
		if (!m_entityState.MechanicEntity.IsVisibleForPlayer || m_entityState.HasHiddenCondition.CurrentValue || m_entityState.IsTBM.CurrentValue)
		{
			return false;
		}
		bool currentValue = m_entityState.IsMouseOverUnit.CurrentValue;
		if (m_entityState.IsDestructible.CurrentValue && !m_entityState.IsDestructibleNotCover.CurrentValue && !currentValue)
		{
			return false;
		}
		if (m_entityState.IsHiddenBySettings && !currentValue)
		{
			return false;
		}
		if (!currentValue)
		{
			return m_entityState.ForceHotKeyPressed.CurrentValue;
		}
		return true;
	}
}
