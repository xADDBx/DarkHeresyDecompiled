using Kingmaker.Blueprints.Attributes;
using Kingmaker.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.SceneManagement;

namespace Kingmaker.Blueprints.Area;

[ComponentName("Area/BlueprintAreaMechanics")]
[TypeId("c542bb267f6d4651af99d4c5b3a0df9a")]
public class BlueprintAreaMechanics : BlueprintScriptableObject
{
	public BlueprintAreaReference Area;

	public bool ContainsNavMesh;

	public SceneReference Scene;

	public AkBankReference AdditionalDataBank;

	public bool IsSceneLoadedNow()
	{
		return SceneManager.GetSceneByName(Scene.SceneName).isLoaded;
	}
}
