using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Stats.Components;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("Stats/Unit/UnitSkillsComponent")]
[TypeId("3b57f256fbac44f9a675879b999a034f")]
public sealed class UnitSkillsComponent : BlueprintComponent
{
	[Serializable]
	public sealed class Skill
	{
		[ArrayElementNameProvider]
		public SkillType Type;

		public int Value;
	}

	public Skill[] Skills = new Skill[0];

	public int? GetAttribute(SkillType skill)
	{
		return Skills.FirstItem((Skill i) => i.Type == skill)?.Value;
	}
}
