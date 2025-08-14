//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Access point for the NativeKeyboard for current platform</summary>
	public class NativeKeyboardManager: MonoBehaviour
	{
		/// <summary>The singleton instance of NativeKeyboardManager</summary>
		private static NativeKeyboardManager instance;

		/// <summary>The NativeKeyboard instance of current platform</summary>
		private NativeKeyboard keyboard;

		/// <summary>The singleton instance of NativeKeyboardManager</summary>
		public static NativeKeyboardManager Instance
		{
			get
			{
				if(instance == null)
				{
					instance = GameObject.FindObjectOfType<NativeKeyboardManager>();
					if(instance == null)
					{
						GameObject gameObject = new GameObject("NativeKeyboardManager");
						DontDestroyOnLoad(gameObject);
						instance = gameObject.AddComponent<NativeKeyboardManager>();
					}
				}

				return instance;
			}
		}

		/// <summary>The TouchScreenKeyboard instance of current platform</summary>
		public static NativeKeyboard Keyboard
		{
			get { return Instance.keyboard; }
		}

		/// <summary>Indicates whether a hardware keyboard is connected</summary>
		public static bool HardwareKeyboardConnected
		{
			get { return Instance.keyboard.HardwareKeyboardConnected; }
		}

		#region UNITY
		private void Awake()
		{
#if UNITY_EDITOR
#if (UNITY_ANDROID || UNITY_IOS || UNITY_UWP)
			if(Settings.SimulateMobileBehaviourInEditor)
			{
				Canvas mobileKeyboardCanvas = null;
				if(Screen.height > Screen.width)
				{
					mobileKeyboardCanvas = GameObject.Instantiate(Settings.PortraitKeyboardCanvasPrefab);
				}
				else
				{
					mobileKeyboardCanvas = GameObject.Instantiate(Settings.LandscapeKeyboardCanvasPrefab);
				}
				DontDestroyOnLoad(mobileKeyboardCanvas.gameObject);
				keyboard = mobileKeyboardCanvas.GetComponentInChildren<SimulatorKeyboard>();
				keyboard.Init(name);
			}
#endif
#elif UNITY_ANDROID
			keyboard = gameObject.AddComponent<AndroidKeyboard>();
			keyboard.Init(name);
#elif UNITY_IOS
			keyboard = gameObject.AddComponent<IOSKeyboard>();
			keyboard.Init(name);
#elif UNITY_WSA
			keyboard = gameObject.AddComponent<UWPKeyboard>();
			keyboard.Init(name);
#else
			Debug.LogWarning("Native Keyboard is only supported on Android, iOS and UWP");
#endif
		}

		private void OnDestroy()
		{
			instance = null;
		}
		#endregion

		public static void TryDestroy()
		{
			if(instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
			}
		}

		/// <summary>Checks whether the native binding should be active or not</summary>
		public static void UpdateKeyboardActiveState()
		{
			if(Keyboard == null) { return; }
			Keyboard.UpdateActiveState();
		}

		/// <summary>
		/// Enables hardware keyboard connectivity checks in the native binding.
		/// Use this when you want connectivity checks when no inputfield is selected.
		/// </summary>
		public static void EnableHardwareKeyboardUpdates()
		{
			if(Keyboard == null) { return; }
			Keyboard.EnableHardwareKeyboardUpdates();
		}

		/// <summary>
		/// Disables hardware keyboard connectivity checks in the native binding.
		/// Use this when you want to disable connectivity checks after using EnableHardwareKeyboardUpdates.
		/// </summary>
		public static void DisableHardwareKeyboardUpdates()
		{
			if(Keyboard == null) { return; }
			Keyboard.DisableHardwareKeyboardUpdates();
		}

		/// <summary>Shows the TouchScreenKeyboard for current platform</summary>
		/// <param name="text">The current text of the InputField</param>
		/// <param name="keyboardType">The keyboard type to use</param>
		/// <param name="characterValidation">The characterValidation to use</param>
		/// <param name="lineType">The lineType to use</param>
		/// <param name="autocorrection">Indicates whether autocorrection is enabled</param>
		/// <param name="characterLimit">The character limit for the text</param>
		/// <param name="secure">Indicates whether input should be secure</param>
		public static void ShowKeyboard(string text, KeyboardType keyboardType = KeyboardType.Default, CharacterValidation characterValidation = CharacterValidation.None, LineType lineType = LineType.SingleLine, AutocapitalizationType autocapitalizationType = AutocapitalizationType.NONE, bool autocorrection = true, bool secure = false, bool emojisAllowed = false, bool hasNext = false, int characterLimit = 0, string characterValidatorJSON = null)
		{
			if(Keyboard == null) { return; }
			Keyboard.ShowKeyboard(text, keyboardType, characterValidation, lineType, autocapitalizationType, autocorrection, secure, emojisAllowed, hasNext, characterLimit, characterValidatorJSON);
		}

		/// <summary>Hides the TouchScreenKeyboard for current platform</summary>
		public static void HideKeyboard()
		{
			if(Keyboard == null) { return; }
			Keyboard.HideKeyboard();
		}

		/// <summary>Adds a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to add</param>
		public static void AddKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener)
		{
			if(Keyboard == null) { return; }
			Keyboard.AddKeyboardHeightChangedListener(listener);
		}

		/// <summary>Removes a KeyboardHeightChangdeListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to remove</param>
		public static void RemoveKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener)
		{
			if(Keyboard == null) { return; }
			Keyboard.RemoveKeyboardHeightChangedListener(listener);
		}

		/// <summary>Adds a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The HardwareKeyboardChangedListener to add</param>
		public static void AddHardwareKeyboardChangedListener(OnHardwareKeyboardChangedHandler listener)
		{
			if(Keyboard == null) { return; }
			Keyboard.AddHardwareKeyboardChangedListener(listener);
		}

		/// <summary>Removes a KeyboardHeightChangedListener</summary>
		/// <param name="listener">The KeyboardHeightChangedListener to remove</param>
		public static void RemoveHardwareKeyboardChangedListener(OnHardwareKeyboardChangedHandler listener)
		{
			if(Keyboard == null) { return; }
			Keyboard.RemoveHardwareKeyboardChangedListener(listener);
		}
	}
}
