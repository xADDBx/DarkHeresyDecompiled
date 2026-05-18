using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[NonOverridable]
[TypeId("e29428d274948784bb31643317d419ba")]
public abstract class BlueprintAnswerBase : BlueprintScriptableObject, IConditionDebugContext, IEditorCommentHolder
{
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

	public void AddConditionDebugMessage(object element, bool result, string messageFormat, object[] @params)
	{
		DialogDebug.AddCondition(this, result, messageFormat, @params);
	}
}
