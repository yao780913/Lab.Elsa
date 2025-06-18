using Elsa.Workflows;
using Elsa.Workflows.Activities;

namespace Elsa.Samples.ConsoleApp.Workflows;

public class HelloWorldWorkflow : WorkflowBase
{
    protected override void Build (IWorkflowBuilder builder)
    {
        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("Hello World!"),
                new WriteLine("We can do more than a one-liner!")
            }
        };
    }
}