using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class FirstLaunchEntityLanguageBaseView<TFirstLaunchEntityLanguageItemView> : SettingsEntityWithValueView<FirstLaunchEntityLanguageVM> where TFirstLaunchEntityLanguageItemView : FirstLaunchEntityLanguageItemBaseView
{
	[SerializeField]
	private List<TFirstLaunchEntityLanguageItemView> m_ItemViews;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		for (int i = 0; i < base.ViewModel.Items.Count; i++)
		{
			m_ItemViews[i].Bind(base.ViewModel.Items[i]);
		}
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
	}

	public override bool HandleLeft()
	{
		return false;
	}

	public override bool HandleRight()
	{
		return false;
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}
}
