using System.Linq;
using DG.Tweening;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.View.Bridge.Canvas;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class FadeVM : ViewModel, ILoadingScreen, IGameModeHandler, ISubscriber
{
	public struct AdvancedParams
	{
		public Ease Ease;

		public float Duration;
	}

	public struct Params
	{
		public bool Fade;

		public AdvancedParams? FadeParams;
	}

	private LoadingScreenState m_State;

	private readonly ReactiveProperty<bool> m_CutsceneOverlay = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<Params> LoadingScreen = new ReactiveProperty<Params>();

	private AdvancedParams? FadeParams;

	public ReadOnlyReactiveProperty<bool> CutsceneOverlay => m_CutsceneOverlay;

	public static FadeVM? Instance { get; private set; }

	public FadeVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		DoCutScene(Game.Instance.CurrentModeType == GameModeType.Cutscene);
		FadeCanvas.Instance.FadeoutCommand.Subscribe(Fadeout).AddTo(this);
		FadeCanvas.Instance.FadeoutAdvancedCommand.Subscribe(Fadeout).AddTo(this);
		FadeCanvas.Instance.ShowLoadingScreenCommand.Subscribe(ShowLoadingScreen).AddTo(this);
		FadeCanvas.Instance.HideLoadingScreenCommand.Subscribe(HideLoadingScreen).AddTo(this);
		Instance = this;
	}

	public void ShowLoadingScreen()
	{
		if (m_State != LoadingScreenState.Shown && m_State != LoadingScreenState.ShowAnimation)
		{
			PFLog.UI.Log("Show fade");
			m_State = LoadingScreenState.ShowAnimation;
			LoadingScreen.Value = new Params
			{
				Fade = true,
				FadeParams = FadeParams
			};
		}
	}

	public void HideLoadingScreen()
	{
		if (m_State == LoadingScreenState.Hidden || m_State == LoadingScreenState.HideAnimation)
		{
			return;
		}
		PFLog.UI.Log("Hide fade");
		m_State = LoadingScreenState.HideAnimation;
		LoadingScreen.Value = new Params
		{
			Fade = false,
			FadeParams = FadeParams
		};
		if (Game.Instance.CurrentModeType == GameModeType.StarSystem)
		{
			EventBus.RaiseEvent(delegate(ISystemMapRadarHandler h)
			{
				h.HandleShowSystemMapRadar();
			});
		}
	}

	public LoadingScreenState GetLoadingScreenState()
	{
		return m_State;
	}

	public void Fadeout(bool fade)
	{
		Fadeout(fade, null);
	}

	public void Fadeout(FadeoutAdvancedParams p)
	{
		Fadeout(p.Fade, new AdvancedParams
		{
			Ease = p.Ease,
			Duration = p.Duration
		});
	}

	private void Fadeout(bool fade, AdvancedParams? fadeParams)
	{
		FadeParams = fadeParams;
		if (fade)
		{
			LoadingProcess.Instance.ShowManualLoadingScreen(this);
		}
		else
		{
			LoadingProcess.Instance.HideManualLoadingScreen();
		}
	}

	private void DoCutScene(bool state)
	{
		bool flag = Game.Instance.EntityPools.Cutscenes.Any((CutscenePlayerData c) => c.HasActiveLockControl && c.Cutscene.ShowOverlay);
		m_CutsceneOverlay.Value = state && flag;
	}

	public void SetStateShowAnimation()
	{
		m_State = LoadingScreenState.ShowAnimation;
	}

	public void SetStateShown()
	{
		m_State = LoadingScreenState.Shown;
	}

	public void SetStateHideAnimation()
	{
		m_State = LoadingScreenState.HideAnimation;
	}

	public void SetStateHidden()
	{
		m_State = LoadingScreenState.Hidden;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			DoCutScene(state: true);
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			DoCutScene(state: false);
		}
	}
}
