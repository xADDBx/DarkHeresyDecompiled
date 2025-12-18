using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ItemsFilterSearchBaseView : View<ItemsFilterSearchVM>
{
	[Header("InputField Part")]
	[SerializeField]
	protected TMP_InputField m_InputField;

	[SerializeField]
	protected TextMeshProUGUI m_Placeholder;

	protected List<string> DropdownValues;

	public void Initialize()
	{
		string text = LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Scroll);
		string text2 = LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Unlearned);
		DropdownValues = new List<string>
		{
			text,
			LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Wand),
			LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Utility),
			LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Potion),
			LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilterType.Recipe),
			text2
		};
	}

	protected override void OnBind()
	{
		m_Placeholder.text = UIStrings.Instance.CharGen.EnterSearchTextHere;
	}

	public abstract void SetActive(bool value);

	protected void OnSearchStringEdit(string value)
	{
		base.ViewModel.SetSearchString(value);
	}
}
