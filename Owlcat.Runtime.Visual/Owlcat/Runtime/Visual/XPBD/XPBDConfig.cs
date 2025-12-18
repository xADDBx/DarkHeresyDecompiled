using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD;

[CreateAssetMenu(fileName = "XPBDConfig", menuName = "XPBD/Config")]
public class XPBDConfig : ScriptableObject
{
	internal const string kFileName = "XPBDConfig";

	private static XPBDConfig s_Instance;

	public SimulationSettings SimulationSettings;

	public CollisionSettings CollisionSettings;

	public EditorSettings EditorSettings;

	public XPBDDebug DebugSettings => WaaaghPipeline.Asset.DebugData.XPBDDebug;

	public static XPBDConfig Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = Resources.Load<XPBDConfig>("XPBDConfig");
			}
			return s_Instance;
		}
	}
}
