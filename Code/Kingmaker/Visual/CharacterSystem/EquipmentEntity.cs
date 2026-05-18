using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.View.Visual.CharacterSystem;
using Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[KnowledgeDatabaseID("05125081574a9a44a83bc14d990e8f1a")]
public class EquipmentEntity : ScriptableObject, IEquipmentFeatureProvider, IResource
{
	public class PaintedTextures
	{
		private class PaintIndices
		{
			public int m_LastRepaintPrimary = -1;

			public int m_LastRepaintSecondary = -1;

			public int m_LastRepaintPrimarySpecial = -1;

			public int m_LastRepaintSecondarySpecial = -1;
		}

		private readonly Dictionary<EquipmentEntity, PaintIndices> m_PaintIndices = new Dictionary<EquipmentEntity, PaintIndices>();

		private readonly Dictionary<CharacterTextureDescription, RenderTexture> m_TexturesMap = new Dictionary<CharacterTextureDescription, RenderTexture>();

		public bool CheckNeedRepaint(EquipmentEntity ee, int primaryIndex, int secondaryIndex)
		{
			if (!m_PaintIndices.TryGetValue(ee, out var value))
			{
				value = new PaintIndices();
				m_PaintIndices.Add(ee, value);
			}
			bool result = value.m_LastRepaintPrimary != primaryIndex || value.m_LastRepaintSecondary != secondaryIndex;
			value.m_LastRepaintPrimary = primaryIndex;
			value.m_LastRepaintSecondary = secondaryIndex;
			return result;
		}

		public void Add(CharacterTextureDescription desc, RenderTexture rt)
		{
			m_TexturesMap[desc] = rt;
		}

		public RenderTexture Get(CharacterTextureDescription desc)
		{
			m_TexturesMap.TryGetValue(desc, out var value);
			return value;
		}

		public void RemoveEquipmentEntity(EquipmentEntity ee)
		{
			foreach (BodyPart bodyPart in ee.BodyParts)
			{
				RenderTexture value = null;
				if (m_TexturesMap.TryGetValue(bodyPart.Textures, out value) && value != null)
				{
					value.Release();
					UnityEngine.Object.DestroyImmediate(value);
				}
			}
			m_PaintIndices.Remove(ee);
		}

		public void Clear()
		{
			foreach (KeyValuePair<CharacterTextureDescription, RenderTexture> item in m_TexturesMap)
			{
				if (!(item.Value == null))
				{
					RenderTexture.ReleaseTemporary(item.Value);
				}
			}
			m_TexturesMap.Clear();
			m_PaintIndices.Clear();
		}
	}

	[KDB("Слой ЕЕ. Порядок отрисовки, верхние слои перекрывают нижние. При одинаковом слое отображается тот ЕЕ что надет последним")]
	[SerializeField]
	private int m_Layer;

	[KDB("Надо ли отрисовывать текстуры ЕЕ со слоями ниже")]
	[SerializeField]
	private bool m_KeepTexturesOfPrevLayers;

	[KDB("Материал. Текстуры ЕЕ сшиваются в алтасы по материалам")]
	[SerializeField]
	private Material m_Material;

	[KDB("При сборке персонажа со всех ЕЕ будет отрезан меш по выставленной вертексной маске")]
	[SerializeField]
	private List<VertexColorMask> m_HideByVertexColorMask;

	[KDB("Модификации костей скелета")]
	[SerializeField]
	private List<Skeleton.BoneModifier> m_SkeletonModifiers;

	[Obsolete]
	public Texture2D PreviewTexture;

	[Header("Color profile")]
	[KDB("Cписок цветов для перекраски. Mode - режим применения сопоставления цветности. Replace - cопоставить пиксели из бейз мапы с градиентом и заменить их, Multiply - cопоставить пиксели из бейз мапы с градиентом и умножить их")]
	[SerializeField]
	private CharacterColorsProfile m_PrimaryColorsProfile;

	[Space(2f)]
	[KDB("Второй список цветов для перекраски. Персы у нас умеют краситься в 2 цвета одновременно. Mode - режим применения сопоставления цветности. Replace - cопоставить пиксели из бейз мапы с градиентом и заменить их, Multiply - cопоставить пиксели из бейз мапы с градиентом и умножить их")]
	[SerializeField]
	private CharacterColorsProfile m_SecondaryColorsProfile;

	[Space(5f)]
	[KDB("Разрешенные комбинации цветов для этого ЕЕ")]
	[SerializeField]
	private RampColorPreset m_ColorPresets;

	[Space(5f)]
	[KDB("Части из которых состоит ЕЕ и впоследствии будет собран персонаж. Может быть и мешом и просто списком текстур")]
	[SerializeField]
	private List<BodyPart> m_BodyParts = new List<BodyPart>();

