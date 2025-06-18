using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Memory;

namespace Elsa.Samples.ConsoleApp.Workflows;

public class IfConditionWorkflow : WorkflowBase
{
    protected override void Build (IWorkflowBuilder builder)
    {
        var ageVariable = new Variable<int>();
        
        builder.Root = new Sequence
        {
            // Register the variable.
            Variables = { ageVariable }, 
    
            // Setup the sequence of activities to run.
            Activities =
            {
                new WriteLine("Please tell me your age:"), 
                new ReadLine(ageVariable), // Stores user input into the provided variable.,
                new If
                {
                    // If aged 18 or up, beer is provided, soda otherwise.
                    Condition = new (context => ageVariable.Get<int>(context) < 18),
                    Then = new WriteLine("Enjoy your soda!"),
                    Else = new WriteLine("Enjoy your beer!")
                },
                new WriteLine("Come again!")
            }
        };
    }
}