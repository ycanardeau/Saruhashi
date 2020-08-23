using Microsoft.Xna.Framework.Graphics;

namespace Aigamo.Saruhashi.MonoGame
{
	public sealed class MonoGameGraphicsFactory : IGraphicsFactory
	{
		public MonoGameGraphicsFactory(SpriteBatch spriteBatch)
		{
			SpriteBatch = spriteBatch;
		}

		public SpriteBatch SpriteBatch { get; }

		public Graphics Create(Control control) => new MonoGameGraphics(control, SpriteBatch);
	}
}
