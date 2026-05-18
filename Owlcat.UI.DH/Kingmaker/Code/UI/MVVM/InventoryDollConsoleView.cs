using Kingmaker.Code.UI.MVVM.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryDollConsoleView : InventoryDollView<InventoryEquipSlotConsoleView>
{
	[Header("Customization Values")]
	[SerializeField]
	private float m_RotateFactor = 4f;

	[SerializeField]
	private float m_ZoomFactor = 0.2f;

	[SerializeField]
	private float m_ZoomThresholdValue = 0.17f;

	[Header("Console")]
	[SerializeField]
	protected InventorySelectorWindowConsoleView m_SelectorWindowView;

	[Header("Character Visual Settings")]
	[SerializeField]
	private CharacterVisualSettingsConsoleView m_VisualSettingsConsoleView;

	private IConsoleEntity m_PrevFocused;

	private ReactiveProperty<bool> m_BlockInteractions = new ReactiveProperty<bool>();

	private WeaponSetSelectorConsoleView WeaponSetConsoleView => m_WeaponSetSelector as WeaponSetSelectorConsoleView;

	public override void Initialize()
	{
		base.Initialize();
		m_SelectorWindowView.Or(null)?.Initialize();
		m_VisualSettingsConsoleView.Or(null)?.Initialize();
		m_VisualSettingsConsoleView.Or(null)?.SetDollRoomController(m_CharacterController, m_RotateFactor, m_ZoomFactor, m_ZoomThresholdValue);
	}

	protected override void OnBind()
	{
		base.OnBind();
		if ((bool)m_SelectorWindowView)
		{
			base.ViewModel.InventorySelectorWindowVM.Subscribe(m_SelectorWindowView.Bind).AddTo(this);
		}
		base.ViewModel.VisualSettingsVM.Subscribe(m_VisualSettingsConsoleView.Bind).AddTo(this);
	}

	private void CreateChooseSlotNavigation()
	{
	}

	public void AddInput()
	{
	}

	private void RotateDoll(float x)
	{
		if (!m_BlockInteractions.Value && !(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_CharacterController.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(float x)
	{
		if (!m_BlockInteractions.Value && !(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_CharacterController.Zoom(x * m_ZoomFactor);
		}
	}
}
