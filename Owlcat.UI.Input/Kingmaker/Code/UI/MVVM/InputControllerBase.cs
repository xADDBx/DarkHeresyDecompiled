using System;
using System.Collections.Generic;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InputControllerBase : IDisposable
{
	protected internal enum ModeType
	{
		None,
		FreeMovement,
		FreeCamera
	}

	internal class Mode
	{
		public virtual void OnEnter(InputContext context)
		{
		}

		public virtual void OnUpdate(InputContext context, InputFrame frame)
		{
		}

		public virtual void OnExit(InputContext context)
		{
		}
	}

	protected DisposableBag m_Bag;

	private readonly Dictionary<ModeType, Mode> m_Modes;

	private ModeType m_Mode;

	public InputControllerBase()
	{
		m_Mode = ModeType.None;
		m_Modes = new Dictionary<ModeType, Mode>
		{
			{
				ModeType.None,
				new Mode()
			},
			{
				ModeType.FreeMovement,
				new FreeMovementMode()
			},
			{
				ModeType.FreeCamera,
				new FreeCameraMode()
			}
		};
		Observable.EveryUpdate(UnityFrameProvider.PreUpdate).Subscribe(OnUpdate).AddTo(ref m_Bag);
	}

	public void Dispose()
	{
		m_Bag.Dispose();
	}

	private void OnUpdate(Unit _)
	{
		InputFrame inputFrame = GetInputFrame();
		InputContext inputContext = GetInputContext();
		ModeType inputMode = GetInputMode();
		if (m_Mode != inputMode)
		{
			m_Modes[m_Mode].OnExit(inputContext);
			m_Mode = inputMode;
			m_Modes[m_Mode].OnEnter(inputContext);
		}
		m_Modes[m_Mode].OnUpdate(inputContext, inputFrame);
	}

	protected abstract InputContext GetInputContext();

	protected abstract InputFrame GetInputFrame();

	protected abstract ModeType GetInputMode();
}
