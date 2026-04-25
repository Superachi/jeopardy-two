using System.Collections.Generic;
using JeopardyTwo.Nodes.JeopardyBoard;

namespace Models.Commands;
public class CSRevealAnswer : CommandSpec
{
    public override string Name => "reveal";
    public override string Description => "Reveals the answer to the current question.";
    public override List<string> Arguments => new List<string> { };
    public override void Execute(params string[] args)
    {
        QuestionManager.RevealAnswer();
    }
}