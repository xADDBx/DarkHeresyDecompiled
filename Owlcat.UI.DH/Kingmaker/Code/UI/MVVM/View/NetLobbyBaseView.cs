using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyBaseView : View<NetLobbyVM>, IInitializable
{
	[Header("Base Part")]
	[SerializeField]
	private TextMeshProUGUI m_Header;

	[Space]
	[SerializeField]
	protected NetLobbySaveSlotCollectionBaseView m_SlotCollectionView;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_SlotCollectionView.Initialize();
		m_Header.text = UIStrings.Instance.NetLobbyTexts.NetHeader;
	}

	protected override void OnBind()
	{
		SetVisibility(state: true);
	}

	protected override void OnUnbind()
	{
		SetVisibility(state: false);
	}

	private void SetVisibility(bool state)
	{
		if (state)
		{
			UISounds.Instance.Sounds.LocalMap.PlayOpen();
		}
		else
		{
			UISounds.Instance.Sounds.LocalMap.PlayClose();
		}
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state, FullScreenUIType.NewGame);
		});
		base.gameObject.SetActive(state);
		Game.Instance.RequestPauseUi(state);
	}
}
