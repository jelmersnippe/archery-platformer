using System;
using System.Collections.Generic;
using Godot;

public static class GlobalTriggerState {
	public static Action<Trigger, bool> TriggerChanged;
	private static readonly Dictionary<string, bool> TriggerStates = new ();

	public static void SetTriggerState(Trigger trigger, bool state)
	{
		TriggerStates[trigger.Name] = state;
		TriggerChanged.Invoke(trigger, state);
	}

	public static bool GetTriggerState(Trigger trigger, bool defaultValue = false) {
		bool hasValue = TriggerStates.TryGetValue(trigger.Name, out bool value);

		return hasValue ? value : defaultValue;
	}
}
