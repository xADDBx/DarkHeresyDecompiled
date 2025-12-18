using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RespecWindowConsoleView : RespecWindowCommonView
{
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	private GridConsoleNavigationBehaviour m_Behaviour;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		m_Behaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		CreateInput();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ConsoleHintsWidget.Dispose();
		GamePad.Instance.PopLayer(m_InputLayer);
	}

	private void CreateInput()
	{
		m_Behaviour.SetEntitiesGrid(m_RespecCharactersSelectorView.GetNavigationEntities(), 4);
		m_Behaviour.FocusOnFirstValidEntity();
		m_InputLayer = m_Behaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Respec"
		});
		m_Behaviour.Focus.Subscribe(Scroll).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			CloseWindow();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnConfirm();
		}, 8), UIStrings.Instance.CommonTexts.Accept).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	public void Scroll(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
