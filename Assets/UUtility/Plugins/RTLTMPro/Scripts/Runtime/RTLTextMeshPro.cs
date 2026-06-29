using TMPro;
using UnityEngine;

namespace RTLTMPro
{
    [ExecuteAlways]
    public class RTLTextMeshPro : TextMeshProUGUI
    {
        // ReSharper disable once InconsistentNaming
#if TMP_VERSION_2_1_0_OR_NEWER
        public override string text
#else
        public new string text
#endif
        {
            get { return base.text; }
            set
            {
                if (originalText == value)
                    return;

                originalText = value;

                UpdateText();
            }
        }

        public string OriginalText
        {
            get { return originalText; }
        }

        public bool PreserveNumbers
        {
            get { return preserveNumbers; }
            set
            {
                if (preserveNumbers == value)
                    return;

                preserveNumbers = value;
                havePropertiesChanged = true;
            }
        }

        public bool Farsi
        {
            get { return farsi; }
            set
            {
                if (farsi == value)
                    return;

                farsi = value;
                havePropertiesChanged = true;
            }
        }

        public bool FixTags
        {
            get { return fixTags; }
            set
            {
                if (fixTags == value)
                    return;

                fixTags = value;
                havePropertiesChanged = true;
            }
        }

        public bool ForceFix
        {
            get { return forceFix; }
            set
            {
                if (forceFix == value)
                    return;

                forceFix = value;
                havePropertiesChanged = true;
            }
        }

        public bool FixYah
        {
            get { return fixYah; }
            set
            {
                if (fixYah == value)
                    return;

                fixYah = value;
                havePropertiesChanged = true;
            }
        }

        public bool PreserveLeftOverLetter
        {
            get { return preserveLeftOverLetter; }
            set
            {
                if (preserveLeftOverLetter == value)
                    return;

                preserveLeftOverLetter = value;
                havePropertiesChanged = true;
            }
        }

        [SerializeField] protected bool preserveNumbers;

        [SerializeField] protected bool farsi = true;

        [SerializeField] [TextArea(3, 10)] protected string originalText;

        [SerializeField] protected bool fixTags = true;

        [SerializeField] protected bool forceFix;

        [SerializeField] protected bool fixYah = true;

        [SerializeField] protected bool preserveLeftOverLetter = false;

        protected readonly FastStringBuilder finalText = new FastStringBuilder(RTLSupport.DefaultBufferSize);

        protected void Update()
        {
            if (havePropertiesChanged)
            {
                UpdateText();
            }
        }

        public void UpdateText()
        {
            if (originalText == null)
                originalText = "";

            if (ForceFix == false && TextUtils.IsRTLInput(originalText) == false)
            {
                isRightToLeftText = false;
                base.text = originalText;
            } else
            {
                isRightToLeftText = true;
                base.text = GetFixedText(originalText);
            }

            havePropertiesChanged = true;
        }

        private string GetFixedText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            finalText.Clear();
            RTLSupport.FixRTL(input, finalText, farsi, fixTags, preserveNumbers, fixYah, preserveLeftOverLetter);
            finalText.Reverse();
            return finalText.ToString();
        }
    }
}