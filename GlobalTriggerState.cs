using System;
using System.Collections.Generic;

public static class GlobalTriggerState {
	public static Action<Trigger, bool>? TriggerChanged;
	private static Dictionary<string, bool> _triggerStates = new();

	public static void Reset() {
		_triggerStates = new Dictionary<string, bool>();
	}

	public static void SetTriggerState(Trigger trigger, bool state) {
		_triggerStates[trigger.Name] = state;
		TriggerChanged?.Invoke(trigger, state);
	}

	public static bool GetTriggerState(Trigger trigger, bool defaultValue = false) {
		bool hasValue = _triggerStates.TryGetValue(trigger.Name, out bool value);

		return hasValue ? value : defaultValue;
	}
}
