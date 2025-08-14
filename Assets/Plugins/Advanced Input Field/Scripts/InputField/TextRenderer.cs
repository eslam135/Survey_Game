using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public struct CharacterInfo
	{
		public Vector2 position;
		public float width;
		public int index;
		public int partIndex;
		public int partCount;
	}

	public struct LineInfo
	{
		public float topY;
		public float height;
		public int startCharIdx;
	}

	public enum TextAlignment { TOP_LEFT, TOP, TOP_RIGHT, LEFT, CENTER, RIGHT, BOTTOM_LEFT, BOTTOM, BOTTOM_RIGHT };

	[RequireComponent(typeof(RectTransform))]
	public abstract class TextRenderer: MonoBehaviour
	{
		private const string NOT_CONFIGURED_ERROR = "This Text Renderer is probably not configured correctly. Please see the ReadMe on how to configure this specific type of Text Renderer";

		protected RectTransform rectTransform;
		protected Vector2 preferredSize;
		protected bool multiline;

		public RectTransform RectTransform
		{
			get
			{
				if(rectTransform == null)
				{
					rectTransform = GetComponent<RectTransform>();
				}

				return rectTransform;
			}
		}

		public virtual bool Visible { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public float CaretWidth { get; set; }
		public virtual bool Multiline { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual TextAlignment TextAlignment { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }

		public virtual string Text { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }

		public Vector2 PreferredSize { get { return preferredSize; } }

		public virtual int LineCount { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual int CharacterCount { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual int CharacterCountVisible { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual Color Color { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual float FontSize { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } set { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual bool ResizeTextForBestFit { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }
		public virtual float FontSizeUsedForBestFit { get { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); } }

		protected virtual void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		public virtual void Show() { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual void Hide() { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }

		public virtual void UpdateImmediately(bool generateOutOfBounds = true) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual void UpdateImmediately(string text, bool generateOutOfBounds = true) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual bool FontHasCharacter(char c) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual bool IsReady() { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual CharacterInfo GetCharacterInfo(int index) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
		public virtual LineInfo GetLineInfo(int index) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }

		/// <summary>Gets the character index  of the line end</summary>
		/// <param name="line">The line to check</param>
		public virtual int GetLineEndCharIndex(int line) { throw new System.NotImplementedException(NOT_CONFIGURED_ERROR); }
	}
}
