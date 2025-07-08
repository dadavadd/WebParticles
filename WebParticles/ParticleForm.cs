namespace WebParticles;

public partial class ParticleForm : Form
{
    private readonly List<Particle> _particles = new();
    private readonly System.Windows.Forms.Timer _animationTimer = new();
    private readonly Pen _connectionPen = new(Color.Cyan, width: 2f);
    private readonly SolidBrush _particleBrush = new(Color.White);
    private Point _mousePosition = default;

    private const int MaxParticles = 25;
    private const float ConnectionDistance = 170f;
    private const float ParticleSpeed = 4f;
    private const float MaxGlowPhase = (float)Math.PI * 2;
    private const float MouseAttractionStrength = 0.1f;
    private const float MouseInfluenceRadius = 150f;
    private const int MaxAlphaValue = 255;

    public ParticleForm()
    {
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;
        DoubleBuffered = true;

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        InitializeParticleSystem();
    }

    private void InitializeParticleSystem()
    {
        for (int i = 0; i < MaxParticles; i++)
        {
            var particle = CreateRandomParticle();
            _particles.Add(particle);
        }

        _animationTimer.Interval = 10;
        _animationTimer.Tick += AnimationTimer_Tick;
        _animationTimer.Start();
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        UpdateParticles();
        Invalidate();
    }

    private void UpdateParticles()
    {
        foreach (var particle in _particles)
        {
            particle.Update(ClientSize.Width, ClientSize.Height, MaxGlowPhase);
        }
    }

    private Particle CreateRandomParticle(int x = -1, int y = -1)
    {
        float pX = x == -1 ? Random.Shared.Next(0, ClientSize.Width) : x;
        float pY = y == -1 ? Random.Shared.Next(0, ClientSize.Height) : y;

        return new()
        {
            X = pX,
            Y = pY,
            VelocityX = (float)(Random.Shared.NextDouble() - 0.5) * ParticleSpeed,
            VelocityY = (float)(Random.Shared.NextDouble() - 0.5) * ParticleSpeed,
            Size = Random.Shared.Next(5, 10),
            GlowPhase = (float)(Random.Shared.NextDouble() * MaxGlowPhase),
            Color = GenerateRandomColor()
        };
    }

    private void DrawConnections(Graphics g)
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            for (int j = i + 1; j < _particles.Count; j++)
            {
                var p1 = _particles[i];
                var p2 = _particles[j];

                float distance = p1.CalculateDistance(p2);

                if (distance < ConnectionDistance)
                {
                    int alphaValue = CalculateConnectionAlpha(distance);

                    var pen = _connectionPen.Color = Color.FromArgb(alphaValue, Color.Red);
                    g.DrawLine(_connectionPen, p1.X, p1.Y, p2.X, p2.Y);
                }
            }
        }
    }

    private void DrawMouseConnections(Graphics g)
    {
        foreach (var particle in _particles)
        {
            float distance = new Particle { X = _mousePosition.X, Y = _mousePosition.Y }.CalculateDistance(particle);

            if (distance < ConnectionDistance)
            {
                int alphaValue = CalculateConnectionAlpha(distance);

                _connectionPen.Color = Color.FromArgb(alphaValue, Color.Cyan);
                g.DrawLine(_connectionPen, _mousePosition.X, _mousePosition.Y, particle.X, particle.Y);
            }
        }
    }

    private void DrawParticles(Graphics g)
    {
        foreach (var particle in _particles)
        {
            float flickerIntensity = (MathF.Sin(particle.GlowPhase) + 1) / 2;
            int alphaValue = (int)(flickerIntensity * MaxAlphaValue);

            Color newAlpha = Color.FromArgb(alphaValue, particle.Color);

            _particleBrush.Color = newAlpha;
            g.FillEllipse(_particleBrush,
                particle.X - particle.Size / 2,
                particle.Y - particle.Size / 2,
                particle.Size,
                particle.Size);
        }
    }

    private static int CalculateConnectionAlpha(float distance)
    {
        float alpha = (ConnectionDistance - distance) / ConnectionDistance;
        int alphaValue = (int)(alpha * MaxAlphaValue);
        return alphaValue;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;

        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        DrawConnections(graphics);
        DrawMouseConnections(graphics);
        DrawParticles(graphics);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        _mousePosition = e.Location;

        foreach (var particle in _particles)
        {
            float distance = new Particle { X = e.X, Y = e.Y }.CalculateDistance(particle);

            if (distance < MouseInfluenceRadius)
            {
                float force = (ConnectionDistance - distance) / ConnectionDistance * MouseAttractionStrength;
                float dx = e.X - particle.X;
                float dy = e.Y - particle.Y;

                float length = MathF.Sqrt(dx * dx + dy * dy);

                if (length > 0)
                {
                    dx /= length;
                    dy /= length;

                    particle.VelocityX += dx * force;
                    particle.VelocityY += dy * force;
                }
            }
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        _particles.Add(CreateRandomParticle(e.X, e.Y));
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _animationTimer?.Stop();
        _animationTimer?.Dispose();
        _connectionPen?.Dispose();
        _particleBrush?.Dispose();

        base.OnFormClosed(e);
    }

    private static Color GenerateRandomColor()
        => Color.FromArgb(Random.Shared.Next(0, 256), Random.Shared.Next(0, 256), Random.Shared.Next(0, 256));
}
