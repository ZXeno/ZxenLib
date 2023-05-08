namespace ZxenLib.Infrastructure;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class FrameDataUtility
{
    private double frameTime;
    private double averageFps;
    private Queue<double> frameTimes;
    private double accumulatedFrameTimes;
    private SpriteFont font;
    private GraphicsDeviceManager graphicsManager;

    public FrameDataUtility(IAssetManager assetManager, GraphicsDeviceManager graphicsManager, string fontId = "uifont")
    {
        this.frameTime = 0;
        this.averageFps = 0;
        this.frameTimes = new Queue<double>();
        this.accumulatedFrameTimes = 0;
        this.font = assetManager.FontsDictionary[fontId];
        this.graphicsManager = graphicsManager;
    }

    public void Update(GameTime gameTime)
    {
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

        this.frameTime = deltaTime;
        this.accumulatedFrameTimes += deltaTime;
        this.frameTimes.Enqueue(deltaTime);

        while (this.accumulatedFrameTimes > 1)
        {
            double oldestFrameTime = this.frameTimes.Dequeue();
            this.accumulatedFrameTimes -= oldestFrameTime;
        }

        this.averageFps = this.frameTimes.Count;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(this.font, $"Frame Time: {this.frameTime * 1000:0.00} ms", new Vector2(10, 10), Color.White);
        spriteBatch.DrawString(this.font, $"Average FPS: {this.averageFps:0.00}", new Vector2(10, 30), Color.White);
        spriteBatch.DrawString(this.font, $"Resolution Width : {this.graphicsManager.PreferredBackBufferWidth}", new Vector2(10, 50), Color.White);
        spriteBatch.DrawString(this.font, $"Resolution Height: {this.graphicsManager.PreferredBackBufferHeight}", new Vector2(10, 70), Color.White);
    }
}