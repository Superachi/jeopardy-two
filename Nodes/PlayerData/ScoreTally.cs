using Achi.Godot.Common;
using Achi.Godot.PathResolving.Attributes;
using Godot;

namespace Achi.Godot.Nodes.PlayerData;

[SceneFile]
public partial class ScoreTally : Panel
{
    private int _playerIndex;
    public int PlayerIndex
    {
        get => _playerIndex;
        private set
        {
            _playerIndex = value;
            Name = $"{nameof(ScoreTally)}_Player{_playerIndex}";
        }
    }

    private RichTextLabel _nameLabel = null!;
    private RichTextLabel _titleLabel = null!;
    private RichTextLabel _scoreLabel = null!;
    private RichTextLabel _indexLabel = null!;
    private GpuParticles2D _winnerSparkle = null!;

    private int _score;
    public int Score => _score;
    private float _displayScore;
    private float _displayScoreGoal;

    private string _playerName = "";
    public string PlayerName => _playerName;
    public string PlayerDisplayName => _playerName.Replace("_", " ");

    private bool _isWinning = false;
    private float _winnerEffectAlpha = 0;

    public override void _Ready()
    {
        _nameLabel = GetNode<RichTextLabel>("NameLabel");
        _titleLabel = GetNode<RichTextLabel>("TitleLabel");
        _scoreLabel = GetNode<RichTextLabel>("ScoreLabel");
        _indexLabel = GetNode<RichTextLabel>("IndexLabel");

        _winnerSparkle = GetNode<GpuParticles2D>("WinnerSparkle");
        _winnerSparkle.Modulate = new Color(1, 1, 1, _winnerEffectAlpha);
    }

    public override void _Process(double delta)
    {
        _displayScore = Mathf.Lerp(_displayScore, _displayScoreGoal, (float)delta * 2);
        _scoreLabel.Text = Mathf.RoundToInt(_displayScore).ToString();

        _winnerEffectAlpha = Mathf.MoveToward(_winnerEffectAlpha, _isWinning ? 1 : 0, (float)delta);
        _winnerSparkle.Modulate = new Color(1, 1, 1, _winnerEffectAlpha);
    }

    public void LoadFromPlayerModel(PlayerModel playerModel)
    {
        _playerName = playerModel.Name.Replace(" ", "_");
        _nameLabel.Text = PlayerDisplayName;

        _titleLabel.Text = playerModel.Title;
        _score = 0;
        _scoreLabel.Text = _score.ToString();

        ReAlignText();
    }

    public void ReAlignText()
    {
        _nameLabel.Size = Size.SetY(Size.Y * 0.4f);
        _nameLabel.Position = new Vector2(0, Size.Y * 0f);
        _titleLabel.Size = Size.SetY(Size.Y * 0.4f);
        _titleLabel.Position = new Vector2(0, Size.Y * 0.35f);
        _scoreLabel.Size = Size.SetY(Size.Y * 0.6f);
        _scoreLabel.Position = new Vector2(0, Size.Y * 0.45f);
        _indexLabel.Size = Size.SetY(Size.Y * 0.3f);
        _indexLabel.Position = new Vector2(0, -30);

        _winnerSparkle.Position = Size / 2;
        var processMat = _winnerSparkle.ProcessMaterial as ParticleProcessMaterial;
        if (processMat != null)
        {
            processMat.EmissionBoxExtents = new Vector3(Size.X / 2, Size.Y / 2, 0);
        }
    }

    public void ShowWinningEffect() => _isWinning = true;
    public void HideWinningEffect() => _isWinning = false;

    public void AddScore(int score)
    {
        _score += score;
        _displayScoreGoal = _score;
    }

    public void SetPlayerName(string name)
    {
        _playerName = name.Replace("_", " ");
        _nameLabel.Text = PlayerDisplayName;
    }

    public void SetPlayerTitle(string title)
    {
        _titleLabel.Text = title.Replace("_", " ");
    }

    public void SetIndex(int index)
    {
        _playerIndex = index;
        _indexLabel.Text = _playerIndex.ToString();
    }
}
