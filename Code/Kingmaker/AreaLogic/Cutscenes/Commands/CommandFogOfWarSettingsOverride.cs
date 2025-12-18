using Kingmaker.Blueprints.Attributes;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandFogOfWarSettingsOverride")]
[TypeId("793e48f1f6164577ba42a886cc7cc2a9")]
public class CommandFogOfWarSettingsOverride : CommandBase
{
	[SerializeField]
	public bool OverrideColor;

	[SerializeField]
	[ShowIf("OverrideColor")]
	public Color Color;

	[SerializeField]
	public bool OverrideColorAlpha;

	[SerializeField]
	[ShowIf("OverrideColorAlpha")]
	public float ColorAlpha;

	[SerializeField]
	public bool OverrideOuterRadius;

	[SerializeField]
	[ShowIf("OverrideOuterRadius")]
	public float RevealerOuterRadius;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!(FogOfWarSettings.Instance == null))
		{
			FogOfWarSettingsOverride fogOfWarSettingsOverride = new FogOfWarSettingsOverride(FogOfWarSettings.Instance);
			if (OverrideColor)
			{
				fogOfWarSettingsOverride.Color.r = Color.r;
				fogOfWarSettingsOverride.Color.g = Color.g;
				fogOfWarSettingsOverride.Color.b = Color.b;
			}
			if (OverrideColorAlpha)
			{
				fogOfWarSettingsOverride.Color.a = ColorAlpha;
			}
			if (OverrideOuterRadius)
			{
				fogOfWarSettingsOverride.RevealerOuterRadius = RevealerOuterRadius;
			}
			FogOfWarSettings.Instance.SetOverride(fogOfWarSettingsOverride);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if (FogOfWarSettings.Instance != null)
		{
			FogOfWarSettings.Instance.ClearOverride();
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	public override string GetCaption()
	{
		return "<b>Fog of war settings override</b>";
	}
}
