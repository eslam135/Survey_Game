//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>The TextNavigator for mobile platforms</summary>
	public class MobileTextNavigator: TextNavigator
	{
		/// <summary>The thumb size multiplier used for selection cursors size calculations</summary>
		private const float THUMB_SIZE_RATIO = 0.5f;

		/// <summary>The TouchScreenKeyboard</summary>
		private NativeKeyboard keyboard;

		private MobileCursorsControl mobileCursorsControl;

		/// <summary>Indicates whether at least one character has been inserted (or deleted) afer last click</summary>
		public bool hasInsertedCharAfterClick;

		/// <summary>Indicates whether at least one character has been inserted (or deleted) afer last click</summary>
		public bool HasInsertedCharAfterClick
		{
			get { return hasInsertedCharAfterClick; }
			set
			{
				hasInsertedCharAfterClick = value;

				if(!InputField.MobileSelectionCursorsEnabled) { return; }
				if(!hasInsertedCharAfterClick && !HasSelection)
				{
					MobileCursorsControl.ShowCurrentCursor();
				}
				else
				{
					MobileCursorsControl.HideCurrentCursor();
				}
			}
		}

		/// <summary>The TouchScreenKeyboard</summary>
		private NativeKeyboard Keyboard
		{
			get
			{
				if(keyboard == null)
				{
					keyboard = NativeKeyboardManager.Keyboard;
				}

				return keyboard;
			}
		}

		public override int ProcessedCaretPosition
		{
			get { return base.ProcessedCaretPosition; }
			set
			{
				base.ProcessedCaretPosition = value;

				if(Keyboard.State == KeyboardState.VISIBLE && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int SelectionStartPosition
		{
			get { return base.SelectionStartPosition; }
			protected set
			{
				int originalValue = base.SelectionStartPosition;
				base.SelectionStartPosition = value;

				if(InputField.MobileSelectionCursorsEnabled)
				{
					if(Canvas != null)
					{
						UpdateMobileSelectionCursors();
					}
				}

				if(InputField.ActionBarEnabled && base.SelectionStartPosition != originalValue)
				{
					UpdateSelectionCursorsActionBar();
				}

				if(Keyboard.State == KeyboardState.VISIBLE && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int ProcessedSelectionStartPosition
		{
			get { return base.ProcessedSelectionStartPosition; }
			protected set
			{
				int originalValue = base.ProcessedSelectionStartPosition;
				base.ProcessedSelectionStartPosition = value;

				if(InputField.MobileSelectionCursorsEnabled)
				{
					if(Canvas != null)
					{
						UpdateMobileSelectionCursors();
					}
				}

				if(InputField.ActionBarEnabled && base.ProcessedSelectionStartPosition != originalValue)
				{
					UpdateSelectionCursorsActionBar();
				}

				if(Keyboard.State == KeyboardState.VISIBLE && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int SelectionEndPosition
		{
			get { return base.SelectionEndPosition; }
			protected set
			{
				int originalValue = base.selectionEndPosition;
				base.SelectionEndPosition = value;

				if(InputField.MobileSelectionCursorsEnabled)
				{
					if(Canvas != null)
					{
						UpdateMobileSelectionCursors();
					}
				}

				if(InputField.ActionBarEnabled && base.SelectionEndPosition != originalValue)
				{
					UpdateSelectionCursorsActionBar();
				}

				if(Keyboard.State == KeyboardState.VISIBLE && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int ProcessedSelectionEndPosition
		{
			get { return base.ProcessedSelectionEndPosition; }
			protected set
			{
				int originalValue = base.ProcessedSelectionEndPosition;
				base.ProcessedSelectionEndPosition = value;

				if(InputField.MobileSelectionCursorsEnabled)
				{
					if(Canvas != null)
					{
						UpdateMobileSelectionCursors();
					}
				}

				if(InputField.ActionBarEnabled && base.ProcessedSelectionEndPosition != originalValue)
				{
					UpdateSelectionCursorsActionBar();
				}

				if(Keyboard.State == KeyboardState.VISIBLE && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		/// <summary>The ActionBar</summary>
		public ActionBar ActionBar { get; set; }

		/// <summary>The MobileCursorsControl</summary>
		public MobileCursorsControl MobileCursorsControl
		{
			get
			{
				if(mobileCursorsControl == null)
				{
					mobileCursorsControl = GameObject.FindObjectOfType<MobileCursorsControl>();
					if(mobileCursorsControl == null) //No existing instance
					{
						mobileCursorsControl = GameObject.Instantiate(Settings.MobileSelectionCursorsPrefab);
					}
				}

				return mobileCursorsControl;
			}
		}

		internal override void SetCaretPosition(int caretPosition, bool invokeCaretPositonChangeEvent = false)
		{
			base.SetCaretPosition(caretPosition, invokeCaretPositonChangeEvent);

			if(Keyboard.State == KeyboardState.VISIBLE && !InputField.ReadOnly && !BlockNativeSelectionChange)
			{
				UpdateNativeSelection();
			}
		}

		internal override void ResetCaret(Vector2 position)
		{
			base.ResetCaret(position);

			if(InputField.MobileSelectionCursorsEnabled)
			{
				if(Canvas != null)
				{
					UpdateMobileCurrentCursor(true);
				}
			}
		}

		internal override void OnCanvasScaleChanged(float canvasScaleFactor)
		{
			base.OnCanvasScaleChanged(canvasScaleFactor);

			UpdateCursorSize(canvasScaleFactor);
		}

		internal void UpdateCursorSize(float canvasScaleFactor)
		{
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

			cursorSize *= Settings.MobileSelectionCursorsScale;

			MobileCursorsControl.UpdateCursorSize(cursorSize);
		}

		internal override void BeginEditMode()
		{
			base.BeginEditMode();

			MobileCursorsControl.Setup(TextContentTransform, this);
			MobileCursorsControl.HideCursors();

			if(Canvas != null)
			{
				UpdateCursorSize(Canvas.scaleFactor);
			}
		}

		internal override void EndEditMode()
		{
			EditMode = false;
			caretBlinkTime = InputField.CaretBlinkRate;
			CaretRenderer.enabled = false;
			UpdateSelection(0, 0);

			if(MobileCursorsControl != null)
			{
				MobileCursorsControl.HideCursors();
			}

			if(ActionBar != null)
			{
				ActionBar.Hide();
			}

			ScrollArea scrollArea = TextAreaTransform.GetComponent<ScrollArea>();
			switch(InputField.ScrollBehaviourOnEndEdit)
			{
				case ScrollBehaviourOnEndEdit.START_OF_TEXT: scrollArea.MoveContentImmediately(Vector2.zero); break;
			}
			scrollArea.EditMode = false;
		}

		internal void HideCurrentMobileCursor()
		{
			MobileCursorsControl.HideCurrentCursor();
		}

		internal void UpdateNativeSelection()
		{
			if(HasSelection)
			{
				Keyboard.ChangeSelection(selectionStartPosition, selectionEndPosition);
			}
			else
			{
				Keyboard.ChangeSelection(caretPosition, caretPosition);
			}
		}

		public void UpdateSelectionStart(Vector2 position, out Vector2 cursorPosition, out bool switchToEnd)
		{
			int charIndex = GetCharacterIndexFromPosition(TextRenderer, position);
			if(charIndex <= SelectionEndPosition)
			{
				SelectionStartPosition = charIndex;
				CaretPosition = charIndex;
				switchToEnd = false;
			}
			else
			{
				SelectionStartPosition = SelectionEndPosition;
				SelectionEndPosition = charIndex;
				CaretPosition = charIndex;
				switchToEnd = true;
			}

			CharacterInfo charInfo = TextRenderer.GetCharacterInfo(charIndex);
			int lineIndex = DetermineCharacterLine(TextRenderer, charIndex);
			LineInfo lineInfo = TextRenderer.GetLineInfo(lineIndex);

			cursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
		}

		public void UpdateSelectionEnd(Vector2 position, out Vector2 cursorPosition, out bool switchToStart)
		{
			int charIndex = GetCharacterIndexFromPosition(TextRenderer, position);
			if(charIndex >= SelectionStartPosition)
			{
				SelectionEndPosition = charIndex;
				CaretPosition = charIndex;
				switchToStart = false;
			}
			else
			{
				SelectionEndPosition = SelectionStartPosition;
				SelectionStartPosition = charIndex;
				CaretPosition = charIndex;
				switchToStart = true;
			}

			CharacterInfo charInfo = TextRenderer.GetCharacterInfo(charIndex);
			int lineIndex = DetermineCharacterLine(TextRenderer, charIndex);
			LineInfo lineInfo = TextRenderer.GetLineInfo(lineIndex);

			cursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
		}

		public void UpdateCurrentCursor(Vector2 position, out Vector2 cursorPosition)
		{
			int charIndex = GetCharacterIndexFromPosition(TextRenderer, position);
			CaretPosition = charIndex;

			CharacterInfo charInfo = TextRenderer.GetCharacterInfo(charIndex);
			int lineIndex = DetermineCharacterLine(TextRenderer, charIndex);
			LineInfo lineInfo = TextRenderer.GetLineInfo(lineIndex);

			cursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
		}

		internal void ToggleActionBar()
		{
			if(ActionBar.Visible)
			{
				ActionBar.Hide();
			}
			else
			{
				ShowActionBar();
			}
		}

		internal void ShowActionBar()
		{
			bool paste = !InputField.ReadOnly && InputField.ActionBarPaste;
			bool selectAll = InputField.ActionBarSelectAll && InputField.Text.Length > 0;
			ActionBar.Show(false, false, paste, selectAll);
		}

		/// <summary>Updates the mobile selection cursors</summary>
		internal void UpdateMobileSelectionCursors(bool resetMobileCursorPosition = false)
		{
			TextRenderer activeTextRenderer = InputField.GetActiveTextRenderer();
			if(SelectionEndPosition > SelectionStartPosition || MobileCursorsControl.StartCursor.Selected || MobileCursorsControl.EndCursor.Selected)
			{
				float lineHeight = activeTextRenderer.GetLineInfo(0).height;

				if(SelectionStartPosition >= 0)
				{
					if(resetMobileCursorPosition)
					{
						int charIndex = Mathf.Clamp(SelectionStartPosition, 0, activeTextRenderer.CharacterCount - 1);
						CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
						int lineIndex = DetermineCharacterLine(activeTextRenderer, charIndex);
						LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

						Vector2 startCursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
						MobileCursorsControl.StartCursor.UpdatePosition(startCursorPosition);
					}

					if(MobileCursorsControl.ShouldStartCursorBeVisible)
					{
						MobileCursorsControl.ShowStartCursor();
					}
					else
					{
						MobileCursorsControl.HideStartCursor();
					}
				}
				else
				{
					MobileCursorsControl.HideStartCursor();
				}

				if(SelectionEndPosition >= 0)
				{
					if(resetMobileCursorPosition)
					{
						int charIndex = Mathf.Clamp(SelectionEndPosition, 0, activeTextRenderer.CharacterCount - 1);
						CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
						int lineIndex = DetermineCharacterLine(activeTextRenderer, charIndex);
						LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

						Vector2 endCursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
						MobileCursorsControl.EndCursor.UpdatePosition(endCursorPosition);
					}

					if(MobileCursorsControl.ShouldEndCursorBeVisible)
					{
						MobileCursorsControl.ShowEndCursor();
					}
					else
					{
						MobileCursorsControl.HideEndCursor();
					}
				}
				else
				{
					MobileCursorsControl.HideEndCursor();
				}

				MobileCursorsControl.HideCurrentCursor();
			}
			else
			{
				MobileCursorsControl.HideStartCursor();
				MobileCursorsControl.HideEndCursor();
			}
		}

		internal void UpdateSelectionCursorsActionBar()
		{
			if(SelectionEndPosition > SelectionStartPosition)
			{
				int startIndex = 0;

				if(SelectionStartPosition >= 0)
				{
					startIndex = Mathf.Clamp(SelectionStartPosition, 0, TextRenderer.CharacterCount - 1);
				}

				ActionBar.transform.SetParent(InputField.transform);
				ActionBar.transform.localScale = Vector3.one;
				bool cut = !InputField.Secure && !InputField.ReadOnly && InputField.ActionBarCut;
				bool copy = !InputField.Secure && InputField.ActionBarCopy;
				bool paste = !InputField.ReadOnly && InputField.ActionBarPaste;
				bool selectAll = InputField.ActionBarSelectAll;
				ActionBar.Show(cut, copy, paste, selectAll);
				Vector2 actionBarPosition = TextRenderer.GetCharacterInfo(startIndex).position;
				actionBarPosition += TextContentTransform.anchoredPosition;
				actionBarPosition.y = Mathf.Min(actionBarPosition.y, 0);
				ActionBar.UpdatePosition(actionBarPosition);

				float topY = GetAbsoluteTopY(ActionBar.RectTransform);
				if(topY > Canvas.pixelRect.height) //Out of bounds, move to bottom of InputField
				{
					actionBarPosition.y -= (InputField.Size.y + ActionBar.RectTransform.rect.height);
					ActionBar.UpdatePosition(actionBarPosition);
				}
			}
			else
			{
				ActionBar.Hide();
			}
		}

		public float GetAbsoluteTopY(RectTransform rectTransform)
		{
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);

			float topY = corners[1].y;
			float normalizedTopY = 0;
			if(Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				normalizedTopY = topY / Screen.height;
			}
			else
			{
				Camera camera = Canvas.worldCamera;
				normalizedTopY = (topY + camera.orthographicSize) / (camera.orthographicSize * 2);
			}

			return normalizedTopY * Canvas.pixelRect.height;
		}

		/// <summary>Updates the current cursor</summary>
		internal void UpdateMobileCurrentCursor(bool resetMobileCursorPosition = false)
		{
			TextRenderer activeTextRenderer = InputField.GetActiveTextRenderer();
			if(MobileCursorsControl.StartCursor.Selected || MobileCursorsControl.EndCursor.Selected)
			{
				MobileCursorsControl.HideCurrentCursor();
				return;
			}

			bool fixEmptyCaret = false;
			if(activeTextRenderer.CharacterCount == 0 || Text.Length == 0) //Workaround to make sure the text generator will give a correct position for the first character
			{
				fixEmptyCaret = true;
				activeTextRenderer.UpdateImmediately(" ");
			}

			int charIndex = Mathf.Clamp(CaretPosition, 0, activeTextRenderer.CharacterCount - 1);
			if(resetMobileCursorPosition)
			{
				CharacterInfo charInfo = activeTextRenderer.GetCharacterInfo(charIndex);
				int lineIndex = DetermineCharacterLine(activeTextRenderer, charIndex);
				LineInfo lineInfo = activeTextRenderer.GetLineInfo(lineIndex);

				Vector2 currentCursorPosition = new Vector2(charInfo.position.x, lineInfo.topY - lineInfo.height);
				if(CaretPosition >= activeTextRenderer.CharacterCountVisible)
				{
					currentCursorPosition.x += charInfo.width;
				}

				MobileCursorsControl.CurrentCursor.UpdatePosition(currentCursorPosition);
			}

			if(HasSelection)
			{
				MobileCursorsControl.HideCurrentCursor();
			}
			else if(!HasInsertedCharAfterClick)
			{
				MobileCursorsControl.ShowCurrentCursor();
			}

			if(InputField.ActionBarEnabled && !HasSelection)
			{
				ActionBar.transform.SetParent(InputField.transform);
				ActionBar.transform.localScale = Vector3.one;
				Vector2 actionBarPosition = activeTextRenderer.GetCharacterInfo(charIndex).position;
				actionBarPosition += TextContentTransform.anchoredPosition;
				actionBarPosition.y = Mathf.Min(actionBarPosition.y, 0);
				ActionBar.UpdatePosition(actionBarPosition);

				float topY = GetAbsoluteTopY(ActionBar.RectTransform);
				if(topY > Canvas.pixelRect.height) //Out of bounds, move to bottom of InputField
				{
					actionBarPosition.y -= (InputField.Size.y + ActionBar.RectTransform.rect.height);
					ActionBar.UpdatePosition(actionBarPosition);
				}
			}

			if(fixEmptyCaret)
			{
				activeTextRenderer.UpdateImmediately(string.Empty);
			}
		}

		internal override void UpdateCaret()
		{
			base.UpdateCaret();

			if(EditMode && (InputField.ActionBarEnabled || InputField.MobileSelectionCursorsEnabled))
			{
				if(Canvas != null)
				{
					UpdateMobileCurrentCursor(InputField.GetActiveTextRenderer());
				}
			}
		}

		internal override void SelectCurrentWord()
		{
			base.SelectCurrentWord();

			if(InputField.MobileSelectionCursorsEnabled)
			{
				if(Canvas != null)
				{
					UpdateMobileSelectionCursors(true);
				}
			}
		}

		internal override void SelectAll()
		{
			base.SelectAll();

			if(InputField.MobileSelectionCursorsEnabled)
			{
				if(Canvas != null)
				{
					UpdateMobileSelectionCursors(true);
				}
			}
		}

		internal override void UpdateSelectionArea(int currentPosition, int pressPosition, bool autoSelectWord)
		{
			base.UpdateSelectionArea(currentPosition, pressPosition, autoSelectWord);

			if(InputField.MobileSelectionCursorsEnabled)
			{
				UpdateMobileSelectionCursors(true);
			}
		}

		/// <summary>Updates the selection without updating selection in native text editor</summary>
		/// <param name="position">The new caret position</param>
		internal void UpdateSelection(int startPosition, int endPosition)
		{
			if(startPosition + 1 <= selectionStartPosition)
			{
				base.SelectionStartPosition = startPosition;
				base.SelectionEndPosition = endPosition;
				base.CaretPosition = startPosition;
			}
			else
			{
				base.SelectionStartPosition = startPosition;
				base.SelectionEndPosition = endPosition;
				base.CaretPosition = endPosition;
			}

			if(InputField.MobileSelectionCursorsEnabled)
			{
				if(Canvas != null)
				{
					UpdateMobileSelectionCursors();
				}
			}

			if(InputField.ActionBarEnabled)
			{
				UpdateSelectionCursorsActionBar();
			}
		}
	}
}