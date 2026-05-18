using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Tutorial;

[AllowedOn(typeof(BlueprintTutorial))]
[ClassInfoBox("`t|SourceUnit`, `t|TargetUnit`, `t|SourceAbility`, `t|SourceItem`, `t|SourceItemOrAbility`")]
[TypeId("64ef042cd0d140a99b1d2de430e653e6")]
public abstract class TutorialTrigger : EntityFactComponentDelegate<TutorialSystem>
{
	[SerializeField]
	private bool _overrideDelay;

	[ShowIf("_overrideDelay")]
	[SerializeField]
	private float _delaySecondsOverride;

	public new Tutorial Fact => (Tutorial)base.Fact;

	private bool IsConsole => Game.Instance.IsControllerGamepad;

	public virtual bool RevealTargetUnitInfo => false;

	public bool OverrideDelay => _overrideDelay;

	public float DelaySecondsOverride => _delaySecondsOverride;

	protected virtual void SetupContext(TutorialContext context, [NotNull] RulebookEvent rule)
	{
		try
		{
			context.SourceUnit = rule.Initiator as BaseUnitEntity;
			context.TargetUnit = rule.Target as BaseUnitEntity;
			context.SourceAbility = rule.Reason.Ability;
			context.SourceFact = rule.Reason.Fact;
			context.SourceItem = rule.Reason.Item;
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, null);
		}
	}

	protected void TryToTrigger([CanBeNull] RulebookEvent rule, Action<TutorialContext> setupContext = null)
	{
		using TutorialContext tutorialContext = ContextData<TutorialContext>.Request();
		string text = ((!Fact.IsEnabled || base.Owner.HasCooldown || base.Owner.HasCandidateForShow) ? "red" : ((Fact.LastShowIndex > 0) ? "yellow" : "green"));
		PFLog.Tutorial.Log($"<color={text}>Trying to trigger {GetType().Name}.</color> Tutorial.IsEnabled = {Fact.IsEnabled}, HasCooldown = {base.Owner.HasCooldown}, LastShowIndex = {Fact.LastShowIndex}, HasCandidateForShow = {base.Owner.HasCandidateForShow}");
		if (rule != null)
		{
			SetupContext(tutorialContext, rule);
		}
		setupContext?.Invoke(tutorialContext);
		if (base.Owner.OnTryToTrigger(Fact, tutorialContext) && (Fact.Blueprint.VisibilitySetting != UISettingsEntityBase.UISettingsPlatform.Console || IsConsole) && (Fact.Blueprint.VisibilitySetting != UISettingsEntityBase.UISettingsPlatform.PC || !IsConsole))
		{
			base.Owner.Trigger(Fact.Blueprint, this);
		}
	}
}
