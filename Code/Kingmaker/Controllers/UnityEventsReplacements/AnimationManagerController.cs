using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation;
using UnityEngine.Playables;

namespace Kingmaker.Controllers.UnityEventsReplacements;

public class AnimationManagerController : IControllerTick, IController
{
	private readonly UpdatableQueue<IAnimationManager> m_AnimationManagers = new UpdatableQueue<IAnimationManager>();

	public void Subscribe(IAnimationManager animationManager)
	{
		m_AnimationManagers.Add(animationManager);
	}

	public void Unsubscribe(IAnimationManager animationManager)
	{
		m_AnimationManagers.Remove(animationManager);
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		float unscaledDeltaTime = Game.Instance.RealTimeController.SystemDeltaTime;
		float deltaTime = Game.Instance.Controllers.TimeController.GameDeltaTime;
		bool isSimulationTick = Game.Instance.Controllers.TimeController.IsSimulationTick;
		m_AnimationManagers.Prepare();
		IAnimationManager value;
		while (m_AnimationManagers.Next(out value))
		{
			try
			{
				TickOnAnimationManager(value);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		void TickOnAnimationManager(IAnimationManager animationManager)
		{
			if (animationManager != null && animationManager.IsValid)
			{
				DirectorUpdateMode updateMode = animationManager.UpdateMode;
				if (updateMode == DirectorUpdateMode.UnscaledGameTime || isSimulationTick)
				{
					float deltaTime2 = ((updateMode == DirectorUpdateMode.UnscaledGameTime) ? unscaledDeltaTime : (deltaTime * animationManager.PlayingSpeed));
					animationManager.CustomUpdate(deltaTime2);
				}
			}
		}
	}
}
