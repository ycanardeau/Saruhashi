using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aigamo.Saruhashi.MonoGame
{
	public sealed class MonoGameImage : IImage
	{
		public MonoGameImage(Texture2D texture, Color color)
		{
			Texture = texture;
			Color = color;
		}

		public Texture2D Texture { get; }
		public Color Color { get; }

	}
}
