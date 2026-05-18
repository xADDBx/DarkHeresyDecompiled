using System;
using System.Collections;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("cf3ab5673cd0a5a4ea851b06d66d443f")]
[ClassInfoBox("Unit pretends to be dead. Instant prone animation without ticking")]
public class FakeDeathAnimationState : UnitBuffComponentDelegate, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	public class TransientData : IEntityFactComponentTransientData
	{
		public bool IsFakingDeath;

		public Coroutine FreezeCoroutine;
	}

	protected override void OnActivate()
	{
		if (!base.IsReapplying)
		{
			TryFake();
		}
	}

	protected override void OnDeactivate()
	{
		if (base.IsReapplying)
		{
			return;
		}
		TransientData transientData = RequestTransientData<TransientData>();
		TryRemoveProne();
		if (transientData.FreezeCoroutine != null)
		{
			CoroutineRunner.Stop(transientData.FreezeCoroutine);
			transientData.FreezeCoroutine = null;
		}
		if (!(base.Owner.View.AnimationManager == null))
		{
			if (!base.Owner.View.Animator.enabled)
			{
				base.Owner.View.Animator.enabled = true;
			}
			base.Owner.View.AnimationManager.Disabled = false;
			base.Owner.View.AnimationManager.IsProne = false;
			base.Owner.View.AnimationManager.IsDead = false;
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (base.Owner.HoldingState != null)
		{
			TryFake();
		}
	}

	public void HandleUnitSpawned()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && base.Owner == baseUnitEntity)
		{
			TryFake();
		}
	}

	private void TryFake()
	{
		TransientData transientData = RequestTransientData<TransientData>();
		if (!transientData.IsFakingDeath && !(base.Owner.View?.AnimationManager == null))
		{
			TryApplyProne();
			if (!base.Owner.View.AnimationManager.IsProne || base.Owner.View.AnimationManager.IsGoingProne)
			{
				base.Owner.View.AnimationManager.IsProne = true;
				base.Owner.View.AnimationManager.Tick(0f);
			}
			transientData.FreezeCoroutine = CoroutineRunner.Start(TurnOffAnimatorAfterDelay((UnitEntity)base.Owner));
			transientData.IsFakingDeath = true;
		}
	}

	private IEnumerator TurnOffAnimatorAfterDelay(UnitEntity unit)
	{
		yield return new WaitForSeconds(1f);
		yield return new WaitWhile(() => unit.View.AnimationManager.Or(null)?.IsGoingProne ?? false);
		yield return null;
		if (unit.View.AnimationManager != null)
		{
			unit.View.AnimationManager.Disabled = true;
		}
	}

	private void TryApplyProne()
	{
		if (!base.Owner.Features.Prone)
		{
			base.Owner.Features.Prone.Retain(base.Fact);
		}
	}

	private void TryRemoveProne()
	{
		if ((bool)base.Owner.Features.Prone)
		{
			base.Owner.Features.Prone.Release(base.Fact);
		}
	}
}
