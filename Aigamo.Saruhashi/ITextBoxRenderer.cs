using System.Drawing;

namespace Aigamo.Saruhashi
{
	public interface ITextBoxRenderer
	{
		void DrawTextBox(Graphics graphics, Rectangle bounds, TextBoxState state);
	}
}
