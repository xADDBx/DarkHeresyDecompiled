using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.ResourceManagement;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class PortraitData : IHashable
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_CustomPortraitId;

	[SerializeField]
	[UsedImplicitly]
	[ValidateNotNull]
	public SpriteLink m_PortraitImage;

	[SerializeField]
	[UsedImplicitly]
	[ValidateNotNull]
	public SpriteLink m_HalfLengthImage;

	[SerializeField]
	[UsedImplicitly]
	public SpriteLink m_FullLengthImage;

	[SerializeField]
	[UsedImplicitly]
	public SpriteLink m_LoadingImage;

	[SerializeField]
	[UsedImplicitly]
	public SpriteLink m_LoadingGlitchImage;

	public SpriteLink InterchapterPortraitLink;

	public PortraitCategory PortraitCategory;

	public bool IsDefault;

	public bool InitiativePortrait;

	public bool FlipFullLengthPortraitInDialog;

	private static int[] s_DefaultPortraitsHashes;

	private string CustomPortraitSmallPath => CustomPortraitsManager.Instance.GetSmallPortraitPath(m_CustomPortraitId);

	private string CustomPortraitMediumPath => CustomPortraitsManager.Instance.GetMediumPortraitPath(m_CustomPortraitId);

	private string CustomPortraitBigPath => CustomPortraitsManager.Instance.GetBigPortraitPath(m_CustomPortraitId);

	public bool IsCustom => !string.IsNullOrEmpty(m_CustomPortraitId);

	public string CustomId => m_CustomPortraitId;

	public bool HasPortrait
	{
		get
		{
			if (SmallPortrait != null && HalfLengthPortrait != null)
			{
				return FullLengthPortrait != null;
			}
			return false;
		}
	}

	public CustomPortraitHandle SmallPortraitHandle { get; private set; }

	public CustomPortraitHandle HalfPortraitHandle { get; private set; }

	public CustomPortraitHandle FullPortraitHandle { get; private set; }

	public Sprite SmallPortrait
	{
		get
		{
			if (IsCustom)
			{
				if (SmallPortraitHandle == null)
				{
					return ConfigRoot.Instance.CharGenRoot.BasePortraitSmall.Load();
				}
				if (!(SmallPortraitHandle.Sprite != null))
				{
					return ConfigRoot.Instance.CharGenRoot.BasePortraitSmall.Load();
				}
				return SmallPortraitHandle.Sprite;
			}
			return m_PortraitImage?.Load(ignorePreloadWarning: true);
		}
	}

	public Sprite HalfLengthPortrait
	{
		get
		{
			if (IsCustom)
			{
				if (HalfPortraitHandle == null)
				{
					return ConfigRoot.Instance.CharGenRoot.BasePortraitMedium.Load();
				}
				if (!(HalfPortraitHandle.Sprite != null))
				{
					return ConfigRoot.Instance.CharGenRoot.BasePortraitMedium.Load();
				}
				return HalfPortraitHandle.Sprite;
			}
			return m_HalfLengthImage.Load(ignorePreloadWarning: true);
		}
	}

	public Sprite FullLengthPortrait
	{
		get
		{
			if (IsCustom)
			{
				if (FullPortraitHandle == null)
				{
					return ConfigRoot.Instance.CharGenRoot.BasePortraitBig.Load();
				}
				if (!(FullPortraitHandle?.Sprite != null))
				{
					return ConfigRoot.Instance.CharGenRoot.BasePortraitBig.Load();
				}
				return FullPortraitHandle?.Sprite;
			}
			return m_FullLengthImage.Load(ignorePreloadWarning: true);
		}
	}

	public Sprite LoadingPortrait
	{
		get
		{
			SpriteLink loadingImage = m_LoadingImage;
			if ((object)loadingImage != null && loadingImage.Exists())
			{
				return m_LoadingImage.Load();
			}
			return null;
		}
	}

	public Sprite LoadingGlitchPortrait
	{
		get
		{
			SpriteLink loadingGlitchImage = m_LoadingGlitchImage;
			if ((object)loadingGlitchImage != null && loadingGlitchImage.Exists())
			{
				return m_LoadingGlitchImage.Load();
			}
			return null;
		}
	}

	[JsonConstructor]
	public PortraitData(string m_customPortraitId)
	{
		m_CustomPortraitId = m_customPortraitId;
		if (IsCustom)
		{
			InitHandles();
		}
	}

	public PortraitData()
	{
		if (IsCustom)
		{
			InitHandles();
		}
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext context)
	{
		if (IsCustom)
		{
			InitHandles();
		}
	}

	public Sprite GetInterChapterPortrait()
	{
		SpriteLink interchapterPortraitLink = InterchapterPortraitLink;
		if ((object)interchapterPortraitLink != null && interchapterPortraitLink.Exists())
		{
			return InterchapterPortraitLink.Load();
		}
		return null;
	}

	public bool DirectoryExists()
	{
		if (!IsCustom)
		{
			return true;
		}
		return CustomPortraitsManager.Instance.EnsureDirectory(m_CustomPortraitId, createNewIfNotExists: false);
	}

	public bool EnsureImages()
	{
		if (!IsCustom)
		{
			return false;
		}
		if (!CustomPortraitsManager.Instance.EnsureDirectory(m_CustomPortraitId, createNewIfNotExists: false))
		{
			return false;
		}
		return (bool)SmallPortraitHandle.Load() & (bool)HalfPortraitHandle.Load() & (bool)FullPortraitHandle.Load();
	}

	public void CheckIfDefaultPortraitData()
	{
		if (TextureIsDefaultPortrait(SmallPortrait.texture, PortraitType.SmallPortrait))
		{
			IsDefault = true;
		}
		else if (TextureIsDefaultPortrait(HalfLengthPortrait.texture, PortraitType.HalfLengthPortrait))
		{
			IsDefault = true;
		}
		else if (TextureIsDefaultPortrait(FullLengthPortrait.texture, PortraitType.FullLengthPortrait))
		{
			IsDefault = true;
		}
		else
		{
			IsDefault = false;
		}
	}

	private void InitHandles()
	{
		SmallPortraitHandle = new CustomPortraitHandle(CustomPortraitSmallPath, PortraitType.SmallPortrait, CustomPortraitsManager.Instance.Storage);
		HalfPortraitHandle = new CustomPortraitHandle(CustomPortraitMediumPath, PortraitType.HalfLengthPortrait, CustomPortraitsManager.Instance.Storage);
		FullPortraitHandle = new CustomPortraitHandle(CustomPortraitBigPath, PortraitType.FullLengthPortrait, CustomPortraitsManager.Instance.Storage);
	}

	private bool TextureIsDefaultPortrait(Texture2D texture, PortraitType portraitType)
	{
		if (s_DefaultPortraitsHashes == null || s_DefaultPortraitsHashes.Length == 0)
		{
			s_DefaultPortraitsHashes = new int[3];
			s_DefaultPortraitsHashes[0] = GetPseudoHash(ConfigRoot.Instance.CharGenRoot.BasePortraitSmall.Load()?.texture);
			s_DefaultPortraitsHashes[1] = GetPseudoHash(ConfigRoot.Instance.CharGenRoot.BasePortraitMedium.Load()?.texture);
			s_DefaultPortraitsHashes[2] = GetPseudoHash(ConfigRoot.Instance.CharGenRoot.BasePortraitBig.Load()?.texture);
		}
		return GetPseudoHash(texture) == s_DefaultPortraitsHashes[(int)portraitType];
	}

	private int GetPseudoHash(Texture2D texture)
	{
		int num = 100;
		int num2 = texture.width * texture.height;
		int num3 = num2 / num;
		int num4 = -2128831035;
		for (int i = 0; i < num2 - 1; i += num3)
		{
			int x = i / texture.width;
			int y = i % texture.width;
			Color pixel = texture.GetPixel(x, y);
			num4 = (num4 ^ (int)pixel.r) * 16777619;
			num4 = (num4 ^ (int)pixel.g) * 16777619;
			num4 = (num4 ^ (int)pixel.b) * 16777619;
		}
		num4 += num4 << 13;
		num4 ^= num4 >> 7;
		num4 += num4 << 3;
		num4 ^= num4 >> 17;
		return num4 + (num4 << 5);
	}

	public void Preload()
	{
		SpriteLink portraitImage = m_PortraitImage;
		if ((object)portraitImage != null && portraitImage.Exists())
		{
			m_PortraitImage.Preload();
		}
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(m_CustomPortraitId);
		return result;
	}
}
