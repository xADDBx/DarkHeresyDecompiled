using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenPregenSelectorItemVM : SelectionGroupEntityVM
{
	private readonly ReactiveProperty<string> m_CharacterName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Class = new ReactiveProperty<string>();

	private readonly ReactiveProperty<Gender> m_Gender = new ReactiveProperty<Gender>();

	private readonly ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<string> m_Race = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Role = new ReactiveProperty<string>();

	public ReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	public ReadOnlyReactiveProperty<string> CharacterName => m_CharacterName;

	public ReadOnlyReactiveProperty<string> Class => m_Class;

	public ReadOnlyReactiveProperty<string> Role => m_Role;

	public ReadOnlyReactiveProperty<string> Race => m_Race;

	public ReadOnlyReactiveProperty<Gender> Gender => m_Gender;

	public ChargenUnit ChargenUnit { get; private set; }

	public CharGenPregenSelectorItemVM(ChargenUnit chargenUnit, bool isCustomCharacter = false)
		: base(allowSwitchOff: false)
	{
		if (!isCustomCharacter)
		{
			SetupCharacterProperties(chargenUnit);
		}
		else
		{
			SetupCustomCharacterProperties();
		}
	}

	protected override void DisposeImplementation()
	{
		ChargenUnit = null;
	}

	private void SetupCharacterProperties(ChargenUnit chargenUnit)
	{
		ChargenUnit = chargenUnit;
		m_Portrait.Value = ChargenUnit.Blueprint.PortraitSafe.HalfLengthPortrait;
		PregenUnitComponent component = ChargenUnit.Blueprint.GetComponent<PregenUnitComponent>();
		if (component != null)
		{
			m_CharacterName.Value = component.PregenName;
			m_Class.Value = component.PregenClass;
		}
	}

	private void SetupCustomCharacterProperties()
	{
		m_CharacterName.Value = UIStrings.Instance.CharGen.CreateCustomCharacter;
	}

	protected override void DoSelectMe()
	{
	}
}
