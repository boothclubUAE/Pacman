namespace UnityEngine.UI
{
	[AddComponentMenu("Layout/Vertical Flow Layout Group", 155)]
	/// <summary>
	/// Layout class for arranging child elements side by side.
	/// </summary>
	public class VerticalFlowLayoutGroup : AbstractFlowLayoutGroup
	{
		protected VerticalFlowLayoutGroup()
		{ }

		public override Vector2 Spacing => new Vector2(lineSpacing, spacing);

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			CalcAlongAxis(1, true);
			CalcAlongAxis(0, true);
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void CalculateLayoutInputVertical()
		{
			CalcAlongAxis(1, true);
			CalcAlongAxis(0, true);
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutHorizontal()
		{
			SetChildrenAlongAxis(1, true);
			SetChildrenAlongAxis(0, true);
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutVertical()
		{
			SetChildrenAlongAxis(1, true);
			SetChildrenAlongAxis(0, true);
		}
	}
}
