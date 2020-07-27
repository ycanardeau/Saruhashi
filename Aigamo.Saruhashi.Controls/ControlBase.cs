using System;

namespace Aigamo.Saruhashi
{
	public abstract class ControlBase : Control
	{
		public ControlBase() : base()
		{
			GetText = () => Text;
		}

		public Func<string> GetText { get; set; }

		protected override void OnKeyDown(KeyEventArgs e)
		{
			var parent = Parent;
			while (parent != null)
			{
				if (parent is Form form && form.KeyPreview)
					form.OnKeyDown(e);

				parent = parent.Parent;
			}

			base.OnKeyDown(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			var parent = Parent;
			while (parent != null)
			{
				if (parent is Form form && form.KeyPreview)
					form.OnKeyPress(e);

				parent = parent.Parent;
			}

			base.OnKeyPress(e);
		}
	}
}
