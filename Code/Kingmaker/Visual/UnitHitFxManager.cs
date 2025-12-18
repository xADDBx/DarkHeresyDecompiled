using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual;

public class UnitHitFxManager : MonoBehaviour
{
	private class ReachFXMover : MonoBehaviour
	{
		public Transform From;

		public Transform To;

		public float Start;

		public float Finish;

		private void Update()
		{
			if ((bool)From && (bool)To && Finish > Start)
			{
				float num = (Time.time - Start) / (Finish - Start);
				base.transform.position = Vector3.Lerp(From.position, To.position, num);
				base.transform.LookAt(To.position);
				if (num < 1f)
				{
					return;
				}
			}
			FxHelper.Destroy(base.gameObject);
		}
	}

	private UnitEntityView m_View;

	private bool m_HasDodgeAction;

	public void HandleMeleeAttackHit(BaseUnitEntity attacker, AttackResult attackResult, bool crit, ItemEntityWeapon attackWeapon)
	{
		switch (attackResult)
		{
		case AttackResult.Hit:
		{
			float num = Vector3.Distance(attacker.Position, m_View.EntityData.Position) - m_View.Corpulence - attacker.Corpulence;
			float num2 = attacker.Corpulence * ConfigRoot.Instance.SystemMechanics.ReachFXBaseRange;
			if (num >= num2)
			{
				SpawnReachHitFX(attacker);
			}
			return;
		}
		case AttackResult.Defended:
			if (!CanDodge())
			{
				attackResult = AttackResult.Miss;
			}
			break;
		}
		if (attackResult != AttackResult.Defended)
		{
			return;
		}
		UnitAnimationManager animationManager = m_View.AnimationManager;
		if ((object)animationManager != null)
		{
			UnitAnimationActionHandle unitAnimationActionHandle = animationManager.CreateHandle(UnitAnimationType.Defence);
			if (unitAnimationActionHandle != null)
			{
				animationManager.Execute(unitAnimationActionHandle);
			}
		}
	}

	private void SpawnReachHitFX(BaseUnitEntity attacker)
	{
		FxHelper.SpawnFxOnEntity(ConfigRoot.Instance.SystemMechanics.ReachFXTargetPrefab, m_View);
		GameObject gameObject = FxHelper.SpawnFxOnEntity(ConfigRoot.Instance.SystemMechanics.ReachFXMovingPrefab, attacker.View);
		if ((bool)gameObject)
		{
			string reachFXLocatorName = ConfigRoot.Instance.SystemMechanics.ReachFXLocatorName;
			Transform transform = attacker.View.ParticlesSnapMap[reachFXLocatorName]?.Transform;
			Transform transform2 = m_View.ParticlesSnapMap[reachFXLocatorName]?.Transform;
			if ((bool)transform && (bool)transform2)
			{
				ReachFXMover reachFXMover = gameObject.EnsureComponent<ReachFXMover>();
				reachFXMover.From = transform;
				reachFXMover.To = transform2;
				reachFXMover.Start = Time.time;
				reachFXMover.Finish = Time.time + ConfigRoot.Instance.SystemMechanics.ReachFXFlightTime;
			}
			else
			{
				FxHelper.Destroy(gameObject);
			}
		}
	}

	private bool CanDodge()
	{
		if (m_HasDodgeAction)
		{
			return m_View.AnimationManager.CanRunIdleAction();
		}
		return false;
	}

	public void SetView(UnitEntityView unitEntityView)
	{
		m_View = unitEntityView;
		m_HasDodgeAction = m_View.AnimationManager != null && m_View.AnimationManager.AnimationSet != null && (bool)m_View.AnimationManager.AnimationSet.GetAction(UnitAnimationType.Defence);
	}
}
