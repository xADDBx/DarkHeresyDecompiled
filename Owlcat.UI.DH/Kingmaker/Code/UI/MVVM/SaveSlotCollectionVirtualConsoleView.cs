using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;
using Rewired;

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

	public void AddInput(InputLayer inputLayer, ConsoleHintsWidget commonHintsWidget, ReadOnlyReactiveProperty<bool> saveListUpdating, ReadOnlyReactiveProperty<bool> isCurrentIronManSave)
	{
		UISaveLoadTexts saveLoadTexts = LocalizedTexts.Instance.UserInterfacesText.SaveLoadTexts;
		commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 8, m_ShowLoadButton.And(saveListUpdating.Not()).And(isCurrentIronManSave.Not()).ToReadOnlyReactiveProperty(initialValue: false)), saveLoadTexts.LoadLabel).AddTo(this);
		commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 8, m_ShowSaveButton.And(saveListUpdating.Not()).ToReadOnlyReactiveProperty(initialValue: false)), saveLoadTexts.SaveLabel).AddTo(this);
		commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 10, m_ShowDeleteButton.And(saveListUpdating.Not()).ToReadOnlyReactiveProperty(initialValue: false)), saveLoadTexts.DeleteLabel).AddTo(this);
		commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 10, m_ShowRenameButton.And(saveListUpdating.Not()).ToReadOnlyReactiveProperty(initialValue: false)), saveLoadTexts.RenameSave).AddTo(this);
		commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 11, m_HasSlot.And(saveListUpdating.Not()).ToReadOnlyReactiveProperty(initialValue: false)), saveLoadTexts.ShowScreenshot).AddTo(this);
		string label = string.Concat(UIStrings.Instance.CommonTexts.Expand, "/", UIStrings.Instance.CommonTexts.Collapse);
		commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 8, m_HasSlot.Not().And(saveListUpdating.Not()).ToReadOnlyReactiveProperty(initialValue: false)), label).AddTo(this);
		commonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
		}, 10, m_HasSlot.Not().And(saveListUpdating.Not()).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed), saveLoadTexts.DeleteCharacter).AddTo(this);
	}
}
