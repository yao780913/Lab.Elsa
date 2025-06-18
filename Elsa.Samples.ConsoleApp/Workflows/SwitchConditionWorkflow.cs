using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Memory;

namespace Elsa.Samples.ConsoleApp.Workflows;

public class SwitchConditionWorkflow : WorkflowBase
{
    protected override void Build (IWorkflowBuilder builder)
    {
        var inputVariable = new Variable<string>();
        
        builder.Root = new Sequence
        {
            Variables = { inputVariable }, 
            Activities =
            {
                new WriteLine("Please enter a color (red, green, or blue):"),
                new ReadLine(inputVariable),
                new Switch
                {
                    Cases =
                    {
                        new SwitchCase("red", context => inputVariable.Get(context) == "red", new WriteLine("You are an adult.")),
                        new SwitchCase("green", context => inputVariable.Get(context) == "green", new WriteLine("You are a teenager.")),
                        new SwitchCase("blue", context => inputVariable.Get(context) == "blue", new WriteLine("You are a minor.")),
                        new SwitchCase("default", context => true, new WriteLine("Unknown color!"))
                    }
                },
                new WriteLine("Come again!")
            }
        };
    }
}