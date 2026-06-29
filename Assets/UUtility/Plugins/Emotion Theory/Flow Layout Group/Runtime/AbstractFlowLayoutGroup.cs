using System.Collections.Generic;

namespace UnityEngine.UI
{
	/// <summary>
	/// Abstract base class for HorizontalLayoutGroup and VerticalLayoutGroup to generalize common functionality.
	/// </summary>
	///
	[ExecuteAlways]
	public abstract class AbstractFlowLayoutGroup : LayoutGroup
	{
		protected override void Awake()
		{
			base.Awake();

			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		[SerializeField] protected float m_Spacing = 0;

		/// <summary>
		/// The spacing to use between layout elements in the layout group.
		/// </summary>
		public float spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }

		[SerializeField] protected float m_LineSpacing = 0;

		/// <summary>
		/// The spacing to use between layout elements in the layout group.
		/// </summary>
		public float lineSpacing { get { return m_LineSpacing; } set { SetProperty(ref m_LineSpacing, value); } }

		public virtual Vector2 Spacing => new Vector2(spacing, lineSpacing);


		[SerializeField] protected bool m_ChildControlWidth = true;

		/// <summary>
		/// Returns true if the Layout Group controls the widths of its children. Returns false if children control their own widths.
		/// </summary>
		/// <remarks>
		/// If set to false, the layout group will only affect the positions of the children while leaving the widths untouched. The widths of the children can be set via the respective RectTransforms in this case.
		///
		/// If set to true, the widths of the children are automatically driven by the layout group according to their respective minimum, preferred, and flexible widths. This is useful if the widths of the children should change depending on how much space is available.In this case the width of each child cannot be set manually in the RectTransform, but the minimum, preferred and flexible width for each child can be controlled by adding a LayoutElement component to it.
		/// </remarks>
		public bool childControlWidth { get { return m_ChildControlWidth; } set { SetProperty(ref m_ChildControlWidth, value); } }

		[SerializeField] protected bool m_ChildControlHeight = true;

		/// <summary>
		/// Returns true if the Layout Group controls the heights of its children. Returns false if children control their own heights.
		/// </summary>
		/// <remarks>
		/// If set to false, the layout group will only affect the positions of the children while leaving the heights untouched. The heights of the children can be set via the respective RectTransforms in this case.
		///
		/// If set to true, the heights of the children are automatically driven by the layout group according to their respective minimum, preferred, and flexible heights. This is useful if the heights of the children should change depending on how much space is available.In this case the height of each child cannot be set manually in the RectTransform, but the minimum, preferred and flexible height for each child can be controlled by adding a LayoutElement component to it.
		/// </remarks>
		public bool childControlHeight { get { return m_ChildControlHeight; } set { SetProperty(ref m_ChildControlHeight, value); } }

		[SerializeField] protected bool m_ChildScaleWidth = false;

		/// <summary>
		/// Whether to use the x scale of each child when calculating its width.
		/// </summary>
		public bool childScaleWidth { get { return m_ChildScaleWidth; } set { SetProperty(ref m_ChildScaleWidth, value); } }

		[SerializeField] protected bool m_ChildScaleHeight = false;

		/// <summary>
		/// Whether to use the y scale of each child when calculating its height.
		/// </summary>
		public bool childScaleHeight { get { return m_ChildScaleHeight; } set { SetProperty(ref m_ChildScaleHeight, value); } }

		/// <summary>
		/// Whether the order of children objects should be sorted in reverse.
		/// </summary>
		/// <remarks>
		/// If False the first child object will be positioned first.
		/// If True the last child object will be positioned first.
		/// </remarks>
		public bool reverseArrangement { get { return m_ReverseArrangement; } set { SetProperty(ref m_ReverseArrangement, value); } }

		[SerializeField] protected bool m_ReverseArrangement = false;

		/// <summary>
		/// Calculate the layout element properties for this layout element along the given axis.
		/// </summary>
		/// <param name="axis">The axis to calculate for. 0 is horizontal and 1 is vertical.</param>
		/// <param name="isVertical">Is this group a vertical group?</param>
		protected void CalcAlongAxis(int axis, bool isVertical)
		{
			float size = rectTransform.rect.size[axis];
			float innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);

			float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical);
			bool controlSize = (axis == 0 ? m_ChildControlWidth : m_ChildControlHeight);
			bool useScale = (axis == 0 ? m_ChildScaleWidth : m_ChildScaleHeight);

			float totalMin = combinedPadding;
			float totalPreferred = combinedPadding;
			float totalFlexible = 0;

