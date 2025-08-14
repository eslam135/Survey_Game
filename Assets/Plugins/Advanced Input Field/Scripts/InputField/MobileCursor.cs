using UnityEngine;
using UnityEngine.EventSystems;

namespace AdvancedInputFieldPlugin
{
	public delegate void OnMobileCursorSelected();
	public delegate void OnMobileCursorMoved(MobileCursor mobileCursor, PointerEventData eventData);
	public delegate void OnMobileCursorMoveFinished();

	public enum MobileCursorType { START_CURSOR, END_CURSOR, CURRENT_CURSOR }

	[RequireComponent(typeof(RectTransform))]
	public class MobileCursor: MonoBehaviour
	{
		private const float MIN_SIZE_RATIO = 0.6f;
		private const float MAX_SIZE_RATIO = 1f;
		private const float TRANSITION_TIME = 0.33f;

		[SerializeField]
		private CanvasFrontRenderer cursorRenderer;

		private RectTransform rectTransform;
		private RectTransform boundsTransform;
		private RectTransform imageTransform;

		private event OnMobileCursorSelected onMobileCursorSelected;
		private event OnMobileCursorMoved onMobileCursorMoved;
		private event OnMobileCursorMoveFinished onMobileCursorMoveFinished;
		private float currentTransitionTime;
		private Vector2 startTransitionPosition;

		public RectTransform RectTransform { get { return rectTransform; } }
		public CanvasFrontRenderer CursorRenderer { get { return cursorRenderer; } }
		public MobileCursorType Type { get; set; }
		public bool Selected { get; private set; }
		public bool OutOfBounds { get; set; }
		public Vector2 TargetPosition { get; set; }
		public Vector2 PressOffset { get; set; }

		public event OnMobileCursorSelected MobileCursorSelected
		{
			add { onMobileCursorSelected += value; }
			remove { onMobileCursorSelected -= value; }
		}

		public event OnMobileCursorMoved MobileCursorMoved
		{
			add { onMobileCursorMoved += value; }
			remove { onMobileCursorMoved -= value; }
		}

		public event OnMobileCursorMoveFinished MobileCursorMoveFinished
		{
			add { onMobileCursorMoveFinished += value; }
			remove { onMobileCursorMoveFinished -= value; }
		}

		public Vector2 Offset
		{
			get
			{
				Vector2 anchorOffset = imageTransform.anchorMax - new Vector2(0.5f, 0.5f);
#if UNITY_2018_1_OR_NEWER
				return anchorOffset * imageTransform.rect.size;
#else
				return new Vector2(anchorOffset.x * imageTransform.rect.size.x, anchorOffset.y * imageTransform.rect.size.y);
#endif
			}
		}

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			boundsTransform = cursorRenderer.GetComponent<RectTransform>();
			imageTransform = boundsTransform.Find("Image").GetComponent<RectTransform>();
			enabled = false;
		}

		private void Start()
		{
			PointerHandler pointerHandler = CursorRenderer.GetComponent<PointerHandler>();
			pointerHandler.Press += OnPress;
			pointerHandler.Release += OnRelease;
			pointerHandler.BeginDrag += OnBeginDrag;
			pointerHandler.Drag += OnDrag;
			pointerHandler.EndDrag += OnEndDrag;
		}

		private void Update()
		{
			if(currentTransitionTime < TRANSITION_TIME)
			{
				currentTransitionTime += Time.deltaTime;
				if(currentTransitionTime >= TRANSITION_TIME)
				{
					currentTransitionTime = TRANSITION_TIME;

					if(!Selected)
					{
						if(onMobileCursorMoveFinished != null)
						{
							onMobileCursorMoveFinished();
						}
					}
				}

				float progress = currentTransitionTime / TRANSITION_TIME;
				float sizeRatio;
				if(Selected)
				{
					sizeRatio = MIN_SIZE_RATIO + (progress * (MAX_SIZE_RATIO - MIN_SIZE_RATIO));
				}
				else
				{
					sizeRatio = MIN_SIZE_RATIO + ((1 - progress) * (MAX_SIZE_RATIO - MIN_SIZE_RATIO));
					rectTransform.anchoredPosition = Vector2.Lerp(startTransitionPosition, TargetPosition, progress);
					Sync();
				}

				imageTransform.sizeDelta = boundsTransform.rect.size * sizeRatio;
			}
		}

		private void OnEnable()
		{
			CursorRenderer.Show();
			Selected = false;
			imageTransform.sizeDelta = boundsTransform.rect.size * MIN_SIZE_RATIO;
			currentTransitionTime = TRANSITION_TIME;
		}

		private void OnDisable()
		{
			CursorRenderer.Hide();
			Selected = false;
			imageTransform.sizeDelta = boundsTransform.rect.size * MIN_SIZE_RATIO;
			currentTransitionTime = TRANSITION_TIME;
		}

		public void OnTextScrollChanged(Vector2 scroll)
		{
			if(!Selected)
			{
				Sync();
			}
		}

		public void OnPress(PointerEventData eventData)
		{
			Selected = true;
			currentTransitionTime = 0;

			if(onMobileCursorSelected != null)
			{
				onMobileCursorSelected();
			}
		}

		public void OnRelease(PointerEventData eventData)
		{
			Selected = false;
			currentTransitionTime = 0;
			startTransitionPosition = rectTransform.anchoredPosition;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if(onMobileCursorMoved != null)
			{
				onMobileCursorMoved(this, eventData);
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
		}

		public void UpdateSize(Vector2 size)
		{
			rectTransform.sizeDelta = size;
			Sync();

			imageTransform.sizeDelta = boundsTransform.rect.size * MIN_SIZE_RATIO;
		}

		public void UpdatePosition(Vector2 position)
		{
			rectTransform.anchoredPosition = position;
			TargetPosition = position;
			Sync();
		}

		private void Sync()
		{
			cursorRenderer.RefreshCanvas(GetComponentInParent<Canvas>());
			cursorRenderer.SyncTransform(rectTransform);
		}
	}
}