	[KDB("Внешняя геометрия на персонаже. Не участвует в сборке персонажа и не является частью основного меша")]
	[SerializeField]
	private List<OutfitPart> m_OutfitParts = new List<OutfitPart>();

	[KDB("Геймплейные особенности предмета")]
	[SerializeField]
	private BlueprintEquipmentFeatureReference[] m_Features;

	private IEquipmentFeatureProvider m_EquipmentFeatureProvider;

	private bool m_IsDirty;

	[HideInInspector]
	public bool IsExportEnabled;

	private IEquipmentFeatureProvider Features => m_EquipmentFeatureProvider ?? (m_EquipmentFeatureProvider = new EquipmentFeatureProvider(m_Features));

	public int Layer => m_Layer;

	public bool KeepTexturesOfPrevLayers => m_KeepTexturesOfPrevLayers;

	public Material Material => m_Material;

	public List<VertexColorMask> HideByVertexColorMask => m_HideByVertexColorMask;

	public List<Skeleton.BoneModifier> SkeletonModifiers => m_SkeletonModifiers;

	public CharacterColorsProfile PrimaryColorsProfile => m_PrimaryColorsProfile;

	public CharacterColorsProfile SecondaryColorsProfile => m_SecondaryColorsProfile;

	public bool HasPrimaryRamps
	{
		get
		{
			if (PrimaryColorsProfile != null)
			{
				return PrimaryColorsProfile.Ramps.Count > 0;
			}
			return false;
		}
	}

	public bool HasSecondaryRamps
	{
		get
		{
			if (SecondaryColorsProfile != null)
			{
				return SecondaryColorsProfile.Ramps.Count > 0;
			}
			return false;
		}
	}

	public RampColorPreset ColorPresets => m_ColorPresets;

	public List<BodyPart> BodyParts => m_BodyParts;

	public List<OutfitPart> OutfitParts => m_OutfitParts;

	public void RepaintTextures(PaintedTextures paintedTextures, int primaryRampIndex, int secondaryRampIndex)
	{
		if (!paintedTextures.CheckNeedRepaint(this, primaryRampIndex, secondaryRampIndex))
		{
			return;
		}
		Texture2D item = ((PrimaryColorsProfile != null && PrimaryColorsProfile.Ramps.IsValidIndex(primaryRampIndex)) ? PrimaryColorsProfile.Ramps[primaryRampIndex] : null);
		Texture2D item2 = ((SecondaryColorsProfile != null && SecondaryColorsProfile.Ramps.IsValidIndex(secondaryRampIndex)) ? SecondaryColorsProfile.Ramps[secondaryRampIndex] : null);
		foreach (BodyPart bodyPart in BodyParts)
		{
			CharacterTextureDescription textures = bodyPart.Textures;
			RenderTexture rtToPaint = paintedTextures.Get(textures);
			textures.Repaint(ref rtToPaint, (primaryRamp: item, primaryProfile: m_PrimaryColorsProfile), (secondaryRamp: item2, secondaryProfile: m_SecondaryColorsProfile));
			paintedTextures.Add(textures, rtToPaint);
		}
	}

	public bool HasFeature(EquipmentFeatureFlag feature)
	{
		if (Features.HasFeature(feature))
		{
			return true;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			if (bodyPart.HasFeature(feature))
			{
				return true;
			}
		}
		foreach (OutfitPart outfitPart in OutfitParts)
		{
			if (outfitPart.HasFeature(feature))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetPhysicsDeformerLayout(out TriangleSkinmap triangleSkinmap, out GameObject prefabMesh)
	{
		return Features.TryGetPhysicsDeformerLayout(out triangleSkinmap, out prefabMesh);
	}

	public bool IsHiddenByVisibilityFeatures(CharacterDisplayOptions displayOptions)
	{
		return Features.IsHiddenByVisibilityFeatures(displayOptions);
	}

	public IEnumerable<T> GetFeatureComponents<T>()
	{
		if (m_Features == null)
		{
			yield break;
		}
		BlueprintEquipmentFeatureReference[] features = m_Features;
		foreach (BlueprintEquipmentFeatureReference blueprintEquipmentFeatureReference in features)
		{
			foreach (T component in blueprintEquipmentFeatureReference.Blueprint.GetComponents<T>())
			{
				yield return component;
			}
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			foreach (T featureComponent in bodyPart.GetFeatureComponents<T>())
			{
				yield return featureComponent;
			}
		}
		foreach (OutfitPart outfitPart in OutfitParts)
		{
			foreach (T featureComponent2 in outfitPart.GetFeatureComponents<T>())
			{
				yield return featureComponent2;
			}
		}
	}
}
