using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionVariantView : View<InteractionVariantVM>
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[Header("Images")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_ActionName;

	[SerializeField]
	private TextMeshProUGUI m_ChanceLabel;

	[SerializeField]
	private GameObject m_ChanceContainer;

	[SerializeField]
	private TextMeshProUGUI m_ResourceCount;

	[SerializeField]
	private Image m_UnitIcon;

	protected override void OnBind()
	{
		m_Icon.sprite = ConfigRoot.Instance.Interaction.GetInteractionIcon(base.ViewModel.InteractionActor.VariantActor.UIType);
		if (base.ViewModel.RequiredResourceCount.HasValue)
		{
			m_ResourceCount.text = ((base.ViewModel.ResourceCount > 0) ? $"{base.ViewModel.ResourceCount}" : string.Empty);
			m_UnitIcon.gameObject.SetActive(value: false);
		}
		else if (base.ViewModel.OnlyOnceCheck && !base.ViewModel.Disabled && !base.ViewModel.InteractionActor.VariantActor.AlreadyUsed)
		{
			m_ResourceCount.text = "1";
			m_UnitIcon.gameObject.SetActive(value: true);
		}
		else if (base.ViewModel.LimitedUnitsCheck)
		{
			m_ResourceCount.text = $"{base.ViewModel.UnitCount}";
			m_UnitIcon.gameObject.SetActive(value: true);
		}
		else
		{
			m_ResourceCount.text = string.Empty;
			m_UnitIcon.gameObject.SetActive(value: false);
		}
		base.ViewModel.InteractionName.Subscribe(delegate(string text)
		{
			m_ActionName.text = text;
		}).AddTo(this);
		base.ViewModel.InteractionChance.Subscribe(delegate(string text)
		{
			m_ChanceLabel.text = text + "%";
			m_ChanceContainer.SetActive(!string.IsNullOrEmpty(text));
		}).AddTo(this);
		m_Button.Interactable = !base.ViewModel.Disabled;
		base.gameObject.name = "InteractionVariantView " + base.ViewModel.InteractionName.CurrentValue + " " + base.ViewModel.ResourceName;
		m_Button.OnPointerEnterAsObservable().Subscribe(delegate
		{
			EventBus.RaiseEvent((IMapObjectEntity)base.ViewModel.InteractionActor.VariantActor.InteractionPart.View.Data, (Action<IDirectInteractionObjectUIHandler>)delegate(IDirectInteractionObjectUIHandler h)
			{
				h.HandleObjectInteract(isOn: true);
			}, isCheckRuntime: true);
		}).AddTo(this);
		m_Button.OnPointerExitAsObservable().Subscribe(delegate
		{
			EventBus.RaiseEvent((IMapObjectEntity)base.ViewModel.InteractionActor.VariantActor.InteractionPart.View.Data, (Action<IDirectInteractionObjectUIHandler>)delegate(IDirectInteractionObjectUIHandler h)
			{
				h.HandleObjectInteract(isOn: false);
			}, isCheckRuntime: true);
		}).AddTo(this);
		m_Button.SetActiveLayer(base.ViewModel.IsAdditionalCombatObj ? "AdditionalCombat" : "Default");
	}
}
