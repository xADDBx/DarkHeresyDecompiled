using UnityEngine.UIElements;

namespace Code.Framework.Editor.Cutscenes.Serialization;

public interface ICutsceneEditorNodeContent
{
	VisualElement DrawContent(string nodeId, CutsceneLayoutMetadata layoutMetadata);

	string GetContentId();

	void TrySelect();
}
