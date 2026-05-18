using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("4ba3c72bb22d4da7857a6fbcdfd82f46")]
[PlayerUpgraderAllowed(true)]
public class SwitchFaction : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Faction")]
	private BlueprintFactionReference m_Faction;

	public bool IncludeGroup = true;

	public BlueprintFaction Faction => m_Faction?.Get();

	protected override void RunAction()
	{
		if (!(Target.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		List<Entity> list;
		using ((baseUnitEntity.Group?.Members ?? Enumerable.Empty<AbstractUnitEntity>()).ToPooledList(out list))
		{
			if (list.Count > 1 && !IncludeGroup)
			{
				list.Clear();
				list.Add(baseUnitEntity);
			}
			if (baseUnitEntity.CombatGroup.Group.Units.Count > 1 && !IncludeGroup)
			{
				baseUnitEntity.CombatGroup.Id = Uuid.Instance.CreateString();
			}
			if (!list.Contains(baseUnitEntity))
			{
				list.Add(baseUnitEntity);
			}
			HashSet<UnitGroup> value;
			using (CollectionPool<HashSet<UnitGroup>, UnitGroup>.Get(out value))
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is BaseUnitEntity baseUnitEntity2 && baseUnitEntity2.Faction.Blueprint != Faction)
					{
						baseUnitEntity2.Faction.Set(Faction);
						value.Add(baseUnitEntity2.CombatGroup.Group);
					}
				}
				foreach (UnitGroup item in value)
				{
					item.ResetFactionSet();
				}
			}
		}
	}

	public override string GetCaption()
	{
		return $"Switch {Target} faction to {Faction.NameSafe()}";
	}
}
