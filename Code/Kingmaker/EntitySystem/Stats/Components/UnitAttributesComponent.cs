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
[ComponentName("Stats/Unit/UnitAttributesComponent")]
[TypeId("3f86aa3b2e594398baa46c5d74bc49c8")]
public sealed class UnitAttributesComponent : BlueprintComponent
{
	[Serializable]
	public sealed class Attribute
	{
		[ArrayElementNameProvider]
		public AttributeType Type;

		public int Value;
	}

	public int DefaultValue = 30;

	public Attribute[] Attributes = new Attribute[0];

	public int? GetAttribute(AttributeType attribute)
	{
		return Attributes.FirstItem((Attribute i) => i.Type == attribute)?.Value ?? DefaultValue;
	}
}
