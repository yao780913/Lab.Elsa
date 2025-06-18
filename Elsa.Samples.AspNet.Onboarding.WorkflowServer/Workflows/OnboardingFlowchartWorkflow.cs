using Elsa.Extensions;
using Elsa.Http;
using Elsa.Samples.AspNet.Onboarding.WorkflowServer.Models;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Activities.Flowchart.Activities;
using Microsoft.AspNetCore.Http;
using Endpoint = Elsa.Workflows.Activities.Flowchart.Models.Endpoint;

namespace Elsa.Samples.AspNet.Onboarding.WorkflowServer.Workflows;

public class OnboardingFlowchartWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        builder.Name = "Onboarding Flowchart Workflow";
        
        var employeeVariable = builder.WithVariable<Employee>().WithMemoryStorage();
        var reviewResultVariable = builder.WithVariable<bool>().WithMemoryStorage();
        var correlationIdVar = builder.WithVariable<string>().WithMemoryStorage();

        // 1. 建立員工帳號 httpEndpoint
        var onboardingEndpoint = new HttpEndpoint
        {
            Path = new ("onboarding"),
            SupportedMethods = new ([HttpMethods.Post]),
            CanStartWorkflow = true,
            ParsedContent = new (employeeVariable)
        };
        // 取得 correlationId 並存到變數
        var setCorrelationId = new SetVariable
        {
            Variable = correlationIdVar,
            Value = new(context => context.GetWorkflowExecutionContext().CorrelationId)
        };
        // 回傳 WorkflowInstanceId 給 client
        var writeWorkflowInstanceId = new WriteHttpResponse
        {
            Content = new(context => $"{{ \"workflowInstanceId\": \"{context.GetWorkflowExecutionContext().Id}\" }}"),
            ContentType = new("application/json")
        };

        // 2. 新增審核 httpEndpoint
        var reviewEndpoint = new HttpEndpoint
        {
            Path = new ("review"),
            SupportedMethods = new ([HttpMethods.Post]),
            CanStartWorkflow = false,
            ParsedContent = new (reviewResultVariable)
        };

        // Activities
        var createAccount = new WriteLine("建立員工帳號");
        var addToFD = new WriteLine("新增帳號至 FD");
        var addToPY = new WriteLine("新增帳號至 PY");
        var notifySuccess = new WriteLine("通知結果 - 成功");
        var notifyFailure = new WriteLine("通知結果 - 失敗");
        var finishApproved = new Finish { Name = "審核通過" };
        var finishRejected = new Finish { Name = "審核失敗" };

        // 決策: 依 reviewResultVariable
        var reviewDecision = new FlowDecision(context => reviewResultVariable.Get(context));

        builder.Root = new Flowchart
        {
            Activities =
            {
                onboardingEndpoint,
                setCorrelationId,
                writeWorkflowInstanceId,
                createAccount,
                reviewEndpoint,
                reviewDecision,
                notifySuccess,
                notifyFailure,
                addToFD,
                addToPY,
                finishApproved,
                finishRejected
            },
            Connections =
            {
                new(new Endpoint(onboardingEndpoint), new Endpoint(setCorrelationId)),
                new(new Endpoint(setCorrelationId), new Endpoint(writeWorkflowInstanceId)),
                new(new Endpoint(writeWorkflowInstanceId), new Endpoint(createAccount)),
                new(new Endpoint(createAccount), new Endpoint(reviewEndpoint)),
                new(new Endpoint(reviewEndpoint), new Endpoint(reviewDecision)),
                new(new Endpoint(reviewDecision, "True"), new Endpoint(notifySuccess)),
                new(new Endpoint(notifySuccess), new Endpoint(addToFD)),
                new(new Endpoint(notifySuccess), new Endpoint(addToPY)),
                new(new Endpoint(addToFD), new Endpoint(finishApproved)),
                new(new Endpoint(addToPY), new Endpoint(finishApproved)),
                new(new Endpoint(reviewDecision, "False"), new Endpoint(notifyFailure)),
                new(new Endpoint(notifyFailure), new Endpoint(finishRejected))
            }
        };
    }
}
