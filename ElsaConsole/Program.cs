using Elsa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Elsa.Workflows.Activities;
using Elsa.Workflows;

// 设置服务容器。
var services = new ServiceCollection();

// 将 Elsa 服务添加到容器。
services.AddElsa();

// 构建服务容器。
var serviceProvider = services.BuildServiceProvider();

// 实例化一个活动以运行。
var activity = new Sequence
{
    Activities =
    {
        new WriteLine("Hello World!"),
        new WriteLine("We can do more than a one-liner!")
    }
};

// 解析工作流运行器以执行活动。
var workflowRunner = serviceProvider.GetRequiredService<IWorkflowRunner>();

// 执行活动。
await workflowRunner.RunAsync(activity);