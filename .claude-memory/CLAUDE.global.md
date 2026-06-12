# 全域偏好設定

## 回覆規則

- 一律使用**繁體中文**回覆，包含說明、建議與程式碼註解
- 回答簡潔，不加多餘摘要或廢話
- 不在專案根目錄建立任何說明文件（CLAUDE.md、README 等），除非使用者明確要求

## 使用者資訊

- 姓名：Yuchen Chou（GitHub：Yuch3nnn）
- 學歷：國立東華大學資訊管理學系
- 背景：軟體工程師，長期從事企業系統與程式開發（WPF、Oracle PL/SQL、Angular、Teams、MSAL）
- 生活：有澳洲 Perth 相關生活背景

## 程式碼規則

- MSAL 驗證：瀏覽器直接訪問一律使用 Redirect 模式，不改為 Popup
- Teams 整合：`isTeamsWeb` 參數邏輯必須保留，不可移除或簡化
- 驗證架構需同時支援 Teams Desktop、Teams Web、Browser 三種環境
