using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UTool.Utility;

using TMPro;

namespace UTool.TabSystem
{
    public class InputFieldCtrl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_InputField inputField;
        [SpaceArea]
        [SerializeField] private bool increment;
        [SerializeField] private bool floatIncrement;
        [SpaceArea]
        [SerializeField] private int defaultSen;
        [SerializeField] private int sen;
        [SpaceArea]
        [SerializeField] private float floatIncrementStage1;
        [SerializeField] private float incrementStage1;
        [SerializeField] private float incrementStage2;
        [SerializeField] private float incrementStage3;
        [SpaceArea]
        [SerializeField] private UnityEvent OnValueChange = new UnityEvent();
        [SerializeField] private UnityEvent OnValueUpdated = new UnityEvent();

        private bool isDragging = false;
        private int dragCount = 0;

        public void UpdateValue(string value)
        {
            OnValueChange?.Invoke();
        }

        public void ApplyValue(string value)
        {
            OnValueUpdated?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isDragging)
                return;

            inputField.Select();
            inputField.MoveTextEnd(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!increment)
                return;

            dragCount++;

            if (dragCount < (Input.GetKey(KeyCode.LeftControl) ? sen : defaultSen))
                return;
            else
                dragCount = 0;

            float incrementAmount = GetincrementAmount() * eventData.delta.x.Signum();
            inputField.text = float.Parse(inputField.text) + incrementAmount + "";

            //if (incrementAmount != 0)
            //    UpdateValue(inputField.text);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;

            //ApplyValue(inputField.text);
        }

        private float GetincrementAmount()
        {
            int stage = 0;

            if (floatIncrement)
                if (Input.touchCount == 1)
                    stage = -1;

            if (Input.GetKey(KeyCode.LeftAlt) || Input.touchCount == 3)
                stage = 1;

            if (Input.GetKey(KeyCode.LeftShift) || Input.touchCount == 4)
                stage = 2;

            switch (stage)
            {
                case -1:
                    return floatIncrementStage1;

                case 0:
                    return incrementStage1;

                case 1:
                    return incrementStage2;

                case 2:
                    return incrementStage3;

                default:
                    return incrementStage1;
            }
        }
    }
}