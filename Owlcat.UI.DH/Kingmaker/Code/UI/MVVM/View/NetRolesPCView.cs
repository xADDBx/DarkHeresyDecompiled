using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetRolesPCView : NetRolesBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private List<NetRolesPlayerPCView> m_Players;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_ApplyButton;

	[SerializeField]
	private TextMeshProUGUI m_ApplyLabel;

	public override void Initialize()
	{
		base.Initialize();
		m_Players.ForEach(delegate(NetRolesPlayerPCView p)
		{
			p.Initialize();
		});
	}

	protected override void OnBind()
	{
		for (int i = 0; i < m_Players.Count; i++)
		{
			m_Players[i].Bind((base.ViewModel.PlayerVms.Count > i) ? base.ViewModel.PlayerVms[i] : null);
		}
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		m_ApplyLabel.text = (base.ViewModel.IsRoomOwner ? UIStrings.Instance.SettingsUI.Apply : UIStrings.Instance.CommonTexts.CloseWindow);
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnClose).AddTo(this);
		m_ApplyButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnClose).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose).AddTo(this);
		base.OnBind();
	}
}
