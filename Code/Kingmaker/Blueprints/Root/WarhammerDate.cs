using System;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("113550c6375846b19e75c6ff79ee5e2c")]
public class WarhammerDate : BlueprintScriptableObject
{
	public int Millenniums;

	public int AMRCYears;

	public int VVYears;

	public int Segments;

	public string Time = "00:00";

	public TimeSpan GetTime()
	{
		try
		{
			return TimeSpan.Parse(Time);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		return TimeSpan.Zero;
	}

	public TimeSpan GetVVDate()
	{
		return VVYears.WarhammerYears() + Segments.Segments();
	}

	public TimeSpan GetAMRCDate()
	{
		return AMRCYears.WarhammerYears() + Segments.Segments();
	}
}