			bool alongOtherAxis = (isVertical ^ (axis == 1));
			var rectChildrenCount = rectChildren.Count;

			int otherAxis = axis == 0 ? 1 : 0;
			float otherSize = rectTransform.rect.size[otherAxis];
			float otherInnerSize = otherSize - (otherAxis == 0 ? padding.horizontal : padding.vertical);
			bool useScaleOtherAxis = (otherAxis == 0 ? m_ChildScaleWidth : m_ChildScaleHeight);
			int rowIndex = 0;
			float currentRowLength = 0;
			float rowOffset = 0;
			float rowSize = 0;

			int startIndex = m_ReverseArrangement ? rectChildren.Count - 1 : 0;
			int endIndex = m_ReverseArrangement ? 0 : rectChildren.Count;
			int increment = m_ReverseArrangement ? -1 : 1;
			for (int i = startIndex; m_ReverseArrangement ? i >= endIndex : i < endIndex; i += increment)
			{
				RectTransform child = rectChildren[i];
				float min, preferred, flexible;
				float vmin, vpreferred, vflexible;
				GetChildSizes(child, axis, controlSize, false, out min, out preferred, out flexible);
				GetChildSizes(child, otherAxis, controlSize, false, out vmin, out vpreferred, out vflexible);

				if (useScale)
				{
					float scaleFactor = child.localScale[axis];
					min *= scaleFactor;
					preferred *= scaleFactor;
					flexible *= scaleFactor;
				}

				// 0 = otherAxis
				// 1 = axis
				if (alongOtherAxis)
				{
					totalMin = Mathf.Max(min + combinedPadding, totalMin);
					totalPreferred = Mathf.Max(preferred + combinedPadding, totalPreferred);
					totalFlexible = Mathf.Max(flexible, totalFlexible);

					currentRowLength += child.sizeDelta[otherAxis] * (useScaleOtherAxis ? child.localScale[otherAxis] : 1f);
					{
						if (currentRowLength > otherInnerSize)
						{
							rowIndex++;
							rowOffset += rowSize;
							rowSize = child.sizeDelta[axis] * (useScale ? child.localScale[axis] : 1f);
							currentRowLength = child.sizeDelta[otherAxis] * (useScaleOtherAxis ? child.localScale[otherAxis] : 1f);
						}
						else
						{
							rowSize = Mathf.Max(child.sizeDelta[axis] * (useScale ? child.localScale[axis] : 1f), rowSize);
						}
						currentRowLength += spacing;
					}
				}
				else
				{
					totalMin += min + spacing;
					totalPreferred += preferred + spacing;

					// Increment flexible size with element's flexible size.
					totalFlexible += flexible;
				}
			}

			if (!alongOtherAxis && rectChildren.Count > 0)
			{
				totalMin -= spacing;
				totalPreferred -= spacing;
			}
			totalPreferred = Mathf.Max(totalMin, totalPreferred);

			if (alongOtherAxis)
			{
				rowOffset += rowSize;
				totalMin = totalPreferred;

				totalPreferred = rowOffset + lineSpacing * rowIndex + combinedPadding;
			}

