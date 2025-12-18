using System.Collections.Generic;
using Kingmaker.Blueprints.Base;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.TextTools;

public class ContextUnitMaleFemaleTemplate : TextTemplate
{
	public override int MinParameters => 2;

	public override int MaxParameters => 2;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		BaseUnitEntity baseUnitEntity = (GameLogContext.InScope ? GameLogContext.TargetEntity.Value : null) as BaseUnitEntity;
		int num = (int)(baseUnitEntity?.GetDescriptionOptional()?.Gender ?? baseUnitEntity?.Blueprint.Gender ?? Gender.Male);
		if (parameters.Count > num)
		{
			return parameters[num];
		}
		return "";
	}
}
