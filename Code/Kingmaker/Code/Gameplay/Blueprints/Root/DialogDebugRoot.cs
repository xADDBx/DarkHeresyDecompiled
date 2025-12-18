using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.Dialog;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Blueprints.Root;

[Serializable]
[ComponentName("Root/Dialogue/DialogDebugRoot")]
[TypeId("4e4234bc142d4acc81469dc2ada73f33")]
public class DialogDebugRoot : BlueprintScriptableObject
{
	[Serializable]
	public class ForcedConditionsEntry
	{
		public BlueprintScriptableObjectReference Blueprint;

		public ForcedConditionsState State;
	}

	[Serializable]
	public class Reference : BlueprintReference<MoraleRoot>
	{
	}

	public class DialogDebugReference : BlueprintReference<DialogDebugRoot>
	{
		public DialogDebugReference()
		{
			guid = "6e39882fd2224c859127dcc94789bc47";
		}
	}

	[SerializeField]
	private List<ForcedConditionsEntry> m_ForcedConditionNodes = new List<ForcedConditionsEntry>();

	private static readonly DialogDebugReference s_Instance = new DialogDebugReference();

	private const string m_Guid = "6e39882fd2224c859127dcc94789bc47";

	public static DialogDebugRoot Instance => s_Instance;

	public void SetForcedCondition(BlueprintScriptableObject bp, ForcedConditionsState state)
	{
	}

	public ForcedConditionsState GetForcedCondition(BlueprintScriptableObject bp)
	{
		return ForcedConditionsState.NotForced;
	}

	private void ClearNullEntries()
	{
	}

	public void ClearForcedConditions()
	{
		m_ForcedConditionNodes.Clear();
	}
}
