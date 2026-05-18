using Kingmaker.EntitySystem.Properties;

namespace Kingmaker.Framework.ContextContract.Roles;

public static class PropertyTargetTypeMapping
{
	public static ContextField ToContextField(PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => ContextField.CurrentEntity, 
			PropertyTargetType.CurrentTarget => ContextField.Target, 
			PropertyTargetType.ContextCaster => ContextField.Caster, 
			PropertyTargetType.ContextOwner => ContextField.Owner, 
			PropertyTargetType.ContextMainTarget => ContextField.ClickedTarget, 
			PropertyTargetType.RuleInitiator => ContextField.RuleInitiator, 
			PropertyTargetType.RuleTarget => ContextField.RuleTarget, 
			_ => ContextField.Caster, 
		};
	}

	public static PropertyTargetType? FromContextField(ContextField field)
	{
		return field switch
		{
			ContextField.CurrentEntity => PropertyTargetType.CurrentEntity, 
			ContextField.Target => PropertyTargetType.CurrentTarget, 
			ContextField.Caster => PropertyTargetType.ContextCaster, 
			ContextField.Owner => PropertyTargetType.ContextOwner, 
			ContextField.ClickedTarget => PropertyTargetType.ContextMainTarget, 
			ContextField.RuleInitiator => PropertyTargetType.RuleInitiator, 
			ContextField.RuleTarget => PropertyTargetType.RuleTarget, 
			_ => null, 
		};
	}

	public static ContextField[] EffectiveFallbackChain(PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => new ContextField[3]
			{
				ContextField.CurrentEntity,
				ContextField.ClickedTarget,
				ContextField.Owner
			}, 
			PropertyTargetType.CurrentTarget => new ContextField[3]
			{
				ContextField.Target,
				ContextField.ClickedTarget,
				ContextField.Owner
			}, 
			PropertyTargetType.ContextMainTarget => new ContextField[2]
			{
				ContextField.ClickedTarget,
				ContextField.Owner
			}, 
			PropertyTargetType.ContextCaster => new ContextField[2]
			{
				ContextField.Caster,
				ContextField.Owner
			}, 
			PropertyTargetType.ContextOwner => new ContextField[2]
			{
				ContextField.Owner,
				ContextField.Caster
			}, 
			PropertyTargetType.RuleInitiator => new ContextField[1] { ContextField.RuleInitiator }, 
			PropertyTargetType.RuleTarget => new ContextField[1] { ContextField.RuleTarget }, 
			_ => new ContextField[1] { ToContextField(type) }, 
		};
	}
}
