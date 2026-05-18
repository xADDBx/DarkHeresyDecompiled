using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.SignalDevice;

public class SignalFxContext : IDetectiveRadarHandler, ISubscriber, IDisposable
{
	private readonly ReactiveCommand<Unit> m_DoPulse = new ReactiveCommand<Unit>();

	private float m_TimeToNextPulse;

	private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

	public static SignalFxContext Instance { get; private set; }

	public Observable<Unit> DoPulse => m_DoPulse;

	private DetectiveRadarController Controller => Game.Instance.Controllers.DetectiveRadarController;

	private SignalUISettings Root => DetectiveClueSignalRoot.Instance.UISettings;

	public SignalFxContext()
	{
		Instance?.Dispose();
		Instance = this;
		EventBus.Subscribe(this).AddTo(m_Disposable);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			Update();
		}).AddTo(m_Disposable);
	}

	public void Dispose()
	{
		m_Disposable?.Clear();
	}

	private void Update()
	{
		if (Controller.SignalState == DetectiveRadarState.Activated)
		{
			if (m_TimeToNextPulse > 0f)
			{
				m_TimeToNextPulse -= Time.deltaTime;
				return;
			}
			m_DoPulse.Execute(Unit.Default);
			m_TimeToNextPulse = Root.GetWavesDelay(Controller.SignalPowerClamped01);
		}
	}

	public void HandleRadarModeChange(DetectiveRadarState state)
	{
		if (state == DetectiveRadarState.Activated)
		{
			m_TimeToNextPulse = 0f;
		}
	}

	public void HandleNearestSignalTurnedOn()
	{
	}
}
