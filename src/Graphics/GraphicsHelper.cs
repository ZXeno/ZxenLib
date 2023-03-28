namespace ZxenLib.Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// A collection of sprite helper functions.
/// </summary>
public static class GraphicsHelper
{
    /// <summary>
    /// Draws a box.
    /// </summary>
    /// <param name="sb">The <see cref="SpriteBatch"/> performing draw call batching.</param>
    /// <param name="texture">The texture to use.</param>
    /// <param name="destinationRect">The destination bounds.</param>
    /// <param name="spriteSourceRect">The source bounds.</param>
    /// <param name="cornerSize">The size of the corner slices used to draw the box.</param>
    /// <param name="color">The draw color. Generally Color.White.</param>
    public static void DrawBox(SpriteBatch sb, Texture2D texture, Rectangle destinationRect, Rectangle spriteSourceRect, int cornerSize, Color color)
    {
        // Top-Left corner
        Rectangle tlDestRect = new Rectangle(
            destinationRect.Left,
            destinationRect.Top,
            cornerSize,
            cornerSize);
        Rectangle tlSourceRect = new Rectangle(
            spriteSourceRect.X,
            spriteSourceRect.Y,
            cornerSize,
            cornerSize);

        // Top-Right corner
        Rectangle trDestRect = new Rectangle(
            destinationRect.Right - cornerSize,
            destinationRect.Top,
            cornerSize,
            cornerSize);
        Rectangle trSourceRect = new Rectangle(
            spriteSourceRect.X + spriteSourceRect.Width - cornerSize,
            spriteSourceRect.Y,
            cornerSize,
            cornerSize);

        // Bottom-Left corner
        Rectangle blDestRect = new Rectangle(
            destinationRect.Left,
            destinationRect.Bottom - cornerSize,
            cornerSize,
            cornerSize);
        Rectangle blSourceRect = new Rectangle(
            spriteSourceRect.X,
            spriteSourceRect.Y + spriteSourceRect.Height - cornerSize,
            cornerSize,
            cornerSize);

        // Bottom-Right corner
        Rectangle brDestRect = new Rectangle(
            destinationRect.Right - cornerSize,
            destinationRect.Bottom - cornerSize,
            cornerSize,
            cornerSize);
        Rectangle brSourceRect = new Rectangle(
            spriteSourceRect.X + spriteSourceRect.Width - cornerSize,
            spriteSourceRect.Y + spriteSourceRect.Height - cornerSize,
            cornerSize,
            cornerSize);

        // Center
        Rectangle cntrDestRect = new Rectangle(
            destinationRect.Left + cornerSize,
            destinationRect.Top + cornerSize,
            destinationRect.Width - (cornerSize * 2),
            destinationRect.Height - (cornerSize * 2));
        Rectangle cntrSourceRect = new Rectangle(
            spriteSourceRect.Left + cornerSize,
            spriteSourceRect.Top + cornerSize,
            spriteSourceRect.Width - (cornerSize * 2),
            spriteSourceRect.Height - (cornerSize * 2));

        // Border top
        Rectangle btDestRect = new Rectangle(
            destinationRect.Left + cornerSize,
            destinationRect.Top,
            destinationRect.Width - (cornerSize * 2),
            cornerSize);
        Rectangle btSourceRect = new Rectangle(
            spriteSourceRect.Left + cornerSize,
            spriteSourceRect.Top,
            spriteSourceRect.Width - (cornerSize * 2),
            cornerSize);

        // Border bottom
        Rectangle bbDestRect = new Rectangle(
            destinationRect.Left + cornerSize,
            destinationRect.Bottom - cornerSize,
            destinationRect.Width - (cornerSize * 2),
            cornerSize);
        Rectangle bbSourceRect = new Rectangle(
            spriteSourceRect.Left + cornerSize,
            spriteSourceRect.Bottom - cornerSize,
            spriteSourceRect.Width - (cornerSize * 2),
            cornerSize);

        // Border Left
        Rectangle bdrlftDestRect = new Rectangle(
            destinationRect.Left,
            destinationRect.Top + cornerSize,
            cornerSize,
            destinationRect.Height - (cornerSize * 2));
        Rectangle bdrlftSourceRect = new Rectangle(
            spriteSourceRect.Left,
            spriteSourceRect.Top + cornerSize,
            cornerSize,
            spriteSourceRect.Height - (cornerSize * 2));

        // Border Right
        Rectangle bdrRDestRect = new Rectangle(
            destinationRect.Right - cornerSize,
            destinationRect.Top + cornerSize,
            cornerSize,
            destinationRect.Height - (cornerSize * 2));
        Rectangle bdrRSourceRect = new Rectangle(
            spriteSourceRect.Right - cornerSize,
            spriteSourceRect.Top + cornerSize,
            cornerSize,
            spriteSourceRect.Height - (cornerSize * 2));

        // Corners
        sb.Draw(texture, tlDestRect, tlSourceRect, color);
        sb.Draw(texture, trDestRect, trSourceRect, color);
        sb.Draw(texture, blDestRect, blSourceRect, color);
        sb.Draw(texture, brDestRect, brSourceRect, color);

        // Content
        sb.Draw(texture, cntrDestRect, cntrSourceRect, color);

        // Border top / bottom
        sb.Draw(texture, btDestRect, btSourceRect, color);
        sb.Draw(texture, bbDestRect, bbSourceRect, color);

        // Border left / right
        sb.Draw(texture, bdrlftDestRect, bdrlftSourceRect, color);
        sb.Draw(texture, bdrRDestRect, bdrRSourceRect, color);
    }
}