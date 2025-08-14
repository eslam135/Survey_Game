using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>The behaviour to determine which keyboard type to use</summary>
	public enum MobileKeyboardBehaviour { USE_HARDWARE_KEYBOARD_WHEN_AVAILABLE, ALWAYS_USE_TOUCHSCREENKEYBOARD, ALWAYS_USE_HARDWAREKEYBOARD }

	public class SettingsData: ScriptableObject
	{
		[Tooltip("The LocalizationData assets for plugin specific strings (ex. ActionBar buttons)")]
		[SerializeField, CustomName("Localizations")]
		private LocalizationData[] localizations;

		[Tooltip("The behaviour to determine which keyboard type to use")]
		[SerializeField, CustomName("Keyboard Behaviour")]
		private MobileKeyboardBehaviour mobileKeyboardBehaviour;

		[Tooltip("The prefab to use for Action Bar on Android")]
		[SerializeField, CustomName("Android Action Bar Prefab")]
		private ActionBar androidActionBarPrefab;

		[Tooltip("The prefab to use for Action Bar on iOS")]
		[SerializeField, CustomName("iOS Action Bar Prefab")]
		private ActionBar iOSActionBarPrefab;

		[Tooltip("The prefab to use for Action Bar on UWP")]
		[SerializeField, CustomName("UWP Action Bar Prefab")]
		private ActionBar uwpActionBarPrefab;

		[Tooltip("The prefab to use for Selection Cursors")]
		[SerializeField, CustomName("Selection Cursors Prefab")]
		private MobileCursorsControl mobileSelectionCursorsPrefab;

		[Tooltip("The scale of the selection cursors (1 is default)")]
		[SerializeField, CustomName("Selection Cursors Scale")]
		[Range(0.01f, 10)]
		private float mobileSelectionCursorsScale = 1;

		[Tooltip("Indicates whether the plugin should behave like a Mobile Device in the Editor")]
		[SerializeField, CustomName("Simulate Mobile Behaviour In Editor")]
		private bool simulateMobileBehaviourInEditor = true;

		[Tooltip("The prefab to use for simulating the Mobile Keyboard in Portrait mode")]
		[SerializeField, CustomName("Portrait Keyboard Canvas Prefab")]
		private Canvas portraitKeyboardCanvasPrefab;

		[Tooltip("The prefab to use for simulating the Mobile Keyboard in Landscape mode")]
		[SerializeField, CustomName("Landscape Keyboard Canvas Prefab")]
		private Canvas landscapeKeyboardCanvasPrefab;

		public LocalizationData[] Localizations { get { return localizations; } }
		public MobileKeyboardBehaviour MobileKeyboardBehaviour { get { return mobileKeyboardBehaviour; } }
		public ActionBar AndroidActionBarPrefab { get { return androidActionBarPrefab; } }
		public ActionBar IOSActionBarPrefab { get { return iOSActionBarPrefab; } }
		public ActionBar UWPActionBarPrefab { get { return uwpActionBarPrefab; } }
		public MobileCursorsControl MobileSelectionCursorsPrefab { get { return mobileSelectionCursorsPrefab; } }
		public float MobileSelectionCursorsScale { get { return mobileSelectionCursorsScale; } }
		public bool SimulateMobileBehaviourInEditor { get { return simulateMobileBehaviourInEditor; } }
		public Canvas PortraitKeyboardCanvasPrefab { get { return portraitKeyboardCanvasPrefab; } }
		public Canvas LandscapeKeyboardCanvasPrefab { get { return landscapeKeyboardCanvasPrefab; } }
	}
}