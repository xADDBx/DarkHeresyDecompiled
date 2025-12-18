using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoSummaryConsoleView : CharInfoSummaryBaseView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private SimpleConsoleNavigationEntity m_MovePointsConsoleEntity;

	private SimpleConsoleNavigationEntity m_ActionPointsConsoleEntity;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CharScreenSummaryView"
		});
		m_MovePointsConsoleEntity = new SimpleConsoleNavigationEntity(m_MovePointsButton);
		m_ActionPointsConsoleEntity = new SimpleConsoleNavigationEntity(m_ActionPointsButton);
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		m_NavigationBehaviour.Clear();
		GridConsoleNavigationBehaviour navigationBehaviour2 = new GridConsoleNavigationBehaviour();
		(m_StatusEffectsView as ICharInfoComponentConsoleView)?.AddInput(ref inputLayer, ref navigationBehaviour2, hintsWidget);
		navigationBehaviour2.AddRow<SimpleConsoleNavigationEntity>(m_MovePointsConsoleEntity);
		navigationBehaviour2.AddRow<SimpleConsoleNavigationEntity>(m_ActionPointsConsoleEntity);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(navigationBehaviour2);
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}
}
