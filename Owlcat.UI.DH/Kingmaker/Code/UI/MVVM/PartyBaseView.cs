using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class PartyBaseView<TPartyCharacterView> : View<PartyVM>, ISwitchPartyCharactersHandler, ISubscriber where TPartyCharacterView : PartyCharacterBaseView
{
	[Header("Switch characters")]
	[SerializeField]
	private OwlcatMultiButton m_NextButton;

	[SerializeField]
	private OwlcatMultiButton m_PrevButton;

	[Header("Content Size Control")]
	[SerializeField]
	private HorizontalLayoutGroup m_LayoutGroup;

	[SerializeField]
	private ContentSizeFitterExtended m_ContentSizeFitter;

	protected readonly List<TPartyCharacterView> m_Characters = new List<TPartyCharacterView>();

	protected virtual void Awake()
	{
		ResetCharacters();
	}

	protected void ResetCharacters()
	{
		m_Characters.Clear();
		TPartyCharacterView[] componentsInChildren = GetComponentsInChildren<TPartyCharacterView>(includeInactive: true);
		foreach (TPartyCharacterView item in componentsInChildren)
		{
			m_Characters.Add(item);
		}
	}

	protected override void OnBind()
	{
		base.ViewModel.NextEnable.Subscribe(delegate(bool value)
		{
			m_NextButton.gameObject.SetActive(value);
		}).AddTo(this);
		base.ViewModel.PrevEnable.Subscribe(delegate(bool value)
		{
			m_PrevButton.gameObject.SetActive(value);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_NextButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Next();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_PrevButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Prev();
		}).AddTo(this);
		for (int i = 0; i < m_Characters.Count; i++)
		{
			PartyCharacterVM partyCharacterVM = base.ViewModel.CharactersVM[i];
			m_Characters[i].Bind(partyCharacterVM);
			partyCharacterVM.IsEnable.Subscribe(delegate
			{
				UpdateLayout();
			}).AddTo(this);
		}
		UpdateLayout();
		EventBus.Subscribe(this).AddTo(this);
	}

	private void UpdateLayout()
	{
		m_LayoutGroup.enabled = true;
		m_ContentSizeFitter.enabled = true;
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		Vector2 size = ((RectTransform)base.transform).rect.size;
		m_LayoutGroup.enabled = false;
		m_ContentSizeFitter.enabled = false;
		((RectTransform)base.transform).sizeDelta = size;
		m_Characters.ForEach(delegate(TPartyCharacterView ch)
		{
			ch.UpdateBasePosition();
		});
	}

	public void HandleSwitchPartyCharacters(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		TPartyCharacterView val = m_Characters.FirstOrDefault((TPartyCharacterView c) => c.UnitEntityData == unit1);
		TPartyCharacterView val2 = m_Characters.FirstOrDefault((TPartyCharacterView c) => c.UnitEntityData == unit2);
		if (!(val == null) && !(val2 == null))
		{
			int index = m_Characters.IndexOf(val);
			int index2 = m_Characters.IndexOf(val2);
			m_Characters[index] = val2;
			m_Characters[index2] = val;
			m_Characters[index].Bind(base.ViewModel.CharactersVM[index]);
			m_Characters[index2].Bind(base.ViewModel.CharactersVM[index2]);
		}
	}
}
