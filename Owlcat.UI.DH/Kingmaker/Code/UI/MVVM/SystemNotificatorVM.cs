using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SystemNotificatorVM : ViewModel, IWarningNotificationUIHandler, ISubscriber
{
	private readonly ReactiveProperty<SystemNotificationVM> m_CurrentNotification = new ReactiveProperty<SystemNotificationVM>();

	public ReadOnlyReactiveProperty<SystemNotificationVM> CurrentNotification => m_CurrentNotification;

	public SystemNotificatorVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleWarning(WarningNotificationType type, bool addToLog, WarningNotificationFormat? format, bool withSound = true)
	{
		WarningNotificationFormat valueOrDefault = format.GetValueOrDefault();
		if (!format.HasValue)
		{
			valueOrDefault = GetFormatByType(type);
			format = valueOrDefault;
		}
		m_CurrentNotification.Value = new SystemNotificationVM(type, withSound, format.Value);
	}

	public void HandleWarning(string str, bool addToLog, WarningNotificationFormat format, bool withSound = true)
	{
		m_CurrentNotification.Value = new SystemNotificationVM(str, withSound, format);
	}

	private WarningNotificationFormat GetFormatByType(WarningNotificationType type)
	{
		return type switch
		{
			WarningNotificationType.SavingImpossible => WarningNotificationFormat.Warning, 
			WarningNotificationType.SavingImpossibleIronman => WarningNotificationFormat.Warning, 
			WarningNotificationType.EquipInCombatIsImpossible => WarningNotificationFormat.Warning, 
			WarningNotificationType.EquipInLevlUpIsImpossible => WarningNotificationFormat.Warning, 
			WarningNotificationType.SavingError => WarningNotificationFormat.Warning, 
			WarningNotificationType.NoQuickSaves => WarningNotificationFormat.Warning, 
			WarningNotificationType.SavingFailed => WarningNotificationFormat.Warning, 
			_ => WarningNotificationFormat.Common, 
		};
	}
}
