using UnityEngine;
using UnityEngine.EventSystems;

namespace AdvancedInputFieldPlugin
{
	public class MobileCursorsControl: MonoBehaviour
	{
		public MobileTextNavigator TextNavigator { get; private set; }
		public MobileCursor CurrentCursor { get; private set; }
		public MobileCursor StartCursor { get; private set; }
		public MobileCursor EndCursor { get; private set; }

		private int startCaretPosition;
		private bool initialized;

		public bool ShouldStartCursorBeVisible
		{
			get
			{
				if(StartCursor.Selected) { return true; }

				return Util.RectTransformIntersects(TextNavigator.TextAreaTransform, 1, StartCursor.RectTransform, 1.1f); //Make size slighly bigger to avoid accuracy problems
			}
		}

		public bool ShouldEndCursorBeVisible
		{
			get
			{
				if(EndCursor.Selected) { return true; }

				return Util.RectTransformIntersects(TextNavigator.TextAreaTransform, 1, EndCursor.RectTransform, 1.1f); //Make size slighly bigger to avoid accuracy problems
			}
		}

		private void Awake()
		{
			if(!initialized) { Initialize(); }
		}

		private void Initialize()
		{
			CurrentCursor = transform.Find("CurrentCursor").GetComponent<MobileCursor>();
			StartCursor = transform.Find("StartCursor").GetComponent<MobileCursor>();
			EndCursor = transform.Find("EndCursor").GetComponent<MobileCursor>();

			initialized = true;
		}

		private void Start()
		{
			CurrentCursor.MobileCursorSelected += OnCurrentCursorSelected;
			CurrentCursor.MobileCursorMoved += OnCurrentCursorMoved;
			CurrentCursor.MobileCursorMoveFinished += OnCurrentCursorMoveFinished;
			StartCursor.MobileCursorMoved += OnCursorMoved;
			StartCursor.MobileCursorMoveFinished += OnCursorMoveFinished;
			EndCursor.MobileCursorMoved += OnCursorMoved;
			EndCursor.MobileCursorMoveFinished += OnCursorMoveFinished;
		}

		public void Setup(Transform parent, MobileTextNavigator textNavigator)
		{
			if(!initialized) { Initialize(); }

			if(TextNavigator != null)
			{
				ScrollArea scrollArea = TextNavigator.TextAreaTransform.GetComponent<ScrollArea>();
				scrollArea.OnValueChanged.RemoveListener(OnTextScrollChanged);
			}

			transform.SetParent(parent);
			transform.localScale = Vector3.one;
			transform.localPosition = Vector3.zero;
			transform.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

			TextNavigator = textNavigator;
			if(TextNavigator != null)
			{
				ScrollArea scrollArea = TextNavigator.TextAreaTransform.GetComponent<ScrollArea>();
				scrollArea.OnValueChanged.AddListener(OnTextScrollChanged);
			}

			CurrentCursor.Type = MobileCursorType.CURRENT_CURSOR;
			StartCursor.Type = MobileCursorType.START_CURSOR;
			EndCursor.Type = MobileCursorType.END_CURSOR;
		}

		public void OnTextScrollChanged(Vector2 scroll)
		{
			CurrentCursor.OnTextScrollChanged(scroll);
			StartCursor.OnTextScrollChanged(scroll);
			EndCursor.OnTextScrollChanged(scroll);

			if(!(StartCursor.Selected || EndCursor.Selected)) { return; } //Probably text removal caused the text scroll

			if(ShouldStartCursorBeVisible)
			{
				ShowStartCursor();
			}
			else
			{
				HideStartCursor();
			}

			if(ShouldEndCursorBeVisible)
			{
				ShowEndCursor();
			}
			else
			{
				HideEndCursor();
			}
		}

		public void ShowCurrentCursor()
		{
			if(!initialized) { Initialize(); }
			CurrentCursor.enabled = true;
		}

		public void ShowStartCursor()
		{
			if(!initialized) { Initialize(); }
			StartCursor.enabled = true;
		}

		public void ShowEndCursor()
		{
			if(!initialized) { Initialize(); }
			EndCursor.enabled = true;
		}

