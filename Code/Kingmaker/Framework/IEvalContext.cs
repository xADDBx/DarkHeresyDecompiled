using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Framework.Utility;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Framework;

public interface IEvalContext
{
	BlueprintScriptableObject? Blueprint { get; }

	MechanicEntityFact? Fact { get; }

	AbilityData? Ability { get; }

	MechanicEntity? Caster { get; }

	MechanicEntity? Owner { get; }

	TargetWrapper? ClickedTarget { get; }

	MechanicEntity? CurrentEntity { get; }

	OrientedPatternData Pattern { get; }

	Vector3 Direction { get; }

	TargetWrapper? Target { get; }

	RulebookEvent? Rule { get; }

	AreaEffectEntity? AreaEffect { get; }

	AbilityExecutionContext? AbilityExecution { get; }

	ItemEntityWeapon? AbilityWeapon { get; }

	BlueprintAbility? AbilityBlueprint { get; }

	MechanicEntity? RuleInitiator { get; }

	MechanicEntity? RuleTarget { get; }

	MechanicEntity? CurrentTargetEntity { get; }

	TargetWrapper? ContextMainTarget { get; }

	Vector3 ContextMainTargetPosition { get; }

	MechanicEntity? ContextOwner { get; }

	MechanicEntity? ContextCaster { get; }

	bool DisableFx { get; }

	LosDescription LosToClickedTarget { get; }

	NodeList SourcePatternNodes { get; }

	MechanicEntity? SourceCaster { get; }

	AbilityData? SourceAbility { get; }

	BlueprintAbility? SourceAbilityBlueprint { get; }

	BlueprintScriptableObject? SourceBlueprint { get; }

	TargetWrapper? SourceClickedTarget { get; }

	Vector3? SourceCastPosition { get; }

	MechanicEntityFact? SourceFact { get; }

	int this[string key] { get; set; }

	int this[ContextPropertyName key] { get; set; }

	int[]? PropertiesArray { get; }

	MechanicEntity? GetEntityByType(PropertyTargetType type);

	Vector3 GetEntityPositionByType(PropertyTargetType type);

	IntRect GetEntityRectByType(PropertyTargetType type);

	T? Get<T>() where T : ContextData<T>, new();

	ScopedStackFrame PushTarget(TargetWrapper target);

	ScopedStackFrame PushRule(RulebookEvent rule);

	ScopedStackFrame PushCurrentEntity(MechanicEntity entity);

	ScopedStackFrame PushAreaEffect(AreaEffectEntity ae);

	ScopedStackFrame PushSourceAbility(AbilityData ability);
}
