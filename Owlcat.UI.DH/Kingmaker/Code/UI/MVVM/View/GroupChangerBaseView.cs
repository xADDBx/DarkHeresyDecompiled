using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class GroupChangerBaseView : View<GroupChangerVM>, IGameModeHandler, ISubscriber, IInitializable
{
	[Header("Remote Content")]
	[SerializeField]
	private GroupChangerCharacterBaseView m_RemoteCharacterView;

	[SerializeField]
	private RectTransform m_RemoteContainer;

	[Header("Party Content")]
	[SerializeField]
	private GroupChangerCharacterBaseView m_PartyCharacterView;

	[SerializeField]
	private RectTransform m_PartyContainer;

	[Header("Window")]
	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	private readonly List<GroupChangerCharacterBaseView> m_PartyCharacterViews = new List<GroupChangerCharacterBaseView>();

	protected readonly List<GroupChangerCharacterBaseView> m_RemoteCharacterViews = new List<GroupChangerCharacterBaseView>();

	public void Initialize()
	{
		m_WindowAnimator.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		m_WindowAnimator.AppearAnimation();
		ModalWindowsSounds.Instance.GroupChanger.Open.Play();
		foreach (GroupChangerCharacterVM item in base.ViewModel.PartyCharacter.Concat(base.ViewModel.RemoteCharacter))
		{
			CreateCharacterView(item, m_PartyCharacterView, AddToParty);
			CreateCharacterView(item, m_RemoteCharacterView, AddToReserve);
		}
		base.gameObject.SetActive(value: true);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.GroupChanger);
		});
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_PartyCharacterViews.ForEach(WidgetFactory.DisposeWidget);
		m_PartyCharacterViews.Clear();
		m_RemoteCharacterViews.ForEach(WidgetFactory.DisposeWidget);
		m_RemoteCharacterViews.Clear();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.GroupChanger);
		});
		base.gameObject.SetActive(value: false);
	}

	protected void OnAccept()
	{
		if (UtilityNet.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.AcceptChangeGroup(base.ViewModel.PartyCharacterRef.ToList(), base.ViewModel.RemoteCharacterRef.ToList(), base.ViewModel.RequiredCharactersRef.ToList(), base.ViewModel is GroupChangerCommonVM);
		}
	}

	protected void OnCancel()
	{
		if (UtilityNet.IsControlMainCharacterWithWarning() && base.ViewModel.CloseCondition())
		{
			m_WindowAnimator.DisappearAnimation(delegate
			{
				Game.Instance.GameCommandQueue.AddCommand(new CloseChangeGroupGameCommand());
			});
			ModalWindowsSounds.Instance.GroupChanger.Close.Play();
		}
	}

	protected void OnSelectedClick()
	{
		base.ViewModel.OnSelectedClick();
	}

	private void CreateCharacterView(GroupChangerCharacterVM vm, GroupChangerCharacterBaseView prefab, Action<GroupChangerCharacterBaseView> addMethod)
	{
		GroupChangerCharacterBaseView widget = WidgetFactory.GetWidget(prefab);
		widget.Bind(vm);
		addMethod(widget);
	}

	private void AddToParty(GroupChangerCharacterBaseView pcView)
	{
		m_PartyCharacterViews.Add(pcView);
		pcView.transform.SetParent(m_PartyContainer, worldPositionStays: false);
	}

	private void AddToReserve(GroupChangerCharacterBaseView pcView)
	{
		m_RemoteCharacterViews.Add(pcView);
		pcView.transform.SetParent(m_RemoteContainer, worldPositionStays: false);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!(gameMode != GameModeType.Dialog))
		{
			OnCancel();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
