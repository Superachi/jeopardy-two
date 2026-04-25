using System.Collections.Generic;

namespace Models.Commands;

public class CommandSpec
{
    public virtual string Name => "name";
    public virtual string Description => "description";
    public virtual List<string> Arguments => new List<string>();
    public virtual void Execute(params string[] args)
    {
        // Default implementation does nothing. Override in derived classes.
    }
}