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
            Name = "Onboarding Endpoint",
            Path = new ("onboarding"),
            SupportedMethods = new ([HttpMethods.Post]),
            CanStartWorkflow = true,
            ParsedContent = new (employeeVariable)
        };
        
        // 取得 correlationId 並存到變數
        var setCorrelationId = new SetVariable
        {
            Name = "Set WorkflowInstanceId",
            Variable = correlationIdVar,
            Value = new(context => context.GetWorkflowExecutionContext().CorrelationId)
        };
        // 回傳 WorkflowInstanceId 給 client
        var writeWorkflowInstanceId = new WriteHttpResponse
        {
            Name = "return WorkflowInstanceId",
            Content = new(context => $"{{ \"workflowInstanceId\": \"{context.GetWorkflowExecutionContext().Id}\" }}"),
            ContentType = new("application/json")
        };

        // 2. 新增審核 httpEndpoint
        var reviewEndpoint = new HttpEndpoint
        {
            Name = "Review Endpoint",
            Path = new ("review"),
            SupportedMethods = new ([HttpMethods.Post]),
            CanStartWorkflow = false,
            ParsedContent = new (reviewResultVariable)
        };

        // Activities
        var createAccount = new WriteLine("建立員工帳號") { Name = "建立帳號" };
        var addToFD = new WriteLine("新增帳號至 FD") { Name = "新增帳號至 FD" };
        var addToPY = new WriteLine("新增帳號至 PY") { Name = "新增帳號至 PY" };
        var notifyApprove = new WriteLine("審核結果 - 通過") { Name = "審核結果 - 通過" };
        var notifyReject = new WriteLine("審核結果 - 駁回") { Name = "審核結果 - 駁回" };
        var finishSuccess = new Finish { Name = "流程結果 - 成功" };
        var failure = new Fault { Name = "流程結果 - 失敗" };

        var reviewDecision = new FlowDecision(context => reviewResultVariable.Get(context)) { Name = "審核決策" };

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
                notifyApprove,
                notifyReject,
                addToFD,
                addToPY,
                finishSuccess,
                failure
            },
            Connections =
            {
                new(new Endpoint(onboardingEndpoint), new Endpoint(setCorrelationId)),
                new(new Endpoint(setCorrelationId), new Endpoint(writeWorkflowInstanceId)),
                new(new Endpoint(writeWorkflowInstanceId), new Endpoint(createAccount)),
                new(new Endpoint(createAccount), new Endpoint(reviewEndpoint)),
                new(new Endpoint(reviewEndpoint), new Endpoint(reviewDecision)),
                new(new Endpoint(reviewDecision, "True"), new Endpoint(notifyApprove)),
                new(new Endpoint(notifyApprove), new Endpoint(addToFD)),
                new(new Endpoint(notifyApprove), new Endpoint(addToPY)),
                new(new Endpoint(addToFD), new Endpoint(finishSuccess)),
                new(new Endpoint(addToPY), new Endpoint(finishSuccess)),
                new(new Endpoint(reviewDecision, "False"), new Endpoint(notifyReject)),
                new(new Endpoint(notifyReject), new Endpoint(failure))
            }
        };
        
        // 設定每個 activity 的 metadata 位置，讓流程圖整齊
        // 主流程橫向排列在 y = 300
        SetDesignerMetadata(onboardingEndpoint,      100, 300);
        SetDesignerMetadata(setCorrelationId,        400, 300);
        SetDesignerMetadata(writeWorkflowInstanceId, 700, 300);
        SetDesignerMetadata(createAccount,          1000, 300);
        SetDesignerMetadata(reviewEndpoint,         1300, 300);
        SetDesignerMetadata(reviewDecision,         1600, 300);
        SetDesignerMetadata(notifyApprove,          1900, 300);
        SetDesignerMetadata(addToFD,                2200, 300);
        SetDesignerMetadata(addToPY,                2200, 200);
        SetDesignerMetadata(finishSuccess,          2500, 300);
        SetDesignerMetadata(notifyReject,           1900, 400);
        SetDesignerMetadata(failure,                2200, 400);
    }

    private void SetDesignerMetadata(Activity activity, int x, int y, double width = 150, double height = 50)
    {
        activity.SetDisplayText(activity.Name ?? activity.Type);
        
        activity.Metadata["designer"] = new 
        {
            position = new { x, y },
            size = new { width, height }
        };
    }
}
