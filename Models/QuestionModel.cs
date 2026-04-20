namespace JeopardyTwo.Models;

public class QuestionModel
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int PointValue { get; set; }
}