using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetRolesConsoleView : NetRolesBaseView, INetRolesConsoleHandler, ISubscriber
{
	[Header("Console")]
	[SerializeField]
	private List<NetRolesPlayerConsoleView> m_Players;

	private readonly ReactiveProperty<bool> m_GamersTagsMode = new ReactiveProperty<bool>();

	public override void Initialize()
	{
		base.Initialize();
		m_Players.ForEach(delegate(NetRolesPlayerConsoleView p)
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
		base.OnBind();
	}

	private void CreateInput()
	{
	}

	private void CreateInputImpl()
	{
	}

	private void SetPlayerListNavigation()
	{
		List<IConsoleEntity> charactersEntities = new List<IConsoleEntity>();
		if (m_Players?.FirstOrDefault() == null)
		{
			return;
		}
		int? num = m_Players?.FirstOrDefault()?.Characters?.Count;
		int i;
		for (i = 0; i < num; i++)
		{
			m_Players?.Where((NetRolesPlayerConsoleView p) => p.ViewModel != null).ToList().ForEach(delegate(NetRolesPlayerConsoleView p)
			{
				if (p.Characters[i].IsValid())
				{
					charactersEntities.Add(p.Characters[i]);
				}
			});
		}
	}

	public void HandleUpdateCharactersNavigation(UnitReference focusCharacter)
	{
	}
}
