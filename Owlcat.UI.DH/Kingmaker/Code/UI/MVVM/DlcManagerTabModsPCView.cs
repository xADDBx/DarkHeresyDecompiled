using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabModsPCView : DlcManagerTabModsBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private DlcManagerTabModsModSelectorPCView m_ModSelectorPCView;

	protected override void OnBind()
	{
		base.OnBind();
		m_ModSelectorPCView.Bind(base.ViewModel.SelectionGroup);
	}

	protected override void SetBottomButtonsImpl()
	{
		ObservableSubscribeExtensions.Subscribe(m_NexusModsButton.OnLeftClickAsObservable(), delegate
		{
			OpenNexusMods();
		}).AddTo(this);
		if (base.ViewModel.IsSteam.CurrentValue)
		{
			ObservableSubscribeExtensions.Subscribe(m_SteamWorkshopButton.OnLeftClickAsObservable(), delegate
			{
				OpenSteamWorkshop();
			}).AddTo(this);
		}
	}
}
