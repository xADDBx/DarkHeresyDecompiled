using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilityStats
{
	public static string GetGlossaryName(StatType stat)
	{
		if (stat == StatType.CohesionRange)
		{
			return "Influence";
		}
		return stat.ToString();
	}
}
