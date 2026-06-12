---
name: eip-permission-schema
description: EIP 專案權限資料表實際 DDL，建在 Invoice DB（InvoceConnectionString），無 SYS_Action 獨立表
metadata: 
  node_type: memory
  type: project
  originSessionId: bfc77c6d-7a5e-40ca-8439-896c6826a7e8
---

# EIP 權限資料表結構（Invoice.dbo）

**資料庫**：Invoice（連線字串 key = `InvoceConnectionString`，注意拼字 Invoce 非 Invoice）

## 與設計差異
- `SYS_Module` 無 `ParentID`（不支援子模組層級）
- 無獨立 `SYS_Action` 表，`ActionID` / `ActionName` 直接放在 `SYS_Permission`
- `SYS_Permission` 含 `ModuleID`、`ActionID`、`ActionName` 三欄

## 資料表

### SYS_Module
| 欄位 | 型別 | 說明 |
|------|------|------|
| ModuleID | varchar(20) PK | 模組代碼 |
| ModuleName | nvarchar(50) | 顯示名稱 |
| SortOrder | int | 排序 |
| IsActive | bit | 是否啟用 |

### SYS_Permission
| 欄位 | 型別 | 說明 |
|------|------|------|
| PermissionID | int IDENTITY PK | 流水號 |
| ModuleID | varchar(20) FK→SYS_Module | 模組 |
| ActionID | varchar(20) | 動作代碼（如 VIEW、STAMP、UPLOAD） |
| ActionName | varchar(20) | 動作顯示名稱 |
| UNIQUE | (ModuleID, ActionID) | 聯合唯一 |

### SYS_Role
| 欄位 | 型別 | 說明 |
|------|------|------|
| RoleID | int IDENTITY PK | 流水號 |
| RoleName | nvarchar(50) | 角色名稱 |
| Description | nvarchar(200) NULL | 說明 |
| IsActive | bit | 是否啟用 |

### SYS_RolePermission（中介表）
| 欄位 | 型別 | 說明 |
|------|------|------|
| RoleID | int PK+FK→SYS_Role | 角色 |
| PermissionID | int PK+FK→SYS_Permission | 權限項目 |

### SYS_UserRole（中介表）
| 欄位 | 型別 | 說明 |
|------|------|------|
| UserID | varchar(20) PK | 員工代號 |
| RoleID | int PK+FK→SYS_Role | 角色 |
| GrantedBy | varchar(20) NULL | 授權人 |
| GrantedDT | datetime | 授權時間 |

### SYS_UserPermission（個人例外）
| 欄位 | 型別 | 說明 |
|------|------|------|
| UserID | varchar(20) PK | 員工代號 |
| PermissionID | int PK+FK→SYS_Permission | 權限項目 |
| IsGranted | bit | 1=額外開放，0=強制封鎖 |
| GrantedBy | varchar(20) NULL | 操作人 |
| GrantedDT | datetime | 操作時間 |

## 實際資料中的 ModuleID / ActionID（程式常數對應）

| 常數名稱 | 實際值 | 說明 |
|---------|--------|------|
| MOD_CONTACTS | SYS00001 | 通訊錄模組 |
| ACT_CONTACTS_VIEW | SYS00001_1 | 通訊錄/檢視 |
| MOD_STAMP | SYS00002 | 電子簽章模組（待建立 DB 資料） |
| ACT_STAMP_VIEW | SYS00002_1 | 電子簽章/檢視（待建立） |
| ACT_STAMP_EXECUTE | SYS00002_2 | 電子簽章/選檔＋蓋章（共用同一權限，待建立） |

Admin 角色（RoleName='Admin', IsActive=1）bypass 所有 Permission 檢查，直接全開。

## 判斷邏輯
1. Admin 角色 bypass（RoleName='Admin', IsActive=1）→ true
2. 查 SYS_UserRole + SYS_RolePermission + SYS_Role(IsActive=1)：有結果 → true，否則 false

SYS_UserPermission（個人例外/封鎖）已棄用：人員離職從源頭刪除帳號，無需封鎖機制。
