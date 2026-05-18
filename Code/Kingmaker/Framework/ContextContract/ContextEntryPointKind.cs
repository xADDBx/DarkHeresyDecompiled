namespace Kingmaker.Framework.ContextContract;

public enum ContextEntryPointKind : byte
{
	AbilityCastRestriction,
	AbilityOnCast,
	AbilityTargetValidation,
	AbilityDelivery,
	AbilityPatternTargetSelection,
	AbilityLoSRangeException,
	AbilityApplyEffect,
	AbilityHaloOuter,
	AbilityHaloInner,
	AbilityAdditionalTargets,
	AbilityCustomLogicCleanup,
	BuffComponent,
	BuffComponentLifecycle,
	BuffComponentRulebookHandler,
	FeatureComponentLifecycle,
	FeatureComponentRulebookHandler
}
