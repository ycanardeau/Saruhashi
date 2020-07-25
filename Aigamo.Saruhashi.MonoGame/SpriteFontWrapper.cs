using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Aigamo.Saruhashi.MonoGame
{
	public sealed class SpriteFontWrapper : IMonoGameFont
	{
		public SpriteFontWrapper(SpriteFont spriteFont)
		{
			SpriteFont = spriteFont;
		}

		public SpriteFont SpriteFont { get; }

		public void Draw(SpriteBatch spriteBatch, string? text, Vector2 position, XnaColor color) => spriteBatch.DrawString(SpriteFont, text, position, color);

		public Vector2 MeasureString(string? text) => SpriteFont.MeasureString(text);
	}
}
