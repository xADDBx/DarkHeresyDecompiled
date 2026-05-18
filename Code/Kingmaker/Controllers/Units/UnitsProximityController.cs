using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers.Units;

public class UnitsProximityController : BaseUnitController
{
	private struct GlobalCooldownCandidate
	{
		public AbstractUnitEntity Unit;

		public BaseUnitEntity User;

		public IUnitInteraction Interaction;

		public int InteractionIndex;

		public float Distance;
	}

	private GlobalCooldownCandidate? m_BestGlobalCooldownCandidate;

	private InteractionGlobalCooldownController m_CooldownController;

	protected override void BeforeTick()
	{
		m_BestGlobalCooldownCandidate = null;
		m_CooldownController = Game.Instance.GetController<InteractionGlobalCooldownController>();
	}

	protected override void AfterTick()
	{
		GlobalCooldownCandidate? bestGlobalCooldownCandidate = m_BestGlobalCooldownCandidate;
		if (!bestGlobalCooldownCandidate.HasValue)
		{
			return;
		}
		GlobalCooldownCandidate valueOrDefault = bestGlobalCooldownCandidate.GetValueOrDefault();
		PartUnitInteractions optional = valueOrDefault.Unit.GetOptional<PartUnitInteractions>();
		if (optional != null)
		{
			if (!(valueOrDefault.Interaction is IGlobalCooldownUser { UseGlobalCooldown: not false }))
			{
				valueOrDefault.Unit.Commands.InterruptAllInterruptible();
			}
			valueOrDefault.Interaction.Interact(valueOrDefault.User, valueOrDefault.Unit);
			optional.Cooldowns[valueOrDefault.InteractionIndex] = valueOrDefault.Interaction.ApproachCooldown;
		}
	}

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		PartUnitInteractions optional = unit.GetOptional<PartUnitInteractions>();
		if (optional == null)
		{
			return;
		}
		optional.UpdateCooldowns();
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.Stealth.Active)
			{
				continue;
			}
			float num = partyAndPet.DistanceTo(unit);
			if (!optional.Distances.TryGetValue(partyAndPet, out var value))
			{
				value = 1000000f;
			}
			optional.Distances[partyAndPet] = num;
			for (int i = 0; i < optional.Interactions.Count; i++)
			{
				IUnitInteraction unitInteraction = optional.Interactions[i];
				if (!unitInteraction.IsApproach || optional.Cooldowns[i] > 0f || num > (float)unitInteraction.Distance)
				{
					continue;
				}
				IGlobalCooldownUser globalCooldownUser = unitInteraction as IGlobalCooldownUser;
				bool flag = globalCooldownUser?.UseGlobalCooldown ?? false;
				bool flag2 = flag && globalCooldownUser.CanCluster && m_CooldownController != null && m_CooldownController.IsInCluster(unit.UniqueId);
				if ((!flag && value < (float)unitInteraction.Distance) || !unitInteraction.IsAvailable(partyAndPet, unit))
				{
					continue;
				}
				if (flag)
				{
					if (flag2)
					{
						m_CooldownController.AddClusterCandidate(unit, partyAndPet, unitInteraction, i, num);
					}
					else if (!m_BestGlobalCooldownCandidate.HasValue || num < m_BestGlobalCooldownCandidate.Value.Distance)
					{
						m_BestGlobalCooldownCandidate = new GlobalCooldownCandidate
						{
							Unit = unit,
							User = partyAndPet,
							Interaction = unitInteraction,
							InteractionIndex = i,
							Distance = num
						};
					}
				}
				else
				{
					unit.Commands.InterruptAllInterruptible();
					unitInteraction.Interact(partyAndPet, unit);
					optional.Cooldowns[i] = unitInteraction.ApproachCooldown;
				}
			}
		}
	}

	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		if (unit.IsInCombat)
		{
			return false;
		}
		return unit.GetOptional<PartUnitInteractions>()?.HasApproachInteractions ?? false;
	}
}
