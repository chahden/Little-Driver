using TMPro;
using UnityEngine;

namespace LittleDriver.UI
{
    /// <summary>
    /// Shows Arabic correctly in a TextMeshPro text: type the sentence normally in <see cref="Text"/>
    /// (as you would read it), and this component joins the letters and writes it right to left.
    /// Edit the text here, not in the TextMeshPro box (that box is overwritten).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class ArabicText : MonoBehaviour
    {
        [Tooltip("The Arabic text, typed normally. Use Enter for a new line; long lines are not wrapped automatically.")]
        [TextArea(2, 5)]
        [SerializeField] private string text;

        private TMP_Text label;

        /// <summary>Change the Arabic text from code.</summary>
        public string Text
        {
            get => text;
            set
            {
                text = value;
                Apply();
            }
        }

        private void OnEnable() => Apply();

#if UNITY_EDITOR
        // Setting the TextMeshPro text inside OnValidate is not allowed, so do it right after.
        private void OnValidate() => UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) Apply();
        };
#endif

        private void Apply()
        {
            if (label == null) label = GetComponent<TMP_Text>();
            if (label == null) return;
            label.isRightToLeftText = false; // the shaper already ordered the letters
            label.richText = true;           // harakat are positioned with <space> tags
            label.text = ArabicShaper.Shape(text, AdvanceEm);
        }

        // Width of a glyph in em (1 em = the text's font size), read from the TextMeshPro font asset.
        private float AdvanceEm(char c)
        {
            TMP_FontAsset font = label.font;
            if (font == null) return 0f;
            if (!font.characterLookupTable.TryGetValue(c, out TMP_Character character))
            {
                font.TryAddCharacters(c.ToString());
                font.characterLookupTable.TryGetValue(c, out character);
            }
            if (character == null || font.faceInfo.pointSize <= 0) return 0f;
            return character.glyph.metrics.horizontalAdvance / font.faceInfo.pointSize;
        }
    }
}
