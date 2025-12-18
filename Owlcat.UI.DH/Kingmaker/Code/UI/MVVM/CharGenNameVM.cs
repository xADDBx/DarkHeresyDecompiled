using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenNameVM : CharInfoComponentWithLevelUpVM
{
	private readonly Func<string> m_GetRandomName;

	private readonly Action<string> m_OnSetName;

	private readonly ReactiveProperty<CharGenChangeNameMessageBoxVM> m_MessageBoxVM = new ReactiveProperty<CharGenChangeNameMessageBoxVM>();

	private readonly ReactiveProperty<string> m_UnitName = new ReactiveProperty<string>();

	public ReadOnlyReactiveProperty<CharGenChangeNameMessageBoxVM> MessageBoxVM => m_MessageBoxVM;

	public ReadOnlyReactiveProperty<string> UnitName => m_UnitName;

	public CharGenNameVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager, Func<string> getRandomName, Action<string> onSetName)
		: base(unit, levelUpManager)
	{
		m_UnitName.Value = base.PreviewUnit.CurrentValue.CharacterName;
		m_GetRandomName = getRandomName;
		m_OnSetName = onSetName;
	}

	public void ShowChangeNameMessageBox(Action onComplete = null)
	{
		DisposeMessageBox();
		m_MessageBoxVM.Value = new CharGenChangeNameMessageBoxVM(UIStrings.Instance.CharGen.ChooseName, UIStrings.Instance.SettingsUI.DialogApply, delegate(string text)
		{
			text = text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				SetName(text);
			}
			onComplete?.Invoke();
		}, UnitName.CurrentValue, GetRandomName, DisposeMessageBox);
	}

	private void DisposeMessageBox()
	{
		MessageBoxVM.Dispose();
	}

	public string GetRandomName()
	{
		return m_GetRandomName?.Invoke();
	}

	public void SetName(string characterName)
	{
		m_UnitName.Value = characterName;
		m_OnSetName?.Invoke(characterName);
	}
}
