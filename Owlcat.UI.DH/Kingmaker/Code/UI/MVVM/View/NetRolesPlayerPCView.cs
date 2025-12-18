using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetRolesPlayerPCView : NetRolesPlayerBaseView
{
	[SerializeField]
	private List<NetRolesPlayerCharacterPCView> m_Characters;

	public override void Initialize()
	{
		base.Initialize();
		m_Characters.ForEach(delegate(NetRolesPlayerCharacterPCView c)
		{
			c.Initialize();
		});
	}

	protected override void OnBind()
	{
		base.OnBind();
		for (int i = 0; i < m_Characters.Count; i++)
		{
			m_Characters[i].Bind((base.ViewModel.Players.Count > i) ? base.ViewModel.Players[i] : null);
		}
	}
}
