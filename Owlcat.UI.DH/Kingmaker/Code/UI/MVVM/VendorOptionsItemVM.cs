using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorOptionsItemVM : ViewModel
{
	private readonly SaleOptions m_Key;

	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<bool> m_State = new ReactiveProperty<bool>(value: false);

	private Dictionary<SaleOptions, bool> SettingsDictionaryMap => Game.Instance.Player.UISettings.OptionsDictionaryMap;

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public ReadOnlyReactiveProperty<bool> State => m_State;

	public VendorOptionsItemVM(SaleOptions options)
	{
		m_Key = options;
		ReactiveProperty<string> title = m_Title;
		LocalizedString localizedString = UIUtilityEncyclopedy.GetGlossaryEntry(m_Key.ToString())?.Title;
		title.Value = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		m_State.Value = SettingsDictionaryMap[m_Key];
	}

	public void SwitchOption()
	{
		SettingsDictionaryMap[m_Key] = !SettingsDictionaryMap[m_Key];
		m_State.Value = SettingsDictionaryMap[m_Key];
	}
}
