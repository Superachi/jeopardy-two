using System.Collections.Generic;

namespace JeopardyTwo.Models;

public class CategoryModel
{
    public string Name { get; set; } = string.Empty;
    public List<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
}