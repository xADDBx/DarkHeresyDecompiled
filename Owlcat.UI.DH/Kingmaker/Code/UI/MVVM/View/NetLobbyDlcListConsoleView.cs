using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyDlcListConsoleView : NetLobbyDlcListBaseView
{
	[SerializeField]
	private NetLobbyDlcListDlcEntityConsoleView m_DlcEntityConsoleViewPrefab;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	protected override void DrawDlcsImpl()
	{
		base.DrawDlcsImpl();
		NetLobbyDlcListDlcEntityVM[] array = base.ViewModel.Dlcs.ToArray();
		if (array.Any())
		{
			m_DlcsWidgetList.DrawEntries(array, m_DlcEntityConsoleViewPrefab);
		}
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "NetLobbyDlcListConsoleView"
		};
		CreateInputImpl(m_InputLayer, m_HintsWidget);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.CloseWindow();
		}, 9, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		inputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
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
