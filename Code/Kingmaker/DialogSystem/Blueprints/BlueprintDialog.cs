using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Enums.Stats;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Localization;
using MemoryPack;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[NonOverridable]
[TypeId("c8ff73feae580b142a9f43e0c61d7f32")]
[OwlPackable(OwlPackableMode.NoGenerate)]
[MemoryPackable(GenerateType.NoGenerate)]
public class BlueprintDialog : BlueprintScriptableObject, IConditionDebugContext, IEditorCommentHolder
{
	[LocalizedStringParam(Kind = "meta", Group = LocalizedStringGroup.Voice_Comments)]
	public LocalizedString VoComment;

	public CueSelection FirstCue;

	[CanBeNull]
	[SerializeReference]
	public PositionEvaluator StartPosition;

	public ConditionsChecker Conditions = new ConditionsChecker();

	public ActionList StartActions = new ActionList();

	public ActionList FinishActions = new ActionList();

	public ActionList ReplaceActions = new ActionList();

	[Tooltip("Если галка стоит, то любое изменение диалога будет проверяться хуком CheckDialogFeatureFreezed")]
	public bool FeatureFreezed;

	public bool TurnPlayer = true;

	public bool TurnFirstSpeaker = true;

	[Tooltip("Отключает вращение камеры игроком на время диалога")]
	public bool IsLockCameraRotationButtons;

	public bool IsNarratorText;

	public DialogType Type;

	public LocalizedString Description;

	[InfoBox("Этот блок настроек - meta информация для статистики")]
	public Chapter Chapter;

	public Cluster Cluster;

	[HideInInspector]
	[SerializeField]
	private EditorCommentHolder m_EditorComment;

	public EditorCommentHolder EditorComment
	{
		get
		{
			return m_EditorComment;
		}
		set
		{
			m_EditorComment = value;
		}
	}

	[NotNull]
	public BlueprintAnswer GetContinueAnswer()
	{
		if (Type == DialogType.Epilog)
		{
			return ConfigRoot.Instance.Dialog.InterchapterContinueAnswer;
		}
		return ConfigRoot.Instance.Dialog.ContinueAnswer;
	}

	[NotNull]
	public BlueprintAnswer GetExitAnswer()
	{
		if (Type == DialogType.Epilog)
		{
			return ConfigRoot.Instance.Dialog.InterchapterExitAnswer;
		}
		return ConfigRoot.Instance.Dialog.ExitAnswer;
	}

	public void AddConditionDebugMessage(object element, bool result, string messageFormat, object[] @params)
	{
		DialogDebug.AddCondition(this, result, messageFormat, @params);
	}
}
