using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UTool;
using UTool.Utility;

public class RingImage : MonoBehaviour
{
    [SerializeField] private RectTransform outerRing;
    [SerializeField] private RectTransform interRing;
    [SerializeField] private RectTransform angleRing;
    [SpaceArea]
    [SerializeField] private Image angleLimit;
    [SerializeField] private Image image;
    [SpaceArea]
    [SerializeField] public Vector2 outerRingSize;
    [SerializeField] public Vector2 interRingSize;
    [SpaceArea]
    [SerializeField] public float ringStartOffset;
    [SerializeField][Range(0, 1)] public float ringPivot;
    [SerializeField] public float angle;
    [SpaceArea]
    [SerializeField][ReorderableList(Foldable = true)] public List<RingChildTransform> ringChildTransforms = new List<RingChildTransform>(); 

    private void OnValidate()
    {
        UpdateTransform();
    }

    private void Start()
    {
        UpdateTransform();
    }

    public void UpdateTransform()
    {
        outerRing.sizeDelta = angleRing.sizeDelta = outerRingSize;
        interRing.sizeDelta = interRingSize;

        angleLimit.fillAmount = angle / 360;

        float rotationOffset = angle * ringPivot;
        rotationOffset -= ringStartOffset;
        angleRing.localRotation = Quaternion.Euler(0, 0, rotationOffset);

        foreach (RingChildTransform cTransform in ringChildTransforms)
        {
            float xPos = UUtility.RangedMapClamp(cTransform.pivot.x, 0, 1, -rotationOffset, -rotationOffset + angle);
            xPos += cTransform.offset.x;

            Vector2 xDir = UUtility.GetCircleVector(xPos).normalized;

            Vector2 currentInnerSize = (interRingSize * 0.5f) * xDir;
            Vector2 currentOuterSize = (outerRingSize * 0.5f) * xDir;

            Vector2 position = Vector2.Lerp(currentInnerSize, currentOuterSize, cTransform.pivot.y);
            position = position + (xDir * cTransform.offset.y);

            if (cTransform.childRect)
            {
                cTransform.childRect.anchoredPosition = position;

                if (cTransform.rotateWithRing)
                {
                    float lookAtAngle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg; // calculate the angle in degrees
                    cTransform.childRect.localRotation = Quaternion.Euler(0, 0, lookAtAngle - 90);
                }

                if(cTransform.scaleWithRing)
                {
                    float size = angle.RangedMapClamp(cTransform.ringSizeRange.x, cTransform.ringSizeRange.y, cTransform.childSizeRange.x, cTransform.childSizeRange.y);
                    cTransform.childRect.localScale = Vector2.one * size;
                }
            }
        }
    }

    [Serializable]
    public class RingChildTransform
    {
        [BeginGroup]
        [InLineEditor]
        public RectTransform childRect;
        [SpaceArea]
        public bool rotateWithRing;
        [SpaceArea]
        public bool scaleWithRing;
        public Vector2 ringSizeRange;
        public Vector2 childSizeRange;
        [SpaceArea]
        public Vector2 pivot;
        [EndGroup]
        public Vector2 offset;
    }
}