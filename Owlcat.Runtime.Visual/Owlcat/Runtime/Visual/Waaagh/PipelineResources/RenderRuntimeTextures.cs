using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PipelineResources;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "R: Waaagh Render Runtime Textures", Order = 1000)]
public class RenderRuntimeTextures : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	[SerializeField]
	[ResourcePaths(new string[] { "Runtime/Waaagh/Data/Textures/BlueNoise256/LDR_LLL1_0.png", "Runtime/Waaagh/Data/Textures/BlueNoise256/LDR_LLL1_1.png", "Runtime/Waaagh/Data/Textures/BlueNoise256/LDR_LLL1_2.png", "Runtime/Waaagh/Data/Textures/BlueNoise256/LDR_LLL1_3.png", "Runtime/Waaagh/Data/Textures/BlueNoise256/LDR_LLL1_4.png", "Runtime/Waaagh/Data/Textures/BlueNoise256/LDR_LLL1_5.png", "Runtime/Waaagh/Data/Textures/BlueNoise256/LDR_LLL1_6.png", "Runtime/Waaagh/Data/Textures/BlueNoise256/LDR_LLL1_7.png" }, SearchType.ProjectPath)]
	private Texture2D[] m_BlueNoise256Textures;

	[SerializeField]
	[ResourcePaths(new string[]
	{
		"Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_0.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_1.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_2.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_3.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_4.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_5.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_6.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_7.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_8.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_9.png",
		"Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_10.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_11.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_12.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_13.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_14.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_15.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_16.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_17.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_18.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_19.png",
		"Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_20.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_21.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_22.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_23.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_24.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_25.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_26.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_27.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_28.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_29.png",
		"Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_30.png", "Runtime/Waaagh/Data/Textures/BlueNoise16/L/LDR_LLL1_31.png"
	}, SearchType.ProjectPath)]
	private Texture2D[] m_BlueNoise16Textures;

	[SerializeField]
	[ResourcePaths(new string[] { "Runtime/Waaagh/Data/Textures/BlueNoise64/L/LDR_LLL1_0.png", "Runtime/Waaagh/Data/Textures/BlueNoise64/L/LDR_LLL1_1.png", "Runtime/Waaagh/Data/Textures/BlueNoise64/L/LDR_LLL1_2.png", "Runtime/Waaagh/Data/Textures/BlueNoise64/L/LDR_LLL1_3.png", "Runtime/Waaagh/Data/Textures/BlueNoise64/L/LDR_LLL1_4.png", "Runtime/Waaagh/Data/Textures/BlueNoise64/L/LDR_LLL1_5.png", "Runtime/Waaagh/Data/Textures/BlueNoise64/L/LDR_LLL1_6.png", "Runtime/Waaagh/Data/Textures/BlueNoise64/L/LDR_LLL1_7.png" }, SearchType.ProjectPath)]
	private Texture2D[] m_BlueNoise64Textures;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Data/Textures/CoherentNoise/OwenScrambledNoise256.png", SearchType.ProjectPath)]
	private Texture2D m_OwenScrambled256Tex;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Data/Textures/CoherentNoise/RankingTile1SPP.png", SearchType.ProjectPath)]
	private Texture2D m_RankingTile1SPP;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Data/Textures/CoherentNoise/ScramblingTile1SPP.png", SearchType.ProjectPath)]
	private Texture2D m_ScramblingTile1SPP;

	[SerializeField]
	[ResourcePaths(new string[] { "Runtime/Waaagh/Data/Textures/FilmGrain/Thin01.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Thin02.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Medium01.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Medium02.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Medium03.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Medium04.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Medium05.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Medium06.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Large01.png", "Runtime/Waaagh/Data/Textures/FilmGrain/Large02.png" }, SearchType.ProjectPath)]
	private Texture2D[] m_FilmGrainTex;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Data/Textures/SMAA/AreaTex.tga", SearchType.ProjectPath)]
	private Texture2D m_SmaaAreaTex;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Data/Textures/SMAA/SearchTex.tga", SearchType.ProjectPath)]
	private Texture2D m_SmaaSearchTex;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Data/Textures/BayerMatrix.png", SearchType.ProjectPath)]
	private Texture2D m_BayerMatrixTex;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Data/Textures/DebugFont.tga", SearchType.ProjectPath)]
	private Texture2D m_DebugFontTex;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/Data/Textures/Makeev/TonyMcMapfaceLUT.png", SearchType.ProjectPath)]
	private Texture3D m_MakeevLut;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public Texture2D[] BlueNoise256Textures
	{
		get
		{
			return m_BlueNoise256Textures;
		}
		set
		{
			this.SetValueAndNotify(ref m_BlueNoise256Textures, value, "BlueNoise256Textures");
		}
	}

	public Texture2D[] BlueNoise16Textures
	{
		get
		{
			return m_BlueNoise16Textures;
		}
		set
		{
			this.SetValueAndNotify(ref m_BlueNoise16Textures, value, "BlueNoise16Textures");
		}
	}

	public Texture2D[] BlueNoise64Textures
	{
		get
		{
			return m_BlueNoise64Textures;
		}
		set
		{
			this.SetValueAndNotify(ref m_BlueNoise64Textures, value, "BlueNoise64Textures");
		}
	}

	public Texture2D OwenScrambled256Tex
	{
		get
		{
			return m_OwenScrambled256Tex;
		}
		set
		{
			this.SetValueAndNotify(ref m_OwenScrambled256Tex, value, "OwenScrambled256Tex");
		}
	}

	public Texture2D RankingTile1SPP
	{
		get
		{
			return m_RankingTile1SPP;
		}
		set
		{
			this.SetValueAndNotify(ref m_RankingTile1SPP, value, "RankingTile1SPP");
		}
	}

	public Texture2D ScramblingTile1SPP
	{
		get
		{
			return m_ScramblingTile1SPP;
		}
		set
		{
			this.SetValueAndNotify(ref m_ScramblingTile1SPP, value, "ScramblingTile1SPP");
		}
	}

	public Texture2D[] FilmGrainTex
	{
		get
		{
			return m_FilmGrainTex;
		}
		set
		{
			this.SetValueAndNotify(ref m_FilmGrainTex, value, "FilmGrainTex");
		}
	}

	public Texture2D SmaaAreaTex
	{
		get
		{
			return m_SmaaAreaTex;
		}
		set
		{
			this.SetValueAndNotify(ref m_SmaaAreaTex, value, "SmaaAreaTex");
		}
	}

	public Texture2D SmaaSearchTex
	{
		get
		{
			return m_SmaaSearchTex;
		}
		set
		{
			this.SetValueAndNotify(ref m_SmaaSearchTex, value, "SmaaSearchTex");
		}
	}

	public Texture2D bayerMatrixTex
	{
		get
		{
			return m_BayerMatrixTex;
		}
		set
		{
			this.SetValueAndNotify(ref m_BayerMatrixTex, value, "m_BayerMatrixTex");
		}
	}

	public Texture2D debugFontTexture
	{
		get
		{
			return m_DebugFontTex;
		}
		set
		{
			this.SetValueAndNotify(ref m_DebugFontTex, value, "m_DebugFontTex");
		}
	}

	public Texture3D MakeevLut
	{
		get
		{
			return m_MakeevLut;
		}
		set
		{
			this.SetValueAndNotify(ref m_MakeevLut, value, "m_MakeevLut");
		}
	}
}
