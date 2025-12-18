using System;
using Kingmaker.Localization;

namespace Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;

[Serializable]
public class DetectiveRadarSignalSettings
{
	public DetectiveRadarSignalType SignalType;

	public float Radius = 3f;

	public float Power = 1f;

	public SharedStringAsset SourceName;

	public bool IsJammer => SignalType == DetectiveRadarSignalType.Jammer;
}
