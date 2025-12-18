using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetRolesConsoleView : NetRolesBaseView, INetRolesConsoleHandler, ISubscriber
{
	[Header("Console")]
	[SerializeField]
	private List<NetRolesPlayerConsoleView> m_Players;

	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_PlayerListNavigationBehaviour;

	private InputLayer m_GamersTagsInputLayer;

	private GridConsoleNavigationBehaviour m_GamersTagsNavigationBehavior;

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
		CreateInput();
	}

	protected override void OnUnbind()
	{
		if (m_GamersTagsNavigationBehavior != null)
		{
			m_GamersTagsNavigationBehavior.UnFocusCurrentEntity();
			m_GamersTagsNavigationBehavior.Clear();
		}
		m_PlayerListNavigationBehaviour.UnFocusCurrentEntity();
		m_PlayerListNavigationBehaviour.Clear();
		base.OnUnbind();
	}

	private void CreateInput()
	{
		m_PlayerListNavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_InputLayer = m_PlayerListNavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "NetRoles"
		});
		CreateInputImpl(m_InputLayer);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		SetPlayerListNavigation();
		m_PlayerListNavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		m_CommonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 8), base.ViewModel.IsRoomOwner ? UIStrings.Instance.SettingsUI.Apply : UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9).AddTo(this);
		if (m_Players == null || !m_Players.Any())
		{
			return;
		}
		m_Players.ForEach(delegate(NetRolesPlayerConsoleView p)
		{
			p.Characters.ForEach(delegate(NetRolesPlayerCharacterConsoleView c)
			{
				c.AddPlayerInput(inputLayer);
			});
		});
	}

	private void SetPlayerListNavigation()
	{
		m_PlayerListNavigationBehaviour.UnFocusCurrentEntity();
		m_PlayerListNavigationBehaviour.Clear();
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
		if (charactersEntities.Any())
		{
			m_PlayerListNavigationBehaviour.SetEntitiesHorizontal(charactersEntities);
		}
	}

	public void HandleUpdateCharactersNavigation(UnitReference focusCharacter)
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			SetPlayerListNavigation();
			m_PlayerListNavigationBehaviour.FocusOnEntityManual(m_PlayerListNavigationBehaviour.Entities.FirstOrDefault((IConsoleEntity e) => e is NetRolesPlayerCharacterConsoleView netRolesPlayerCharacterConsoleView && netRolesPlayerCharacterConsoleView.Character == focusCharacter));
		}, 1);
	}

	private void AddGamersTagsInput(ConsoleHintsWidget hintsWidget)
	{
		m_GamersTagsNavigationBehavior = new GridConsoleNavigationBehaviour().AddTo(this);
		m_GamersTagsInputLayer = m_GamersTagsNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "GamersTags"
		});
		m_Players.ForEach(delegate(NetRolesPlayerConsoleView p)
		{
			p.AddGamerTagInput(m_GamersTagsInputLayer, hintsWidget, CloseGamersTagsMode);
		});
	}

	private void ShowGamersTagsMode()
	{
		m_GamersTagsMode.Value = true;
		SetGamersTagsNavigation();
		NetRolesPlayerConsoleView focusedPlayer = m_Players.FirstOrDefault((NetRolesPlayerConsoleView p) => p.Characters.FirstOrDefault((NetRolesPlayerCharacterConsoleView c) => c.IsFocused.CurrentValue));
		GamerTagAndNameBaseView gamerTagAndNameBaseView = m_GamersTagsNavigationBehavior.Entities.OfType<GamerTagAndNameBaseView>().FirstOrDefault((GamerTagAndNameBaseView e) => e.GetUserId() == focusedPlayer.Or(null)?.GetUserId());
		GamePad.Instance.PushLayer(m_GamersTagsInputLayer);
		m_PlayerListNavigationBehaviour.UnFocusCurrentEntity();
		if (gamerTagAndNameBaseView != null)
		{
			m_GamersTagsNavigationBehavior.FocusOnEntityManual(gamerTagAndNameBaseView);
		}
		else
		{
			m_GamersTagsNavigationBehavior.FocusOnFirstValidEntity();
		}
	}

	private void CloseGamersTagsMode()
	{
		m_GamersTagsNavigationBehavior?.UnFocusCurrentEntity();
		m_PlayerListNavigationBehaviour?.FocusOnCurrentEntity();
		m_GamersTagsMode.Value = false;
		GamePad.Instance.PopLayer(m_GamersTagsInputLayer);
	}

	private void SetGamersTagsNavigation()
	{
		m_GamersTagsNavigationBehavior.Clear();
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		list.AddRange(m_Players.Select((NetRolesPlayerConsoleView player) => player.GamerTagAndName));
		if (list.Any())
		{
			m_GamersTagsNavigationBehavior.SetEntitiesVertical(list);
		}
	}
}