			if (!alongOtherAxis)
				SetLayoutInputForAxis(totalFlexible, totalFlexible, totalFlexible, axis);
			else
				SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);

		}

		public float GetWidth => rectTransform.rect.width - padding.horizontal;

		public float GetHeight => rectTransform.rect.height;

		/// <summary>
		/// Set the positions and sizes of the child layout elements for the given axis.
		/// </summary>
		/// <param name="axis">The axis to handle. 0 is horizontal and 1 is vertical.</param>
		/// <param name="isVertical">Is this group a vertical group?</param>
		protected void SetChildrenAlongAxis(int axis, bool isVertical)
		{
			float size = rectTransform.rect.size[axis];
			float innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);
			bool controlSize = (axis == 0 ? m_ChildControlWidth : m_ChildControlHeight);
			bool useScale = (axis == 0 ? m_ChildScaleWidth : m_ChildScaleHeight);
			float alignmentOnAxis = GetAlignmentOnAxis(axis);

			bool alongOtherAxis = (isVertical ^ (axis == 1));
			int startIndex = m_ReverseArrangement ? rectChildren.Count - 1 : 0;
			int endIndex = m_ReverseArrangement ? 0 : rectChildren.Count;
			int increment = m_ReverseArrangement ? -1 : 1;

			int otherAxis = axis == 0 ? 1 : 0;
			bool useScaleOtherAxis = (otherAxis == 0 ? m_ChildScaleWidth : m_ChildScaleHeight);
			float otherSize = rectTransform.rect.size[otherAxis];
			float otherInnerSize = otherSize - (otherAxis == 0 ? padding.horizontal : padding.vertical);

			// 0 = otherAxis
			// 1 = axis
			if (alongOtherAxis)
			{
				float currentRowLength = 0;
				int rowIndex = 0;
				float rowSize = 0;
				float rowOffset = 0;

				float CalcOtherAxisOffset(float delta)
				{
					// Horizontal
					if (axis == 0)
					{
						// Left
						if (childAlignment == TextAnchor.UpperLeft || childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.LowerLeft)
							return 0;
						// Center
						else if (childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.LowerCenter)
							return delta / 2f;
						// Right
						else
							return delta;
					}
					// Vertical
					else
					{
						// Top
						if (childAlignment == TextAnchor.UpperLeft || childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.UpperRight)
							return 0;
						// Middle
						else if (childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.MiddleRight)
							return delta / 2f;
						// Bottom
						else
							return delta;
					}
				}

				for (int i = startIndex; m_ReverseArrangement ? i >= endIndex : i < endIndex; i += increment)
				{
					RectTransform child = rectChildren[i];
					float min, preferred, flexible;
					float hMin, hPreferred, hFlexible;
					GetChildSizes(child, axis, controlSize, false, out min, out preferred, out flexible);
					GetChildSizes(child, otherAxis, controlSize, false, out hMin, out hPreferred, out hFlexible);

					float scaleFactor = useScale ? child.localScale[axis] : 1f;
					float requiredSpace = Mathf.Clamp(innerSize, min, flexible > 0 ? size : preferred);

					float startOffset = size - GetTotalPreferredSize(axis);
					startOffset = CalcOtherAxisOffset(startOffset);
					startOffset += axis == 0 ? padding.left : padding.top;

					currentRowLength += child.sizeDelta[otherAxis] * (useScaleOtherAxis ? child.localScale[otherAxis] : 1f);
					{
						if (currentRowLength > otherInnerSize)
						{
							rowIndex++;
							rowOffset += rowSize;
							rowSize = child.sizeDelta[axis] * (useScale ? child.localScale[axis] : 1f);
							currentRowLength = child.sizeDelta[otherAxis] * (useScaleOtherAxis ? child.localScale[otherAxis] : 1f);
						}
						else
						{
							rowSize = Mathf.Max(child.sizeDelta[axis] * (useScaleOtherAxis ? child.localScale[axis] : 1f), rowSize);
						}
						currentRowLength += spacing;
					}

					float rowPos = rowOffset + /*rowSize * rowIndex*/ +lineSpacing * (rowIndex);

					if (controlSize)
					{
						SetChildAlongAxisWithScale(child, axis, startOffset + rowPos, requiredSpace, scaleFactor);
					}
					else
					{
						float offsetInCell = (requiredSpace - child.sizeDelta[axis]) * alignmentOnAxis;
						SetChildAlongAxisWithScale(child, axis, startOffset + offsetInCell + rowPos, scaleFactor);
					}
				}
			}
			else
			{
				float pos = (axis == 0 ? padding.left : padding.top);
				float itemFlexibleMultiplier = 0;
				float surplusSpace = size - GetTotalPreferredSize(axis);

				float minMaxLerp = 0;
				if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
					minMaxLerp = Mathf.Clamp01((size - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));

				// Get lengths for each row to know how much to offset them by when ordering them.
				List<List<RectTransform>> DivideIntoRows()
				{
					int _rowIndex = 0;
					float _currentRowLength = 0;

					List<List<RectTransform>> _rows = new List<List<RectTransform>>();
					_rows.Add(new List<RectTransform>());

					for (int i = startIndex; m_ReverseArrangement ? i >= endIndex : i < endIndex; i += increment)
					{
						RectTransform child = rectChildren[i];
						float min, preferred, flexible;
						GetChildSizes(child, axis, controlSize, false, out min, out preferred, out flexible);
						{
							min = preferred;

							_currentRowLength += child.sizeDelta[axis] * (useScale ? child.localScale[axis] : 1f);

							if (_currentRowLength > innerSize)
							{
								//Debug.Log("We are exceeding!!! With length " + currentRowLength);
								_rowIndex++;
								_rows.Add(new List<RectTransform>());
								_currentRowLength = child.sizeDelta[axis] * (useScale ? child.localScale[axis] : 1f);

								//pos = (axis == 0 ? padding.left : padding.top);
							}
							else
							{
								//currentRowLength += spacing;
							}
							_rows[_rowIndex].Add(child);
							_currentRowLength += spacing;
						}
					}

					return _rows;
				}
				List<List<RectTransform>> rows = DivideIntoRows();
				float CalcRowSize(int index)
				{
					var row = rows[index];
					float _size = 0;
					foreach (var child in row)
					{
						_size += child.sizeDelta[axis] * (useScale ? child.localScale[axis] : 1f);
						_size += spacing;
					}
					if (row.Count > 0)
						_size -= spacing;
					return _size;
				}
				float CalcRowOffset(int index)
				{
					float rowSize = CalcRowSize(index);
					float delta = innerSize - rowSize;

					// Horizontal
					if (axis == 0)
					{
						if (childAlignment == TextAnchor.UpperLeft || childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.LowerLeft)
							return 0;
						else if (childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.LowerCenter)
							return delta / 2f;
						else
							return delta;
					}
					// Vertical
					else
					{
						if (childAlignment == TextAnchor.UpperLeft || childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.UpperRight)
							return 0;
						else if (childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.MiddleRight)
							return delta / 2f;
						else
							return delta;
					}
				}

				int rowIndex = 0;
				float currentRowLength = 0;

				// 0 = axis
				// 1 = otherAxis
				for (int i = startIndex; m_ReverseArrangement ? i >= endIndex : i < endIndex; i += increment)
				{
					RectTransform child = rectChildren[i];
					float min, preferred, flexible;
					GetChildSizes(child, axis, controlSize, false, out min, out preferred, out flexible);

					{
						min = preferred;

						currentRowLength += child.sizeDelta[axis] * (useScale ? child.localScale[axis] : 1f);

						if (currentRowLength > innerSize)
						{
							rowIndex++;
							currentRowLength = child.sizeDelta[axis] * (useScale ? child.localScale[axis] : 1f);

							pos = (axis == 0 ? padding.left : padding.top);
						}
						currentRowLength += spacing;
					}

					float scaleFactor = useScale ? child.localScale[axis] : 1f;
					float childSize = Mathf.Lerp(min, preferred, minMaxLerp);
					childSize += flexible * itemFlexibleMultiplier;
					float posOffset = CalcRowOffset(rowIndex);
					if (controlSize)
					{
						SetChildAlongAxisWithScale(child, axis, pos + posOffset, childSize, scaleFactor);
					}
					else
					{
						SetChildAlongAxisWithScale(child, axis, pos + posOffset, scaleFactor);
					}
					pos += childSize * scaleFactor + spacing;
				}
			}
		}

		private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand,
			out float min, out float preferred, out float flexible)
		{
			if (!controlSize)
			{
				min = child.sizeDelta[axis];
				preferred = min;
				flexible = 0;
			}
			else
			{
				min = LayoutUtility.GetMinSize(child, axis);
				preferred = LayoutUtility.GetPreferredSize(child, axis);
				flexible = LayoutUtility.GetFlexibleSize(child, axis);
			}
		}

