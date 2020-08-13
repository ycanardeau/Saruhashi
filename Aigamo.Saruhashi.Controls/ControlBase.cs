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
	}
}
