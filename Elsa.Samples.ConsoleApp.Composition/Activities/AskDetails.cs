using Elsa.Extensions;
using Elsa.Samples.ConsoleApp.Composition.Models;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;

namespace Elsa.Samples.ConsoleApp.Composition.Activities;

public class AskDetails : Composite<Person>
{
    private readonly Variable<string> _name = new();
    private readonly Variable<int> _age = new();

    public Input<string> NamePrompt { get; set; } = new("Please tell me your name:");
    public Input<string> AgePrompt { get; set; } = new("Please tell me your age:");

    public AskDetails()
    {
        Variables = new List<Variable> { _name, _age };
        Root = new Sequence
        {
            Activities =
            {
                new AskName
                {
                    Prompt = NamePrompt,
                    Result = new (_name)
                },
                new AskAge
                {
                    Prompt = AgePrompt,
                    Result = new (_age)
                }
            }
        };
    }

    protected override void OnCompleted(ActivityCompletedContext context)
    {
        var name = _name.Get<string>(context.TargetContext)!;
        var age = _age.Get<int>(context.TargetContext);
        var person = new Person(name, age);

        context.TargetContext.Set(Result, person);
    }
}