using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.PreciseAttackOvertip;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;

public class PreciseAttackVM : ViewModel, IAbilityTargetSelectionUIHandler, ISubscriber
{
	private MechanicEntityUIWrapper m_TargetUIWrapper;

	private readonly PreciseAttackController m_PreciseAttackController;

	private readonly ReactiveCommand<Unit> m_PointsUpdated = new ReactiveCommand<Unit>();

	public readonly PreciseAttackOvertipVM OvertipVM;

	public IEnumerable<PreciseAttackController.BodyPartUIData> BodyParts => m_PreciseAttackController.Points;

	public Observable<Unit> PointsUpdated => m_PointsUpdated;

	public Observable<MechanicEntity> TargetChanged => m_PreciseAttackController.Target;

	public PreciseAttackController.BodyPartUIData SelectedBodyPart => m_PreciseAttackController.SelectedBodyPart.CurrentValue;

	public MechanicEntity Unit => m_TargetUIWrapper.MechanicEntity;

	public PreciseAttackVM(PreciseAttackController preciseAttackController)
	{
		m_PreciseAttackController = preciseAttackController;
		m_PreciseAttackController.Target.Subscribe(UpdateTarget).AddTo(this);
		OvertipVM = new PreciseAttackOvertipVM(m_PreciseAttackController.Target).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		MechanicEntity unit = Unit;
		BaseUnitEntity targetUnit = unit as BaseUnitEntity;
		if (targetUnit != null)
		{
			EventBus.RaiseEvent(delegate(IPreciseAttackUIHandler h)
			{
				h.HandleOpenPreciseAttackInterface(targetUnit, preciseAttackController.IsTargetCovered());
			});
		}
	}

	public void Close()
	{
		m_PreciseAttackController.HandlePreciseAttackCancelled();
	}

	public void Accept()
	{
		m_PreciseAttackController.HandlePreciseAttackAccepted();
	}

	public void SelectNext()
	{
		m_PreciseAttackController.SelectNextTarget();
	}

	public void SelectPrev()
	{
		m_PreciseAttackController.SelectPrevTarget();
	}

	public void SetSelectedBodyPart(PreciseAttackController.BodyPartUIData bodyPart)
	{
		m_PreciseAttackController.SetSelectedBodyPart(bodyPart);
	}

	public void SetHoveredBodyPart(PreciseAttackController.BodyPartUIData bodyPart)
	{
		m_PreciseAttackController.SetHoveredBodyPart(bodyPart);
	}

	public List<FxLocator> GetBodyPartBone(PreciseAttackController.BodyPartUIData bodyPart)
	{
		List<FxLocator> result = new List<FxLocator>();
		ParticlesSnapMap particlesSnapMap = Unit.View.ParticlesSnapMap;
		List<BpRef<BlueprintFxLocatorGroup>> list = bodyPart.BodyPart?.FxLocatorGroups;
		if (particlesSnapMap != null && list != null)
		{
			result = particlesSnapMap.GetLocatorsWithGroups(list);
		}
		return result;
	}

	public float GetHitChance(BlueprintBodyPart bodyPart)
	{
		return m_PreciseAttackController.GetBodyPartHitChance(bodyPart);
	}

	void IAbilityTargetSelectionUIHandler.HandleAbilityTargetSelectionStart(AbilityData ability)
	{
	}

	void IAbilityTargetSelectionUIHandler.HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		Close();
	}

	private void UpdateTarget(MechanicEntity target)
	{
		m_TargetUIWrapper = new MechanicEntityUIWrapper(target);
	}
}
