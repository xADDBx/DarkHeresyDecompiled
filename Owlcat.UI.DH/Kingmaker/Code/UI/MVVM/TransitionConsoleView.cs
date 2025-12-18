using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionConsoleView : TransitionBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
		BuildNavigation();
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Transition"
		});
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void BuildNavigation()
	{
		m_NavigationBehaviour.SetEntitiesVertical(GetNavigationEntities());
	}

	private List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		if (m_CurrentPart.WidgetList.Entries != null)
		{
			foreach (MonoBehaviour entry in m_CurrentPart.WidgetList.Entries)
			{
				if (entry is TransitionLegendButtonView item)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private void OnConfirmClick()
	{
		(m_NavigationBehaviour.CurrentEntity as IConfirmClickHandler)?.OnConfirmClick();
	}
}
