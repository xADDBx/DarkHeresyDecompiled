using System;
using System.Linq;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/Dialogue/DialogRoot")]
[TypeId("d431d511cbcf03e46a3d1c685ead2f08")]
public class DialogRoot : BlueprintScriptableObject
{
	[SerializeField]
	[FormerlySerializedAs("ExitAnswer")]
	private BlueprintAnswerReference m_ExitAnswer;

	[SerializeField]
	[FormerlySerializedAs("ContinueAnswer")]
	private BlueprintAnswerReference m_ContinueAnswer;

	[SerializeField]
	[FormerlySerializedAs("InterchapterExitAnswer")]
	private BlueprintAnswerReference m_InterchapterExitAnswer;

	[SerializeField]
	[FormerlySerializedAs("InterchapterContinueAnswer")]
	private BlueprintAnswerReference m_InterchapterContinueAnswer;

	[SerializeField]
	private BlueprintBarkBanterList.Reference[] m_DefaultBarkBanterListReferences;

	[FormerlySerializedAs("PCRange")]
	public float ListenerRange = 20f;

	public Sprite DefaultBookEventPicture;

	[SerializeField]
	private DialogCameraPositionOffsetEntry[] m_CameraOffsetBySize;

	[field: SerializeField]
	public Vector2 DialogCameraCorrection { get; private set; } = new Vector2(0.15f, 0f);


	public BlueprintAnswer ExitAnswer => m_ExitAnswer?.Get();

	public BlueprintAnswer ContinueAnswer => m_ContinueAnswer?.Get();

	public BlueprintAnswer InterchapterExitAnswer => m_InterchapterExitAnswer?.Get();

	public BlueprintAnswer InterchapterContinueAnswer => m_InterchapterContinueAnswer?.Get();

	public BlueprintBarkBanterList[] DefaultBlueprintBarkBanterLists => m_DefaultBarkBanterListReferences?.Dereference().ToArray() ?? Array.Empty<BlueprintBarkBanterList>();

	public float GetCameraOffsetBySize(Size size)
	{
		if (!m_CameraOffsetBySize.TryFind((DialogCameraPositionOffsetEntry entry) => entry.Size == size, out var result))
		{
			return 0f;
		}
		return result.HeightOffset;
	}
}
