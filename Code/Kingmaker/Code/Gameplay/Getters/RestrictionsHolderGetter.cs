using Code.Utility.Attributes;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Getters;

[TypeId("bf065770ee9b44ba9790f245a86625d9")]
public class RestrictionsHolderGetter : BoolPropertyGetter
{
	[SerializeField]
	[ValidateNotNull]
	[InlineBlueprint]
	private RestrictionsHolder.Reference m_Property;

	public RestrictionsHolder Property => m_Property;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"#{Property}";
	}

	protected override bool GetBaseValue()
	{
		return Property.IsPassed(base.CurrentEntity, base.Context, base.Context.Target);
	}
}
