using JetBrains.Annotations;
using Kingmaker.Enums;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationGroupBaseView : View<JournalNavigationGroupVM>
{
	[Space]
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Label;

	[Header("Collapse")]
	[SerializeField]
	[UsedImplicitly]
	protected OwlcatMultiButton m_MultiButton;

	[Header("Elements")]
	[SerializeField]
	[UsedImplicitly]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	public WidgetList WidgetList => m_WidgetList;

	protected bool ShowCompletedQuests => Game.Instance.Player.UISettings.JournalShowCompletedQuest;

	protected override void OnBind()
	{
		m_Label.text = base.ViewModel.Title;
		base.ViewModel.IsSelected.Subscribe(delegate(bool value)
		{
			m_MultiButton.Interactable = !value;
			m_MultiButton.SetActiveLayer(value ? "On" : "Off");
		}).AddTo(this);
		m_StateSelectable.SetActiveLayer(GetStateLayer());
	}

	private string GetStateLayer()
	{
		if (base.ViewModel.QuestGroup.Id != QuestGroupId.DetectiveCases)
		{
			return "Default";
		}
		return "Detective";
	}
}
