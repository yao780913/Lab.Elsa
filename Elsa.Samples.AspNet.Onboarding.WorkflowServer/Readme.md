# Onboarding 範例 - Workflow Server

## 使用方式

1. **設定專案**
  - 修改 `ElsaStudioBlazorWasm` 專案的 `appsettings.json`：
    - 將 `Backend Url` 設為 `Elsa.Samples.AspNet.Onboarding.WorkflowServer` 專案執行時的 URL。
1. **執行專案**
  - 啟動以下專案：
    - `Elsa.Samples.AspNet.Onboarding.WorkflowServer`
    - `ElsaStudioBlazorWasm`
1. **操作流程**
  - 開啟 `ElsaStudioBlazorWasm`，使用預設帳號密碼 `admin` / `password` 登入。
  - 在左側的 Workflow 列表中找到 `OnboardingFlowchartWorkflow`。
  - 點擊進入工作流程編輯畫面，並點擊右上角的 `Run` 按鈕執行流程。
1. **觸發工作流程**
  - 使用 `run-onboard-flow-chart-workflow.http` 檔案觸發 Onboarding 流程：
    - **建立 Onboarding**：發送員工資訊以啟動流程。
    - **Review**：使用返回的 `workflowInstanceId` 進行審核操作。

## 已知限制

- 在 C# 中定義的 `OnboardingFlowchartWorkflow`，在 Elsa Studio 中顯示時可能過於擁擠。
- 若直接在畫面上調整 Activity，可能導致部分行為異常（如無法接收回傳訊息）。
  - 以此案例來說，收不到回傳的 workflowInstanceId 

## 待解決問題

- 如何進行上板操作？
- Workflow Definition 變更是否會影響已執行的 Workflow Instance？