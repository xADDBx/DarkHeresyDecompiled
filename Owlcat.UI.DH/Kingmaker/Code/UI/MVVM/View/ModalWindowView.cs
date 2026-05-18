using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class ModalWindowView : View<ModalWindowVM>
{
	private const string LayerOnlyTitle = "OnlyTitle";

	private const string LayerDefault = "Default";

	[SerializeField]
	private TMP_Text m_Header;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private ModalWindowButton m_ButtonPrefab;

	[SerializeField]
	private RectTransform m_ButtonsParent;

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	[SerializeField]
	private HorizontalOrVerticalLayoutGroup m_LayoutGroup;

	private readonly List<ModalWindowButton> m_Buttons = new List<ModalWindowButton>();

	protected IReadOnlyList<ModalWindowButton> Buttons => m_Buttons;

	protected override void OnBind()
	{
		m_Header.text = base.ViewModel.Header;
		bool flag = base.ViewModel.Description != null;
		m_Description.gameObject.SetActive(flag);
		if (flag)
		{
			m_Description.text = base.ViewModel.Description;
		}
		IReadOnlyList<ModalWindowAction> actions = base.ViewModel.Actions;
		bool flag2 = actions != null && actions.Count > 0;
		bool flag3 = !flag && !flag2;
		m_LayoutGroup.childAlignment = ((!flag3) ? TextAnchor.UpperCenter : TextAnchor.MiddleCenter);
		m_Selectable.SetActiveLayer(flag3 ? "OnlyTitle" : "Default");
		if (!flag2)
		{
			return;
		}
		foreach (ModalWindowAction action in base.ViewModel.Actions)
		{
			ModalWindowButton widget = WidgetFactory.GetWidget(m_ButtonPrefab);
			widget.transform.SetParent(m_ButtonsParent, worldPositionStays: false);
			widget.Initialize(action);
			m_Buttons.Add(widget);
		}
		base.ViewModel.ButtonInteractableStateChanged.Subscribe(SetButtonInteractable).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Buttons.ForEach(delegate(ModalWindowButton button)
		{
			button.Dispose();
			WidgetFactory.DisposeWidget(button);
		});
		m_Buttons.Clear();
	}

	private void SetButtonInteractable((int index, bool interactable) data)
	{
		if (data.index >= 0 && data.index < m_Buttons.Count)
		{
			m_Buttons[data.index].MultiButton.Interactable = data.interactable;
		}
	}
}
