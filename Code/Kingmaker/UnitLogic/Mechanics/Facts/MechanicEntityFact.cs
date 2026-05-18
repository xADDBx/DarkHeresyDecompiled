using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Facts;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicEntityFact : EntityFact, IMechanicEntityFact, IHashable, IOwlPackable<MechanicEntityFact>
{
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	protected MechanicsContext m_ParentContext;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	protected MechanicsContext m_Context;

	public override Type RequiredEntityType => EntityInterfacesHelper.MechanicEntityInterface;

	public new BlueprintMechanicEntityFact Blueprint => (BlueprintMechanicEntityFact)base.Blueprint;

	public new MechanicEntity Owner => (MechanicEntity)base.Owner;

	public virtual bool Hidden => (Owner?.GetOptional<PartHiddenFacts>()?.IsHidden(Blueprint)).GetValueOrDefault();

	public override MechanicsContext MaybeContext => m_Context;

	[NotNull]
	public MechanicsContext Context => m_Context ?? throw new NullReferenceException();

	[CanBeNull]
	public MechanicsContext MaybeParentContext => m_ParentContext;

	public MechanicEntity Caster => m_ParentContext?.MaybeCaster ?? Owner;

	public override string Description
	{
		get
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Owner;
				return base.Description;
			}
		}
	}

	public MechanicEntityFact(BlueprintMechanicEntityFact fact, [CanBeNull] IEvalContext parentContext)
		: base(fact)
	{
		m_ParentContext = EvalContext.ClaimMechanicsContext(parentContext);
	}

	public MechanicEntityFact()
	{
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		m_Context = MechanicsContext.Claim(Blueprint, m_ParentContext?.MaybeCaster, Owner, m_ParentContext, null, this);
	}

	protected override void OnDetach()
	{
		m_Context?.Dispose();
		m_Context = null;
		base.OnDetach();
	}

	protected override void OnComponentsDidActivated()
	{
		Owner.Actor.HandleFactActivated(this);
		base.OnComponentsDidActivated();
	}

	protected override void OnComponentsDidPostLoad()
	{
		base.OnComponentsDidPostLoad();
		if (base.IsActive)
		{
			Owner.Actor.HandleFactActivated(this);
		}
	}

	protected override void OnDeactivate()
	{
		Owner.Actor.HandleFactDeactivated(this);
		base.OnDeactivate();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicsContext>.GetHash128(m_ParentContext);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<MechanicsContext>.GetHash128(m_Context);
		result.Append(ref val3);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicEntityFact<TOwner> : MechanicEntityFact, IHashable, IOwlPackable<MechanicEntityFact<TOwner>> where TOwner : MechanicEntity
{
	public override Type RequiredEntityType => typeof(TOwner);

	public new TOwner Owner => (TOwner)base.Owner;

	public MechanicEntityFact(BlueprintMechanicEntityFact fact, IEvalContext parentContext)
		: base(fact, parentContext)
	{
	}

	public MechanicEntityFact()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
