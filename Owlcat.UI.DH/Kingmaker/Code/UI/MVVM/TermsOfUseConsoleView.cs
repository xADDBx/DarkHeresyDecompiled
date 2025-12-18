using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class TermsOfUseConsoleView : TermsOfUseBaseView
{
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private ConsoleHint m_AcceptHint;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void OnBind()
	{
		base.OnBind();
		BuildNavigation();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	private void BuildNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		BuildNavigationImpl(m_NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "TermsOfUseLayer"
		});
		m_InputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
		m_AcceptHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.TermsOfUseAccept();
		}, 8)).AddTo(this);
		m_AcceptHint.SetLabel(UIStrings.Instance.TermsOfUseTexts.AcceptBtn);
		CreateInputImpl(m_InputLayer);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}
}
