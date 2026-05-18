using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenPortraitSelectorItemVM : SelectionGroupEntityVM
{
	public readonly bool IsCustom;

	private readonly BlueprintPortrait m_BlueprintPortrait;

	private readonly Action<CharGenPortraitSelectorItemVM> m_OnPortraitChange;

	private readonly Action m_OnPortraitCreate;

	public readonly PortraitData PortraitData;

	public bool CustomPortraitCreatorItem { get; }

	public CharGenPortraitSelectorItemVM(BlueprintPortrait blueprintPortrait, bool custom = false)
		: base(allowSwitchOff: false)
	{
		m_BlueprintPortrait = blueprintPortrait;
		IsCustom = custom;
		if (!custom)
		{
			PortraitData = m_BlueprintPortrait.Data;
		}
	}

	public CharGenPortraitSelectorItemVM(PortraitData portraitData, Action<CharGenPortraitSelectorItemVM> onPortraitChange)
		: this(ConfigRoot.Instance.CharGenRoot.CustomPortrait, custom: true)
	{
		PortraitData = portraitData;
		m_OnPortraitChange = onPortraitChange;
	}

	public CharGenPortraitSelectorItemVM(Action onPortraitCreate)
		: base(allowSwitchOff: false)
	{
		m_OnPortraitCreate = onPortraitCreate;
		CustomPortraitCreatorItem = true;
	}

	public BlueprintPortrait GetBlueprintPortrait()
	{
		m_BlueprintPortrait.Data = PortraitData;
		return m_BlueprintPortrait;
	}

	public void OnCustomPortraitCreate()
	{
		m_OnPortraitCreate?.Invoke();
	}

	public void OnCustomPortraitChange()
	{
		m_OnPortraitChange?.Invoke(this);
	}

	protected override void DoSelectMe()
	{
	}
}
