using Kingmaker.Code.View.UI.UIUtils;
using Kingmaker.GameModes;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class GameOverContext : ViewModel
{
	private readonly ReactiveProperty<GameOverVM> m_GameOver;

	public GameOverContext(ReactiveProperty<GameOverVM> gameOver)
	{
		m_GameOver = gameOver;
		GameUIState.Instance.GameMode.Subscribe(OnGameModeChanged).AddTo(this);
	}

	private void OnGameModeChanged(GameModeType gameMode)
	{
		if (gameMode == GameModeType.GameOver)
		{
			m_GameOver.Value = new GameOverVM().AddTo(this);
		}
		else
		{
			m_GameOver.ClearDisposableValue();
		}
	}
}
