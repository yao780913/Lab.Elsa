using System.Net;
using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;

namespace ElsaWeb.Workflows;

public class HelloWorldHttpWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        builder.Root = new Sequence
        {
            Activities =
            {
                new HttpEndpoint
                {
                    Path = new("/hello-world"),
                    SupportedMethods = new([HttpMethods.Get]),
                    CanStartWorkflow = true
                },
                new WriteHttpResponse
                {
                    StatusCode = new(HttpStatusCode.OK),
                    Content = new("Hello world!")
                }
            }
        };
    }

    
    // protected override void Build(IWorkflowBuilder builder)
    // {
    //     var queryStringsVariable = builder.WithVariable<IDictionary<string, object>>();
    //     var messageVariable = builder.WithVariable<string>();
    //
    //     builder.Root = new Sequence
    //     {
    //         Activities =
    //         {
    //             new HttpEndpoint
    //             {
    //                 Path = new("/hello-world"),
    //                 CanStartWorkflow = true,
    //                 SupportedMethods = new([HttpMethods.Get]),
    //                 QueryStringData = new(queryStringsVariable)
    //             },
    //             new SetVariable
    //             {
    //                 Variable = messageVariable,
    //                 Value = new(context =>
    //                 {
    //                     var queryStrings = queryStringsVariable.Get(context)!;
    //                     var message = queryStrings.TryGetValue("message", out var messageValue) ? messageValue.ToString() : "Hello world of HTTP workflows!";
    //                     return message;
    //                 })
    //             },
    //             new WriteHttpResponse
    //             {
    //                 Content = new(messageVariable)
    //             }
    //         }
    //     };
    // }
}