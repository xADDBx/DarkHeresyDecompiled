using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual.Critters;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("2a8e14bbfbc525d4ebd89cb6f464b28e")]
public class FamiliarSettingsOverride : EntityFactComponentDelegate<MechanicEntity>
{
	[SerializeField]
	[ValidateNotNull]
	private FamiliarSettings m_FamiliarSettings = new FamiliarSettings();

	public FamiliarSettings FamiliarSettings => m_FamiliarSettings;
}
