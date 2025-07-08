

namespace WebParticles;

public class Particle
{
    private const int BoundaryOffset = 50;

    public float X { get; set; }
    public float Y { get; set; }

    public float VelocityX { get; set; }
    public float VelocityY { get; set; }

    public int Size { get; set; }
    public Color Color { get; set; }

    public float GlowPhase { get; set; }

    public void Update(int screenWidth, int screenHeight, float maxGlowPhase)
    {
        X += VelocityX;
        Y += VelocityY;

        GlowPhase += 0.15f;

        if (GlowPhase >= maxGlowPhase)
        {
            GlowPhase -= maxGlowPhase;
        }

        if (X <= -BoundaryOffset)
        {
            X = -BoundaryOffset;
            VelocityX *= -1;
        }
        else if (X >= screenWidth + BoundaryOffset)
        {
            X = screenWidth + BoundaryOffset;
            VelocityX *= -1;
        }

        if (Y <= -BoundaryOffset)
        {
            Y = -BoundaryOffset;
            VelocityY *= -1;
        }
        else if (Y >= screenHeight + BoundaryOffset)
        {
            Y = screenHeight + BoundaryOffset;
            VelocityY *= -1;
        }
    }

    public float CalculateDistance(Particle p)
    {
        float dx = X - p.X;
        float dy = Y - p.Y;

        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
