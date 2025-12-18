using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionLegendButtonView : View<TransitionEntryVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private Image m_Attention;

	public PantographConfig PantographConfig { get; private set; }

	protected override void OnBind()
	{
		base.gameObject.SetActive(base.ViewModel.IsVisible.CurrentValue);
		base.ViewModel.Attention.Subscribe(delegate(bool value)
		{
			m_Attention.gameObject.SetActive(value);
			if (value)
			{
				m_Button.SetTooltip(base.ViewModel.GetTooltipTemplate(), new TooltipConfig(InfoCallPCMethod.None));
				Sprite firstObjectiveTypeSprite = GetFirstObjectiveTypeSprite();
				m_Attention.gameObject.SetActive(firstObjectiveTypeSprite != null);
				if (!(firstObjectiveTypeSprite == null))
				{
					m_Attention.sprite = firstObjectiveTypeSprite;
				}
			}
		}).AddTo(this);
		m_Title.text = base.ViewModel.Name.CurrentValue;
		m_Button.OnHoverAsObservable().Subscribe(delegate(bool value)
		{
			if (value && base.ViewModel.IsInteractable.CurrentValue)
			{
				EventBus.RaiseEvent(delegate(IPantographHandler h)
				{
					h.Bind(PantographConfig);
				});
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Enter();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.Enter();
		}).AddTo(this);
		base.ViewModel.IsInteractable.Subscribe(delegate(bool value)
		{
			m_Button.Interactable = value;
		}).AddTo(this);
		SetupPantographConfig();
	}

	private Sprite GetFirstObjectiveTypeSprite(bool isPaper = true)
	{
		BlueprintQuestObjective blueprintQuestObjective = base.ViewModel.Entry.GetLinkedObjectives().FirstOrDefault();
		if (blueprintQuestObjective == null)
		{
			return null;
		}
		QuestType type = blueprintQuestObjective.Quest.Type;
		if (!isPaper)
		{
			return ConfigRoot.Instance.UIConfig.UIIcons.QuestTypesIcons.GetQuestMonitorTypeIcon(type);
		}
		return ConfigRoot.Instance.UIConfig.UIIcons.QuestTypesIcons.GetQuestPaperTypeIcon(type);
	}

	private void SetupPantographConfig()
	{
		List<Sprite> list = new List<Sprite>();
		if (base.ViewModel.Attention.CurrentValue)
		{
			list.Add(GetFirstObjectiveTypeSprite(isPaper: false));
		}
		PantographConfig = new PantographConfig(base.transform, m_Title.text, list);
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}
}
