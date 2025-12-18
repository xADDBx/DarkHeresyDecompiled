using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class GroupChangerConsoleView : GroupChangerBaseView, IConsoleNavigationOwner, IConsoleEntity
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private const int ColumnsCount = 6;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(this, null, Vector2Int.one).AddTo(this);
		GamePad.Instance.PushLayer(GetInputLayer()).AddTo(this);
		CreateNavigation();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	private void CreateNavigation()
	{
		if (RemoteCharacterViews.Count <= 6)
		{
			m_NavigationBehaviour.AddRow(RemoteCharacterViews);
		}
		else
		{
			m_NavigationBehaviour.AddRow(RemoteCharacterViews.GetRange(0, 6));
			m_NavigationBehaviour.AddRow(RemoteCharacterViews.GetRange(6, RemoteCharacterViews.Count - 6));
		}
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private InputLayer GetInputLayer()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "GroupChanger"
		}, null, leftStick: true, rightStick: true);
		if (UtilityNet.IsControlMainCharacter())
		{
			m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				OnCancel();
			}, 9, base.ViewModel.CloseActionsIsSame.Not().And(base.ViewModel.CloseEnabled).ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.CommonTexts.Cancel).AddTo(this);
			m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				OnAccept();
			}, 10, InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.CommonTexts.Accept).AddTo(this);
			m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				OnSelectedClick();
			}, 8), UIStrings.Instance.CommonTexts.Select).AddTo(this);
		}
		return m_InputLayer;
	}

	public void EntityFocused(IConsoleEntity entity)
	{
	}
}
