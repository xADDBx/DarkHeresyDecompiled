using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

public interface IEquipmentFeatureProvider
{
	bool HasFeature(EquipmentFeatureFlag feature);

	bool TryGetPhysicsDeformerLayout(out TriangleSkinmap triangleSkinmap, out GameObject prefabMesh);

	bool IsHiddenByVisibilityFeatures(CharacterDisplayOptions displayOptions);

	IEnumerable<T> GetFeatureComponents<T>();
}
