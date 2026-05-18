using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenNameVM : CharInfoComponentWithLevelUpVM
{
	private readonly ReactiveProperty<string> m_CurrentDisplayName = new ReactiveProperty<string>();

	private readonly Func<string> m_GetRandomName;

	private readonly Action<string> m_OnSetName;

	public ReadOnlyReactiveProperty<string> CurrentDisplayName => m_CurrentDisplayName;

	public CharGenNameVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager, Func<string> getRandomName, Action<string> onSetName)
		: base(unit, levelUpManager)
	{
		m_CurrentDisplayName.Value = string.Empty;
		m_GetRandomName = getRandomName;
		m_OnSetName = onSetName;
	}

	public void SetName(string characterName)
	{
		m_CurrentDisplayName.Value = characterName;
		m_OnSetName?.Invoke(characterName);
	}

	public void SetRandomName()
	{
		string name = m_GetRandomName?.Invoke();
		SetName(name);
	}
}
