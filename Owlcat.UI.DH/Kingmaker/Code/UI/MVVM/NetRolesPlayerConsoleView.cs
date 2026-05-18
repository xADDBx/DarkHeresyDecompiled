using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetRolesPlayerConsoleView : NetRolesPlayerBaseView
{
	[SerializeField]
	private List<NetRolesPlayerCharacterConsoleView> m_Characters;

	public List<NetRolesPlayerCharacterConsoleView> Characters => m_Characters;

	public override void Initialize()
	{
		base.Initialize();
		m_Characters.ForEach(delegate(NetRolesPlayerCharacterConsoleView c)
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

	public void AddGamerTagInput()
	{
	}
}
