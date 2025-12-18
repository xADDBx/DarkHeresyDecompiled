using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

[AllowedOn(typeof(BlueprintEquipmentFeature))]
[TypeId("03787cb0432e41af8062d788ac102665")]
public class EquipmentPhysicsFeatureComponent : BlueprintComponent
{
	[SerializeField]
	public TriangleSkinmap TriangleSkinmap;

	[SerializeField]
	public GameObject MasterPrefab;
}
