using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCustomPortraitCreatorConsoleView : CharGenCustomPortraitCreatorView
{
	[SerializeField]
	private ConsoleHint m_DeclineHint;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void OnBind()
	{
		CreateNavigation();
		base.OnBind();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CharGenCustomPortraitCreatorConsoleView"
		});
		m_NavigationBehaviour.AddEntityVertical(m_OpenFolderButton);
		m_NavigationBehaviour.AddEntityVertical(m_RefreshPortraitButton);
		ObservableSubscribeExtensions.Subscribe(m_OpenFolderButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnOpenFolderClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_RefreshPortraitButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnRefreshPortraitClick();
		}).AddTo(this);
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9);
		m_DeclineHint.Bind(inputBindStruct).AddTo(this);
		inputBindStruct.AddTo(this);
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		m_NavigationBehaviour.FocusOnEntityManual(m_OpenFolderButton);
		GamePad.Instance.PushLayer(m_InputLayer);
	}

	protected override void Show()
	{
		base.Show();
		m_NavigationBehaviour.FocusOnEntityManual(m_OpenFolderButton);
	}

	protected override void Hide()
	{
		base.Hide();
		m_NavigationBehaviour.Clear();
		GamePad.Instance.PopLayer(m_InputLayer);
	}
}
