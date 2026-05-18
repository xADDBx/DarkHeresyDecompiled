using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionEntryVM_OBSOLETE : ViewModel
{
	private readonly ReactiveProperty<string> m_Name = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_Attention = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_IsInteractable = new ReactiveProperty<bool>(value: true);

	public readonly BlueprintMultiEntranceEntry Entry;

	public readonly Action CloseAction;

	public readonly Action ClickAction;

	public readonly bool IsCurrentlyLocation;

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public ReadOnlyReactiveProperty<bool> Attention => m_Attention;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<bool> IsInteractable => m_IsInteractable;

	public TransitionEntryVM_OBSOLETE(BlueprintMultiEntranceEntry entry, Action closeAction)
	{
		CloseAction = closeAction;
		ClickAction = Enter;
		Entry = entry;
		m_Name.Value = GetPointName();
		m_Attention.Value = entry.GetLinkedObjectives().Count > 0;
		m_IsVisible.Value = entry.IsVisible;
		m_IsInteractable.Value = entry.IsInteractable;
		IsCurrentlyLocation = entry.CheckCurrentlyEntryLocation();
	}

	private string GetPointName()
	{
		ChangeTransitionPointName component = Entry.GetComponent<ChangeTransitionPointName>();
		if (component == null || !component.Conditions.HasConditions)
		{
			return Entry.Name;
		}
		if (component.Conditions.Check() && !string.IsNullOrWhiteSpace(component.AnotherName))
		{
			return component.AnotherName;
		}
		return Entry.Name;
	}

	public void Enter()
	{
		if (UtilityNet.IsControlMainCharacterWithWarning())
		{
			if (Entry != null)
			{
				Game.Instance.GameCommandQueue.AreaTransition(Entry);
			}
			CloseAction?.Invoke();
		}
	}

	public TooltipBaseTemplate GetTooltipTemplate()
	{
		return new TooltipTemplateTransitionEntry(Entry);
	}
}
