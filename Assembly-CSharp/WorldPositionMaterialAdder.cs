using System.Collections.Generic;
using UnityEngine;

public class WorldPositionMaterialAdder : MonoBehaviour
{
	[Tooltip("List of GameObjects whose hierarchy will be scanned for renderers")]
	public List<GameObject> targetObjects;

	[Tooltip("Material that will be added to each renderer")]
	public Material addedMaterial;

	[Tooltip("Shader variable name to which this object's world position will be assigned")]
	public string shaderVariableName = "_WorldPosPoint";

	private void Start()
	{
		if (addedMaterial == null)
		{
			Debug.LogError("No material assigned to be added.");
			return;
		}
		Vector3 position = base.transform.position;
		foreach (GameObject targetObject in targetObjects)
		{
			if (targetObject == null)
			{
				continue;
			}
			Renderer[] componentsInChildren = targetObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (!addedMaterial.HasVector(shaderVariableName))
				{
					Debug.LogWarning("Material '" + addedMaterial.name + "' does not contain the variable " + shaderVariableName);
					continue;
				}
				Material material = new Material(addedMaterial);
				material.SetVector(shaderVariableName, position);
				Material[] sharedMaterials = renderer.sharedMaterials;
				Material[] array = new Material[sharedMaterials.Length + 1];
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					array[j] = sharedMaterials[j];
				}
				array[sharedMaterials.Length] = material;
				renderer.materials = array;
			}
		}
	}
}
