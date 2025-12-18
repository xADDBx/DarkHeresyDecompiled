using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.Code.View.Scene;

public class FogOfWarSceneOverrider : MonoBehaviour, ICutsceneHandler, ISubscriber<CutscenePlayerData>, ISubscriber
{
	[SerializeField]
	private bool m_OverrideColor;

	[SerializeField]
	[ShowIf("m_OverrideColor")]
	private Color m_FogColor = Color.black;

	[SerializeField]
	private bool m_OverrideColorAlpha;

	[SerializeField]
	[ShowIf("m_OverrideColorAlpha")]
	[Range(0f, 1f)]
	private float m_FogAlpha = 0.3529412f;

	[SerializeField]
	private bool m_OverrideOuterRadius;

	[SerializeField]
	[ShowIf("m_OverrideOuterRadius")]
	private float m_RevealerOuterRadius;

	private void Awake()
	{
		EventBus.Subscribe(this);
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
		OverrideFogOfWar();
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		RestoreFogOfWar();
	}

	private void OnDestroy()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleCutsceneStarted(bool queued)
	{
	}

	public void HandleCutsceneRestarted()
	{
	}

	public void HandleCutscenePaused(CutscenePauseReason reason)
	{
	}

	public void HandleCutsceneResumed()
	{
	}

	public void HandleCutsceneStopped()
	{
		OverrideFogOfWar();
	}

	private void OverrideFogOfWar()
	{
		if (!(FogOfWarSettings.Instance == null))
		{
			FogOfWarSettingsOverride fogOfWarSettingsOverride = new FogOfWarSettingsOverride(FogOfWarSettings.Instance);
			if (m_OverrideColor)
			{
				fogOfWarSettingsOverride.Color.r = m_FogColor.r;
				fogOfWarSettingsOverride.Color.g = m_FogColor.g;
				fogOfWarSettingsOverride.Color.b = m_FogColor.b;
			}
			if (m_OverrideColorAlpha)
			{
				fogOfWarSettingsOverride.Color.a = m_FogAlpha;
			}
			if (m_OverrideOuterRadius)
			{
				fogOfWarSettingsOverride.RevealerOuterRadius = m_RevealerOuterRadius;
			}
			FogOfWarSettings.Instance.SetOverride(fogOfWarSettingsOverride);
		}
	}

	private void RestoreFogOfWar()
	{
		if (!(FogOfWarSettings.Instance == null))
		{
			FogOfWarSettings.Instance.ClearOverride();
		}
	}
}
