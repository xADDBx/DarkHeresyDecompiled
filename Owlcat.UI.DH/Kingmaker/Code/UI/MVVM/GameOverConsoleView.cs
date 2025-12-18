using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class GameOverConsoleView : GameOverView
{
	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_NavigationBehaviour.AddTo(this);
		m_NavigationBehaviour.SetEntitiesVertical<OwlcatMultiButton>(m_QuickLoadButton, m_LoadButton, m_MainMenuButton, m_IronManDeleteSaveButton, m_IronManContinueGameButton);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "GameOverConsoleView"
		});
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}
}
