using System;
using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class BulletTemplate : TextTemplate
{
	private enum BulletTypes
	{
		Neutral,
		Positive,
		Negative,
		Cost
	}

	private const string BulletDefault = "//-";

	private static readonly Dictionary<BulletTypes, string> BulletTypeToFormat = new Dictionary<BulletTypes, string>
	{
		{
			BulletTypes.Neutral,
			"<style=Bullet.Neutral>//-</style>"
		},
		{
			BulletTypes.Positive,
			"<style=Bullet.Positive>//-</style>"
		},
		{
			BulletTypes.Negative,
			"<style=Bullet.Negative>//-</style>"
		},
		{
			BulletTypes.Cost,
			"<style=Bullet.Cost></style>"
		}
	};

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (parameters == null || parameters.Count < 1)
		{
			return string.Empty;
		}
		if (!Enum.TryParse<BulletTypes>(parameters[0], out var result))
		{
			return string.Empty;
		}
		if (!BulletTypeToFormat.TryGetValue(result, out var value))
		{
			return string.Empty;
		}
		return value;
	}
}
