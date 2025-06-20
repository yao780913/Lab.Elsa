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

        var startProcessEndpoint = new Sequence
        {
            Name = "Start Process",
            Activities =
            {
                // 建立員工帳號 httpEndpoint
                new HttpEndpoint
                {
                    Name = "Onboarding Endpoint",
                    Path = new ("onboarding"),
                    SupportedMethods = new ([HttpMethods.Post]),
                    CanStartWorkflow = true,
                    ParsedContent = new (employeeVariable)
                },
                // 取得 correlationId 並存到變數
                new SetVariable
                {
                    Name = "Set WorkflowInstanceId",
                    Variable = correlationIdVar,
                    Value = new(context => context.GetWorkflowExecutionContext().CorrelationId)
                },
                // 回傳 WorkflowInstanceId 給 client
                new WriteHttpResponse
                {
                    Name = "return WorkflowInstanceId",
                    Content = new(context => $"{{ \"workflowInstanceId\": \"{context.GetWorkflowExecutionContext().Id}\" }}"),
                    ContentType = new("application/json")
                }
            }
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
        var addToFDFlow = new Sequence { Name = "新增帳號至 FD Flow" };
        
        addToFDFlow.Activities.Add(new Inline(_ =>
        {
            var random = new Random();
            if (random.Next(0, 100) < 50)
            {
                throw new Exception("Random failure when adding user to FD system!");
            }
            
            return default;
        })
        {
            Name = "新增帳號至 FD: Step1",
        });
        
        var addToPY = new WriteLine("新增帳號至 PY") { Name = "新增帳號至 PY" };
        var notifyApprove = new WriteLine("Review: Approve") { Name = "Review: Approve" };
        var notifyReject = new WriteLine("Review: Reject") { Name = "Review: Reject" };
        var finishSuccess = new Finish { Name = "流程結果 - 成功" };
        var failure = new Fault { Name = "流程結果 - 失敗" };

        var reviewDecision = new FlowDecision(context => reviewResultVariable.Get(context)) { Name = "Review Result" };

        builder.Root = new Flowchart
        {
            Activities =
            {
                startProcessEndpoint,
                createAccount,
                reviewEndpoint,
                reviewDecision,
                notifyApprove,
                notifyReject,
                addToFDFlow,
                addToPY,
                finishSuccess,
                failure
            },
            Connections =
            {
                new(new Endpoint(startProcessEndpoint), new Endpoint(createAccount)),
                new(new Endpoint(createAccount), new Endpoint(reviewEndpoint)),
                new(new Endpoint(reviewEndpoint), new Endpoint(reviewDecision)),
                new(new Endpoint(reviewDecision, "True"), new Endpoint(notifyApprove)),
                new(new Endpoint(notifyApprove), new Endpoint(addToFDFlow)),
                new(new Endpoint(notifyApprove), new Endpoint(addToPY)),
                new(new Endpoint(addToFDFlow), new Endpoint(finishSuccess)),
                new(new Endpoint(addToPY), new Endpoint(finishSuccess)),
                new(new Endpoint(reviewDecision, "False"), new Endpoint(notifyReject)),
                new(new Endpoint(notifyReject), new Endpoint(failure))
            }
        };
        
        // 設定每個 activity 的 metadata 位置，讓流程圖整齊
        // 主流程橫向排列在 y = 300
        SetDesignerMetadata(startProcessEndpoint,    100, 300);
        SetDesignerMetadata(createAccount,           400, 300);
        SetDesignerMetadata(reviewEndpoint,          700, 300);
        SetDesignerMetadata(reviewDecision,         1000, 300);
        SetDesignerMetadata(notifyApprove,          1300, 300);
        SetDesignerMetadata(addToFDFlow,            1600, 300);
        SetDesignerMetadata(addToPY,                1600, 200);
        SetDesignerMetadata(finishSuccess,          1900, 300);
        SetDesignerMetadata(notifyReject,           1300, 400);
        SetDesignerMetadata(failure,                1600, 400);
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