		public void HideCursors()
		{
			if(!initialized) { Initialize(); }
			CurrentCursor.enabled = false;
			StartCursor.enabled = false;
			EndCursor.enabled = false;
		}

		public void HideCurrentCursor()
		{
			if(!initialized) { Initialize(); }
			CurrentCursor.enabled = false;
		}

		public void HideStartCursor()
		{
			if(!initialized) { Initialize(); }
			StartCursor.enabled = false;
		}

		public void HideEndCursor()
		{
			if(!initialized) { Initialize(); }
			EndCursor.enabled = false;
		}

		public void UpdateCursorSize(float cursorSize)
		{
			CurrentCursor.UpdateSize(new Vector2(cursorSize, cursorSize));
			StartCursor.UpdateSize(new Vector2(cursorSize, cursorSize));
			EndCursor.UpdateSize(new Vector2(cursorSize, cursorSize));
		}

		public void OnCursorMoved(MobileCursor mobileCursor, PointerEventData eventData)
		{
			mobileCursor.OutOfBounds = PositionOutOfBounds(eventData);

			Vector2 localMousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(TextNavigator.TextAreaTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
			localMousePosition.x += (TextNavigator.TextAreaTransform.rect.width * 0.5f);
			localMousePosition.y -= (TextNavigator.TextAreaTransform.rect.height * 0.5f);
			localMousePosition += mobileCursor.Offset;

			Vector2 anchoredPosition = localMousePosition;
			anchoredPosition -= TextNavigator.TextContentTransform.anchoredPosition;
			mobileCursor.UpdatePosition(anchoredPosition);

			if(mobileCursor.Type == MobileCursorType.START_CURSOR)
			{
				bool switchToEnd;
				Vector2 cursorPosition;
				TextNavigator.UpdateSelectionStart(localMousePosition, out cursorPosition, out switchToEnd);
				mobileCursor.TargetPosition = cursorPosition;

				if(switchToEnd)
				{
					mobileCursor.Type = MobileCursorType.END_CURSOR;
				}
			}
			else if(mobileCursor.Type == MobileCursorType.END_CURSOR)
			{
				bool switchToStart;
				Vector2 cursorPosition;
				TextNavigator.UpdateSelectionEnd(localMousePosition, out cursorPosition, out switchToStart);
				mobileCursor.TargetPosition = cursorPosition;

				if(switchToStart)
				{
					mobileCursor.Type = MobileCursorType.START_CURSOR;
				}
			}
		}

		public bool PositionOutOfBounds(PointerEventData eventData)
		{
			return !RectTransformUtility.RectangleContainsScreenPoint(TextNavigator.TextAreaTransform, eventData.position, eventData.pressEventCamera);
		}

		public void OnCursorMoveFinished()
		{
			TextNavigator.UpdateMobileSelectionCursors(true);
			StartCursor.Type = MobileCursorType.START_CURSOR;
			EndCursor.Type = MobileCursorType.END_CURSOR;
		}

		public void OnCurrentCursorSelected()
		{
			startCaretPosition = TextNavigator.CaretPosition;
		}

		public void OnCurrentCursorMoved(MobileCursor mobileCursor, PointerEventData eventData)
		{
			mobileCursor.OutOfBounds = PositionOutOfBounds(eventData);

			Vector2 localMousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(TextNavigator.TextAreaTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
			localMousePosition.x += (TextNavigator.TextAreaTransform.rect.width * 0.5f);
			localMousePosition.y -= (TextNavigator.TextAreaTransform.rect.height * 0.5f);
			localMousePosition += mobileCursor.Offset;

			Vector2 anchoredPosition = localMousePosition;
			anchoredPosition -= TextNavigator.TextContentTransform.anchoredPosition;
			mobileCursor.UpdatePosition(anchoredPosition);

			Vector2 cursorPosition;
			TextNavigator.UpdateCurrentCursor(localMousePosition, out cursorPosition);
			mobileCursor.TargetPosition = cursorPosition;
		}

		public void OnCurrentCursorMoveFinished()
		{
			if(TextNavigator.CaretPosition == startCaretPosition)
			{
				TextNavigator.ToggleActionBar();
			}
		}
	}
}
