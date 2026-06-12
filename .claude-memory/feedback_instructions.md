---
name: feedback-instructions
description: 使用者給定的持久性指令與行為偏好
metadata:
  type: feedback
---

回覆語言：一律使用**繁體中文**。

**Why:** 使用者明確設定偏好，記錄於 2025-04-01。
**How to apply:** 所有回覆、說明、程式碼註解建議皆用繁體中文。

---

MSAL 驗證：瀏覽器直接訪問時持續使用 **Redirect 模式**，不改為 Popup。

**Why:** 使用者明確指定，記錄於 2025-07-08。
**How to apply:** 修改 Angular MSAL 相關程式碼時，browser direct access 路徑保留 Redirect。

---

Teams 整合：`isTeamsWeb` 參數邏輯必須保留並正確處理。

**Why:** 專案需求，記錄於 2025-07-14。
**How to apply:** 重構或修改 Teams 相關 Angular 程式碼時，不可移除或簡化 `isTeamsWeb` 判斷。

---

Teams 驗證架構：任何驗證方案都需同時支援 Teams Desktop、Teams Web、Browser 三種環境。

**Why:** 多次明確要求統一驗證架構，避免 `uninitialized_public_client_application` 錯誤。
**How to apply:** 設計驗證流程時，三種環境的登入路徑都要納入考量。
