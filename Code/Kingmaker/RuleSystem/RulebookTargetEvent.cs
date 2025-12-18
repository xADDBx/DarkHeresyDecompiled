using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem;

public abstract class RulebookTargetEvent : RulebookEvent
{
	private readonly MechanicEntity _target;

	public MechanicEntity ConcreteTarget => _target;

	[CanBeNull]
	public BaseUnitEntity TargetUnit => _target as BaseUnitEntity;

	public override MechanicEntity Target => _target;

	protected RulebookTargetEvent([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target)
		: this((MechanicEntity)initiator, (MechanicEntity)target)
	{
	}

	protected RulebookTargetEvent([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target)
		: base(initiator)
	{
		_target = target;
	}
}
public abstract class RulebookTargetEvent<TTarget> : RulebookTargetEvent where TTarget : MechanicEntity
{
	[NotNull]
	public new TTarget Target => (TTarget)base.Target;

	protected RulebookTargetEvent([NotNull] MechanicEntity initiator, TTarget target)
		: base(initiator, target)
	{
	}

	protected RulebookTargetEvent([NotNull] IMechanicEntity initiator, TTarget target)
		: this((MechanicEntity)initiator, target)
	{
	}
}
public abstract class RulebookTargetEvent<TInitiator, TTarget> : RulebookTargetEvent where TInitiator : MechanicEntity where TTarget : MechanicEntity
{
	[NotNull]
	public new TInitiator Initiator => (TInitiator)base.Initiator;

	[NotNull]
	public new TTarget Target => (TTarget)base.Target;

	protected RulebookTargetEvent([NotNull] TInitiator initiator, TTarget target)
		: base(initiator, target)
	{
	}
}
