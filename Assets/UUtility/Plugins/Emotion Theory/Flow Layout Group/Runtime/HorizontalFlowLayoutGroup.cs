namespace UnityEngine.UI
{
	[AddComponentMenu("Layout/Horizontal Flow Layout Group", 154)]
	/// <summary>
	/// Layout class for arranging child elements side by side.
	/// </summary>
	public class HorizontalFlowLayoutGroup : AbstractFlowLayoutGroup
	{
		protected HorizontalFlowLayoutGroup()
		{ }

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			CalcAlongAxis(0, false);
			CalcAlongAxis(1, false); // new
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void CalculateLayoutInputVertical()
		{
			CalcAlongAxis(0, false); // new
			CalcAlongAxis(1, false);
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutHorizontal()
		{
			SetChildrenAlongAxis(0, false);
			SetChildrenAlongAxis(1, false);  // new
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutVertical()
		{
			SetChildrenAlongAxis(0, false); // new
			SetChildrenAlongAxis(1, false);
		}
	}
}
