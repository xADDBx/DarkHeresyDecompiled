namespace Kingmaker.Visual.Particles;

public class WeaponParticlesSnapController : SnapControllerBase
{
	private WeaponParticlesSnapMap m_WeaponMap;

	protected override void OnDisable()
	{
		base.OnDisable();
		m_WeaponMap = null;
	}

	protected override void OnStartPlaying(SnapMapBase snapMap)
	{
		base.OnStartPlaying(snapMap);
		m_WeaponMap = snapMap as WeaponParticlesSnapMap;
	}
}
