using Achi.Godot.PathResolving.Attributes;
using Godot;

namespace Achi.Godot.Nodes.PlayerData;

[SceneFile]
public partial class ScoreTally : Panel
{
    private Label _nameLabel = null!;
    private Label _titleLabel = null!;
    private Label _scoreLabel = null!;
    private int _score;

    public override void _Ready()
    {
        Name = $"{nameof(ScoreTally)}_UnnamedPlayer";

        _nameLabel = GetNode<Label>("NameLabel");
        _titleLabel = GetNode<Label>("TitleLabel");
        _scoreLabel = GetNode<Label>("ScoreLabel");
    }

    public void AddScore(int score)
    {
        _score += score;
        _scoreLabel.Text = _score.ToString();
    }

    public void LoadFromPlayerModel(PlayerModel playerModel)
    {
        Name = $"{nameof(ScoreTally)}_{playerModel.Name}";

        _nameLabel.Text = playerModel.Name;
        _titleLabel.Text = playerModel.Title;
        _score = 0;
        _scoreLabel.Text = _score.ToString();
    }
}
