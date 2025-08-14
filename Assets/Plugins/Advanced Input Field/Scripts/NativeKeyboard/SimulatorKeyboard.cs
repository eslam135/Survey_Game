using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public class SimulatorKeyboard: NativeKeyboard
	{
		public const float TRANSITION_TIME = 0.5f;
		public readonly Color DEFAULT_COLOR = Color.black;
		public readonly Color ACTIVE_COLOR = new Color(0, 0.5f, 1f);

		[SerializeField]
		private Button characterButtonPrefab;

		[SerializeField]
		private float xSpacing = 16;

		[SerializeField]
		private float ySpacing = 16;

		[SerializeField]
		private RectTransform[] characterRows;

		[SerializeField]
		private string[] mainPageValues;

		[SerializeField]
		private string[] symbolPage1Values;

		[SerializeField]
		private string[] symbolPage2Values;

		[SerializeField]
		private Button shiftButton;

		private RectTransform rectTransform;
		private Canvas canvas;
		private Vector2 buttonSize;
		private bool mainPageActive;
		private int pageNr;
		private bool uppercaseActive;

		private string text;
		private int selectionStartPosition;
		private int selectionEndPosition;

		private KeyboardType keyboardType;
		private CharacterValidation characterValidation;
		private LineType lineType;
		private AutocapitalizationType autocapitalizationType;
		private bool autocorrect;
		private bool secure;
		private bool emojisAllowed;
		private int characterLimit;
		private CharacterValidator characterValidator;
		private TextValidator textValidator;

		private float currentTransitionTime;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		private void Start()
		{
			buttonSize = DetermineSmallestButtonSize();
			ConfigureCharacterRows(mainPageValues);
			textValidator = new TextValidator(CharacterValidation.None, LineType.SingleLine);
		}

		private void Update()
		{
			if(State == KeyboardState.PENDING_SHOW)
			{
				currentTransitionTime += Time.deltaTime;
				if(currentTransitionTime >= TRANSITION_TIME)
				{
					currentTransitionTime = TRANSITION_TIME;
					OnKeyboardShow();
					int keyboardHeight = Mathf.RoundToInt(rectTransform.rect.height * canvas.scaleFactor); //Convert to screen pixels
					InvokeKeyboardHeightChanged(keyboardHeight); //Fully shown
				}

				float progress = currentTransitionTime / TRANSITION_TIME;
				Vector2 anchoredPositon = rectTransform.anchoredPosition;
				anchoredPositon.y = -((1 - progress) * rectTransform.rect.height);
				rectTransform.anchoredPosition = anchoredPositon;
			}
			else if(State == KeyboardState.PENDING_HIDE)
			{
				currentTransitionTime += Time.deltaTime;
				if(currentTransitionTime >= TRANSITION_TIME)
				{
					currentTransitionTime = TRANSITION_TIME;
					OnKeyboardHide();
					InvokeKeyboardHeightChanged(0); //Fully hidden
				}

				float progress = currentTransitionTime / TRANSITION_TIME;
				Vector2 anchoredPositon = rectTransform.anchoredPosition;
				anchoredPositon.y = -(progress * rectTransform.rect.height);
				rectTransform.anchoredPosition = anchoredPositon;
			}
		}

		internal override void Setup()
		{
			LoadMainPage();
			mainPageActive = true;
			uppercaseActive = false;
		}

		public override void ShowKeyboard(string text, KeyboardType keyboardType, CharacterValidation characterValidation, LineType lineType, AutocapitalizationType autocapitalizationType, bool autocorrection, bool secure, bool emojisAllowed, bool hasNext, int characterLimit, string characterValidatorJSON)
		{
			this.text = text;
			this.characterLimit = characterLimit;
			this.emojisAllowed = emojisAllowed;
			textValidator.Validation = characterValidation;
			textValidator.LineType = lineType;

			CharacterValidator characterValidator = null;
			if(!string.IsNullOrEmpty(characterValidatorJSON))
			{
				characterValidator = ScriptableObject.CreateInstance<CharacterValidator>();
				JsonUtility.FromJsonOverwrite(characterValidatorJSON, characterValidator);
			}
			textValidator.Validator = characterValidator;

			canvas = GetComponentInParent<Canvas>(); //Update current Canvas
			currentTransitionTime = 0;
			State = KeyboardState.PENDING_SHOW;

			LoadMainPage();
			mainPageActive = true;
			uppercaseActive = false;
		}

		public override void HideKeyboard()
		{
			canvas = GetComponentInParent<Canvas>(); //Update current Canvas
			currentTransitionTime = 0;
			State = KeyboardState.PENDING_HIDE;
		}

		public override void ChangeText(string text)
		{
			this.text = text;
		}

		public override void ChangeSelection(int selectionStartPosition, int selectionEndPosition)
		{
			this.selectionStartPosition = selectionStartPosition;
			this.selectionEndPosition = selectionEndPosition;
		}

		public void ConfigureCharacterRows(string[] characterRowValues)
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				ConfigureCharacterRow(characterRows[i], characterRowValues[i]);
			}
		}

		private void ConfigureCharacterRow(RectTransform characterRow, string characterRowValue)
		{
			CleanCharacterRow(characterRow);

			Vector2 boundsSize = characterRow.rect.size;
			int length = characterRowValue.Length;
			float x = (-((length - 1) * 0.5f) * (buttonSize.x + xSpacing));
			float y = 0;

			for(int i = 0; i < length; i++)
			{
				Button characterButton = CreateCharacterButton(characterRow);
				characterButton.onClick.AddListener(() => OnCharacterButtonClick(characterButton));
				RectTransform rectTransform = characterButton.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(x, y);
				rectTransform.sizeDelta = buttonSize;

				x += (buttonSize.x + xSpacing);
			}
		}

		private void CleanCharacterRow(RectTransform characterRow)
		{
			while(characterRow.childCount > 0)
			{
				DestroyImmediate(characterRow.GetChild(0).gameObject);
			}
		}

		private Vector2 DetermineSmallestButtonSize()
		{
			Vector2 smallestButtonSize = new Vector2(float.MaxValue, float.MaxValue);

			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				Vector2 boundsSize = characterRows[i].rect.size;
				float buttonWidth = ((boundsSize.x - xSpacing) / mainPageValues[i].Length) - xSpacing;
				float buttonHeight = boundsSize.y - ySpacing;

				smallestButtonSize.x = Mathf.Min(buttonWidth, smallestButtonSize.x);
				smallestButtonSize.y = Mathf.Min(buttonHeight, smallestButtonSize.y);
			}

			return smallestButtonSize;
		}

		private Button CreateCharacterButton(Transform parentTransform)
		{
			Button characterButton = Instantiate(characterButtonPrefab);
			RectTransform rectTransform = characterButton.GetComponent<RectTransform>();
			Vector2 size = rectTransform.sizeDelta;
			rectTransform.SetParent(parentTransform);
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.localPosition = Vector3.zero;
			rectTransform.sizeDelta = size;

			return characterButton;
		}

		private void LoadMainPage()
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				UpdateCharacterRow(characterRows[i], mainPageValues[i]);
			}

			Image iconRenderer = shiftButton.transform.Find("Icon").GetComponent<Image>();
			Text label = shiftButton.transform.Find("Label").GetComponent<Text>();
			iconRenderer.enabled = true;
			iconRenderer.color = DEFAULT_COLOR;
			label.enabled = false;
		}

		private void LoadSymbolsPage1()
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				UpdateCharacterRow(characterRows[i], symbolPage1Values[i]);
			}

			Image iconRenderer = shiftButton.transform.Find("Icon").GetComponent<Image>();
			Text label = shiftButton.transform.Find("Label").GetComponent<Text>();
			iconRenderer.enabled = false;
			label.enabled = true;
			label.text = "1/2";
		}

		private void LoadSymbolsPage2()
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				UpdateCharacterRow(characterRows[i], symbolPage2Values[i]);
			}


			Image iconRenderer = shiftButton.transform.Find("Icon").GetComponent<Image>();
			Text label = shiftButton.transform.Find("Label").GetComponent<Text>();
			iconRenderer.enabled = false;
			label.enabled = true;
			label.text = "2/2";
		}

		private void UpdateCharacterRow(RectTransform characterRow, string characterRowValue)
		{
			int length = characterRow.childCount;
			for(int i = 0; i < length; i++)
			{
				if(i >= characterRowValue.Length) { break; }

				Button characterButton = characterRow.GetChild(i).GetComponent<Button>();
				Text label = characterButton.GetComponentInChildren<Text>();
				label.text = characterRowValue[i].ToString();
			}
		}

		private void UpdateMainPageCase()
		{
			int length = characterRows.Length;
			for(int i = 0; i < length; i++)
			{
				UpdateCharacterRowCase(characterRows[i]);
			}
		}

		private void UpdateCharacterRowCase(RectTransform characterRow)
		{
			int length = characterRow.childCount;
			for(int i = 0; i < length; i++)
			{
				Button characterButton = characterRow.GetChild(i).GetComponent<Button>();
				Text label = characterButton.GetComponentInChildren<Text>();
				if(uppercaseActive)
				{
					label.text = label.text.ToUpper();
				}
				else
				{
					label.text = label.text.ToLower();
				}
			}
		}

		public void OnCharacterButtonClick(Button characterButton)
		{
			Text label = characterButton.GetComponentInChildren<Text>();
			string input = label.text;

			Insert(input);
		}

		public void Insert(string input)
		{
			if(characterLimit > 0 && text.Length + input.Length > characterLimit)
			{
				if(text.Length < characterLimit)
				{
					int amountAllowed = characterLimit - text.Length;
					input = input.Substring(0, amountAllowed);
				}
				else
				{
					return;
				}
			}

			int caretPosition = selectionStartPosition;
			if(selectionEndPosition > selectionStartPosition)
			{
				text = text.Remove(selectionStartPosition, selectionEndPosition - selectionStartPosition);
				selectionEndPosition = selectionStartPosition;
			}

			string lastText = text;

			textValidator.Validate(text, input, caretPosition, selectionStartPosition);
			string resultText = textValidator.ResultText;
			int resultCaretPosition = textValidator.ResultCaretPosition;

			ApplyCharacterLimit(ref resultText, ref resultCaretPosition);

			text = resultText;
			selectionStartPosition = resultCaretPosition;
			selectionEndPosition = resultCaretPosition;

			OnTextChanged(text);
			OnSelectionChanged(selectionStartPosition + ", " + selectionEndPosition);

			if(uppercaseActive)
			{
				OnShiftClick();
			}
		}

		public void ApplyCharacterLimit(ref string text, ref int caretPosition)
		{
			if(characterLimit != 0 && text.Length > characterLimit)
			{
				int amountOverLimit = text.Length - characterLimit;
				text = text.Substring(0, characterLimit);
				caretPosition = Mathf.Clamp(caretPosition, 0, text.Length);
			}
		}

		public void OnShiftClick()
		{
			if(mainPageActive)
			{
				Image iconRenderer = shiftButton.transform.Find("Icon").GetComponent<Image>();
				iconRenderer.enabled = true;
				uppercaseActive = !uppercaseActive;
				if(uppercaseActive)
				{
					iconRenderer.color = ACTIVE_COLOR;
				}
				else
				{
					iconRenderer.color = DEFAULT_COLOR;
				}
				UpdateMainPageCase();
			}
			else
			{
				if(pageNr == 1)
				{
					pageNr = 2;
					LoadSymbolsPage2();
				}
				else
				{
					pageNr = 1;
					LoadSymbolsPage1();
				}
			}
		}

		public void OnBackspaceClick()
		{
			if(selectionEndPosition > selectionStartPosition)
			{
				text = text.Remove(selectionStartPosition, selectionEndPosition - selectionStartPosition);
				selectionEndPosition = selectionStartPosition;

				OnTextChanged(text);
				OnSelectionChanged(selectionStartPosition + ", " + selectionEndPosition);
			}
			else if(selectionStartPosition > 0)
			{
				selectionStartPosition--;
				text = text.Remove(selectionStartPosition, 1);
				selectionEndPosition = selectionStartPosition;

				OnTextChanged(text);
				OnSelectionChanged(selectionStartPosition + ", " + selectionEndPosition);
			}

			if(uppercaseActive)
			{
				OnShiftClick();
			}
		}

		public void OnSymbolsClick()
		{
			mainPageActive = !mainPageActive;
			pageNr = 1;

			if(mainPageActive)
			{
				ConfigureCharacterRows(mainPageValues);
				LoadMainPage();
				uppercaseActive = false;
				UpdateMainPageCase();
			}
			else
			{
				ConfigureCharacterRows(symbolPage1Values);
				LoadSymbolsPage1();
			}
		}

		public void OnCommaClick()
		{
			Insert(",");
		}

		public void OnSpaceClick()
		{
			Insert(" ");
		}

		public void OnDotClick()
		{
			Insert(".");
		}

		public void OnDoneClick()
		{
			if(lineType == LineType.SingleLine || lineType == LineType.MultiLineNewline)
			{
				OnKeyboardDone();
			}
			else if(lineType == LineType.MultiLineNewline)
			{
				Insert("\n");
			}
		}
	}
}