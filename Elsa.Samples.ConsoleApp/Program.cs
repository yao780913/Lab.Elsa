using Elsa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Elsa.Workflows;
using ElsaConsole.Workflows;

// 设置服务容器。
var services = new ServiceCollection();

// 将 Elsa 服务添加到容器。
services.AddElsa(
    elsa => elsa.UseHttp());

// 构建服务容器。
var serviceProvider = services.BuildServiceProvider();

// 解析工作流运行器以执行活动。
var workflowRunner = serviceProvider.GetRequiredService<IWorkflowRunner>();

// await workflowRunner.RunAsync<HelloWorldWorkflow>();
// await workflowRunner.RunAsync<IfConditionWorkflow>();
// await workflowRunner.RunAsync<OutboundHttpRequestsWorkflow>();
// await workflowRunner.RunAsync<SwitchConditionWorkflow>();
await workflowRunner.RunAsync<FlowDecisionWorkflow>();