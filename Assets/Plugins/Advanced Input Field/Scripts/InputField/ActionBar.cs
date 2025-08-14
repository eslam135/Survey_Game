//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	/// <summary>The onscreen control for cut, copy, paste and select all operations</summary>
	[RequireComponent(typeof(RectTransform))]
	public class ActionBar: MonoBehaviour
	{
		/// <summary>The multiplier for thumb height to calculate action bar height</summary>
		private const float THUMB_SIZE_RATIO = 0.5f;

		/// <summary>The multiplier of canvas min size (width or height) to calculate action bar width</summary>
		private const float CANVAS_MIN_SIZE_RATIO = 0.9f;

		/// <summary>The maximum amount of buttons that can active at one time (cut, copy, paste, select all)</summary>
		private const int MAX_BUTTONS = 4;

		/// <summary>The RectTransform for the ActionBar that will be rendered last in the Canvas</summary>
		[SerializeField]
		private CanvasFrontRenderer actionBarRenderer;

		/// <summary>The button for the cut operation</summary>
		[SerializeField]
		private Button cutButton;

		/// <summary>The button for the copy operation</summary>
		[SerializeField]
		private Button copyButton;

		/// <summary>The button for the paste operation</summary>
		[SerializeField]
		private Button pasteButton;

		/// <summary>The button for the select all operation</summary>
		[SerializeField]
		private Button selectAllButton;

		[SerializeField]
		private bool showDividers;

		/// <summary>The RectTransform</summary>
		public RectTransform RectTransform { get; private set; }

		/// <summary>The max size of the ActionBar when all buttons are enabled</summary>
		private Vector2 fullSize;

		/// <summary>The size of a button</summary>
		private Vector2 buttonSize;

		/// <summary>The TextInputHandler for mobile</summary>
		public MobileTextInputHandler TextInputHandler { get; private set; }

		/// <summary>The InputField</summary>
		public AdvancedInputField InputField { get; private set; }

		/// <summary>The Canvas</summary>
		public Canvas Canvas { get { return InputField.Canvas; } }

		/// <summary>Indicates if the ActionBar is visible</summary>
		public bool Visible { get { return gameObject.activeInHierarchy; } }

		/// <summary>Indicates if the cut operation is enabled</summary>
		private bool cut;

		/// <summary>Indicates if the copy operation is enabled</summary>
		private bool copy;

		/// <summary>Indicates if the paste operation is enabled</summary>
		private bool paste;

		/// <summary>Indicates if the select all operation is enabled</summary>
		private bool selectAll;

		/// <summary>Initializes this class</summary>
		internal void Initialize(AdvancedInputField inputField, MobileTextInputHandler textInputHandler)
		{
			InputField = inputField;
			TextInputHandler = textInputHandler;

			if(Canvas != null)
			{
				UpdateSize(Canvas.scaleFactor);
				actionBarRenderer.Initialize();
			}
		}

		#region UNITY
		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			gameObject.SetActive(false);
		}

		private void OnEnable()
		{
			actionBarRenderer.Show();
		}

		private void OnDisable()
		{
			actionBarRenderer.Hide();
		}
		#endregion

		/// <summary>Determines fullSize and buttonSize</summary>
		internal void UpdateSize(float canvasScaleFactor)
		{
			if(RectTransform == null)
			{
				RectTransform = GetComponent<RectTransform>();
			}

#if UNITY_EDITOR
			int thumbSize = -1;
#else
			int thumbSize = Util.DetermineThumbSize();
#endif
			float cursorSize;
			if(thumbSize <= 0) //Unknown DPI
			{
				if(InputField.TextRenderer.ResizeTextForBestFit)
				{
					cursorSize = InputField.TextRenderer.FontSizeUsedForBestFit * 1.5f;
				}
				else
				{
					cursorSize = InputField.TextRenderer.FontSize * 1.5f;
				}
			}
			else
			{
				cursorSize = (thumbSize * THUMB_SIZE_RATIO) / canvasScaleFactor;
			}

			float canvasMinSize = Mathf.Min(Canvas.pixelRect.width, Canvas.pixelRect.height);
			fullSize = new Vector2((canvasMinSize * CANVAS_MIN_SIZE_RATIO) / canvasScaleFactor, cursorSize);
			RectTransform.sizeDelta = fullSize;

			buttonSize = new Vector2(fullSize.x / MAX_BUTTONS, 0);

			actionBarRenderer.RefreshCanvas(Canvas);
			actionBarRenderer.SyncTransform(RectTransform);
		}

		public void UpdateCutLabel(string text)
		{
			cutButton.GetComponentInChildren<TextRenderer>().Text = text;
		}

		public void UpdateCopyLabel(string text)
		{
			copyButton.GetComponentInChildren<TextRenderer>().Text = text;
		}

		public void UpdatePasteLabel(string text)
		{
			pasteButton.GetComponentInChildren<TextRenderer>().Text = text;
		}

		public void UpdateSelectAllLabel(string text)
		{
			selectAllButton.GetComponentInChildren<TextRenderer>().Text = text;
		}

		/// <summary>Shows the ActionBar</summary>
		/// <param name="cut">Indicates if the cut button should be enabled</param>
		/// <param name="copy">Indicates if the copy button should be enabled</param>
		/// <param name="paste">Indicates if the paste button should be enabled</param>
		/// <param name="selectAll">Indicates if the select all button should be enabled</param>
		internal void Show(bool cut, bool copy, bool paste, bool selectAll)
		{
			if(Visible && this.cut == cut && this.copy == copy && this.paste == paste && this.selectAll == selectAll)
			{
				return;
			}
			else
			{
				this.cut = cut;
				this.copy = copy;
				this.paste = paste;
				this.selectAll = selectAll;
			}

			gameObject.SetActive(true);

			if(Canvas != null)
			{
				UpdateSize(Canvas.scaleFactor);
			}

			UpdateButtons();
		}

		internal void UpdateButtons()
		{
			List<Button> activeButtons = new List<Button>();
			cutButton.gameObject.SetActive(cut);
			if(cut)
			{
				activeButtons.Add(cutButton);
			}

			copyButton.gameObject.SetActive(copy);
			if(copy)
			{
				activeButtons.Add(copyButton);
			}

			pasteButton.gameObject.SetActive(paste);
			if(paste)
			{
				activeButtons.Add(pasteButton);
			}

			selectAllButton.gameObject.SetActive(selectAll);
			if(selectAll)
			{
				activeButtons.Add(selectAllButton);
			}

			int length = activeButtons.Count;
			RectTransform.sizeDelta = new Vector2(buttonSize.x * length, fullSize.y);

			for(int i = 0; i < length; i++)
			{
				RectTransform rectTransform = activeButtons[i].GetComponent<RectTransform>();
				rectTransform.sizeDelta = buttonSize;
				rectTransform.anchoredPosition = new Vector2(buttonSize.x * i, 0);

				if(!showDividers)
				{
					UpdateDividers(activeButtons[i], false, false);
				}
				else if(i == 0)
				{
					UpdateDividers(activeButtons[i], false, true);
				}
				else if(i == length - 1)
				{
					UpdateDividers(activeButtons[i], true, false);
				}
				else
				{
					UpdateDividers(activeButtons[i], true, true);
				}
			}

			if(Canvas != null)
			{
				actionBarRenderer.RefreshCanvas(Canvas);
				actionBarRenderer.SyncTransform(RectTransform);
			}
		}

		public void UpdateDividers(Button button, bool showLeft, bool showRight)
		{
			Transform leftDivider = button.transform.Find("LeftDivider");
			Transform rightDivider = button.transform.Find("RightDivider");

			if(leftDivider != null)
			{
				leftDivider.gameObject.SetActive(showLeft);
			}

			if(rightDivider != null)
			{
				rightDivider.gameObject.SetActive(showRight);
			}
		}

		/// <summary>Changes the position of the ActionBar</summary>
		/// <param name="position">The new position of the ActionBar</param>
		internal void UpdatePosition(Vector2 position)
		{
			if(Canvas != null)
			{
				position.x = 0;
				RectTransform.anchoredPosition = position;
				RectTransform.SetAsLastSibling();

				actionBarRenderer.RefreshCanvas(Canvas);
				actionBarRenderer.SyncTransform(RectTransform);
			}
		}

		/// <summary>Hides the ActionBar</summary>
		internal void Hide()
		{
			gameObject.SetActive(false);
			actionBarRenderer.gameObject.SetActive(false);
		}

		/// <summary>Event callback when the cut button has been clicked</summary>
		public void OnCutClick()
		{
			TextInputHandler.OnCut();
		}

		/// <summary>Event callback when the copy button has been clicked</summary>
		public void OnCopyClick()
		{
			TextInputHandler.OnCopy();
		}

		/// <summary>Event callback when the paste button has been clicked</summary>
		public void OnPasteClick()
		{
			TextInputHandler.OnPaste();
		}

		/// <summary>Event callback when the select all button has been clicked</summary>
		public void OnSelectAllClick()
		{
			TextInputHandler.OnSelectAll();
		}
	}
}