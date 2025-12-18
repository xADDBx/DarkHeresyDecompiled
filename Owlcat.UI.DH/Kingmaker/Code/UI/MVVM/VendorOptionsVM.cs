using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorOptionsVM : ViewModel
{
	public readonly List<VendorOptionsItemVM> ItemVms = new List<VendorOptionsItemVM>();

	private Dictionary<SaleOptions, bool> SettingsDictionaryMap => Game.Instance.Player.UISettings.OptionsDictionaryMap;

	public VendorOptionsVM()
	{
		AddOptionsDictionary(SaleOptions.MasterWork, defaultValue: false);
		AddOptionsDictionary(SaleOptions.NonMagical, defaultValue: false);
		AddOptionsDictionary(SaleOptions.GemsAnimalParts, defaultValue: true);
		foreach (KeyValuePair<SaleOptions, bool> item in SettingsDictionaryMap)
		{
			ItemVms.Add(new VendorOptionsItemVM(item.Key));
		}
	}

	protected override void OnDispose()
	{
		ItemVms.ForEach(delegate(VendorOptionsItemVM vm)
		{
			vm.Dispose();
		});
		ItemVms.Clear();
	}

	private void AddOptionsDictionary(SaleOptions key, bool defaultValue)
	{
		if (!SettingsDictionaryMap.ContainsKey(key))
		{
			SettingsDictionaryMap.Add(key, defaultValue);
		}
	}
}
