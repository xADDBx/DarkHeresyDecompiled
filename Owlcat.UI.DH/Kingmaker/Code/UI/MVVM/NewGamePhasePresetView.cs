using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhasePresetView : View<NewGamePhasePresetVM>
{
	[SerializeField]
	private NewGamePhasePresetSelectorView m_NewGamePhasePresetSelectorView;

	protected override void OnBind()
	{
		base.OnBind();
		base.gameObject.SetActive(value: true);
		m_NewGamePhasePresetSelectorView.Bind(base.ViewModel.SelectionGroup);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
