using Kingmaker.Code.View.Bridge.Enums;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotCollectionVirtualConsoleView : SaveSlotCollectionVirtualBaseView
{
	private readonly ReactiveProperty<bool> m_HasSlot = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowDeleteButton = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowSaveButton = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowLoadButton = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowRenameButton = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.SelectedSaveSlot.CombineLatest(base.ViewModel.Mode, (SaveSlotVM vm, SaveLoadMode mode) => new { vm, mode }).Subscribe(value =>
		{
			m_HasSlot.Value = value.vm != null;
			m_ShowDeleteButton.Value = value.vm?.IsActuallySaved ?? false;
			ReactiveProperty<bool> showSaveButton = m_ShowSaveButton;
			SaveSlotVM vm2 = value.vm;
			showSaveButton.Value = vm2 != null && vm2.ShowSaveLoadButton && value.mode == SaveLoadMode.Save;
			m_ShowLoadButton.Value = value.vm != null && value.mode == SaveLoadMode.Load && !value.vm.ShowDlcRequiredLabel.CurrentValue;
			m_ShowRenameButton.Value = m_HasSlot.Value && !m_ShowDeleteButton.Value;
		}).AddTo(this);
	}

	public void AddInput()
	{
	}
}