#if UNITY_EDITOR
		protected override void Reset()
		{
			base.Reset();

			// For new added components we want these to be set to false,
			// so that the user's sizes won't be overwritten before they
			// have a chance to turn these settings off.
			// However, for existing components that were added before this
			// feature was introduced, we want it to be on be default for
			// backwardds compatibility.
			// Hence their default value is on, but we set to off in reset.
			m_ChildControlWidth = false;
			m_ChildControlHeight = false;
		}

		private int m_Capacity = 10;
		private Vector2[] m_Sizes = new Vector2[10];

		protected virtual void Update()
		{
			if (Application.isPlaying)
				return;

			int count = transform.childCount;

			if (count > m_Capacity)
			{
				if (count > m_Capacity * 2)
					m_Capacity = count;
				else
					m_Capacity *= 2;

				m_Sizes = new Vector2[m_Capacity];
			}

			// If children size change in editor, update layout (case 945680 - Child GameObjects in a Horizontal/Vertical Layout Group don't display their correct position in the Editor)
			bool dirty = false;
			for (int i = 0; i < count; i++)
			{
				RectTransform t = transform.GetChild(i) as RectTransform;
				if (t != null && t.sizeDelta != m_Sizes[i])
				{
					dirty = true;
					m_Sizes[i] = t.sizeDelta;
				}
			}

			if (dirty)
				LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
		}

#endif
	}
}