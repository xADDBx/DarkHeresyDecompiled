using System;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.Code.UI.MVVM.View;

[Serializable]
public class CareerPathRoundProgressionConfig
{
	public CareerPathTier Tier;

	public int ItemsRadius;

	public int ProgressBarSize;
}
