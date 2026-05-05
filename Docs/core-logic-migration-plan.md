# MyMoney Core Logic Migration Plan

## Goal
Move non-UI business logic out of `MyMoney` (WPF app) into `MyMoney.Core` so a different app framework can reuse the same core behavior.

## Scope Rules Used
- Included: domain rules, data manipulation, calculations, cross-entity workflows, persistence orchestration.
- Excluded: dialog/window interactions, `ObservableCollection`/binding state, LiveCharts/Skia chart object construction, theme/resource access, WPF services and controls.

## Highest-Value Move Targets (From ViewModels)

### 1) `AccountsViewModel` -> `MyMoney.Core`
File: `MyMoney/ViewModels/Pages/AccountsViewModel.cs`

#### Move candidate: Account transaction workflows
- `UpdateAccountBalance(...)`
- Transfer workflow currently in `ExecuteTransfer(...)`
- Balance update transaction creation in `UpdateAccountBalance()` command

Proposed core location:
- `MyMoney.Core/Services/Accounts/AccountTransactionService.cs`
- Interface: `IAccountTransactionService`

Why:
- These are business rules about how account totals and transactions change.
- They are currently mixed with dialog + selection UI state.

Notes:
- Service should accept plain models (`Account`, `Transaction`, `Currency`) and return result objects (success/failure + mutated entities/new transactions).
- Keep persistence in core-level repository abstraction (see “Infrastructure seam” below).

#### Move candidate: Savings category side-effects of transaction add/edit/delete
- `UpdateSavingsCategory(...)`
- `TransactionOperation` enum

Proposed core location:
- `MyMoney.Core/Services/Budgets/SavingsCategoryTransactionSyncService.cs`
- Interface: `ISavingsCategoryTransactionSyncService`

Why:
- This is core budgeting behavior, not UI behavior.
- Needed regardless of front-end technology.

Notes:
- Make method explicit: `ApplyTransactionChangeToCurrentBudget(...)`.
- Return a structured result (changed/not changed + reason) for app-level messages/logging.

#### Move candidate: Transaction paging/search orchestration contract
- `GetTransactionsPage(...)`

Proposed core location:
- `MyMoney.Core/Services/Accounts/TransactionQueryService.cs`
- Interface: `ITransactionQueryService`

Why:
- Query shape (paging boundary by date/id) is domain access logic.
- UI should request a page; it should not encode pagination/filter internals.

Notes:
- Keep WPF threading (`Dispatcher`) in app project; only query composition and paging belong in core.

#### Move candidate: transaction validation rule
- `ValidateTransactionAmount(...)` rule (amount cannot overdraw account)

Proposed core location:
- `MyMoney.Core/Services/Accounts/AccountValidationService.cs`
- Or fold into `AccountTransactionService`.

Why:
- Validation rule is domain logic.
- Current method is coupled to message box display; split rule from presentation.

### 2) `BudgetViewModel` -> `MyMoney.Core`
File: `MyMoney/ViewModels/Pages/BudgetViewModel.cs`

#### Move candidate: Budget totals and derived values
- `UpdateIncomeTotals()`
- `UpdateExpenseTotals()`
- `UpdateLeftToBudget()`
- Existence checks (`DoesIncomeItemExist`, `DoesSavingsCategoryExist`, `DoesExpenseGroupExist`, `DoesExpenseItemExist`)

Proposed core location:
- `MyMoney.Core/Services/Budgets/BudgetComputationService.cs`
- `MyMoney.Core/Services/Budgets/BudgetValidationService.cs`

Why:
- Pure business calculations and constraints.
- Easy to unit test and framework-agnostic.

#### Move candidate: New budget creation/copy rules
- `CreateNewBudget()` cloning and carry-forward logic
- Savings carry-forward transaction generation

Proposed core location:
- `MyMoney.Core/Services/Budgets/BudgetCreationService.cs`
- Interface: `IBudgetCreationService`

Why:
- This is core month rollover behavior.
- It includes non-trivial invariants (planned transaction hash, balance transaction hash, carry-forward semantics).

Notes:
- Keep "show dialog" and month picker interactions in UI.
- Core method should take `selectedMonth` and source budget(s), return ready-to-persist `Budget`.

#### Move candidate: Savings propagation to future budgets
- `UpdateFutureSavingsCategories(...)`

Proposed core location:
- `MyMoney.Core/Services/Budgets/FutureBudgetAdjustmentService.cs`

Why:
- Cross-budget update policy is core behavior.

#### Move candidate: Apply report actuals to budget models
- `AddActualSpentToCurrentBudget()`
- `UpdateIncomeActuals(...)`
- `UpdateSavingsActuals(...)`
- `UpdateExpenseActuals(...)`

Proposed core location:
- `MyMoney.Core/Services/Budgets/BudgetActualsApplier.cs`

Why:
- Mapping report outputs back into budget entities is domain logic.

### 3) `DashboardViewModel` and `BudgetReportsViewModel` -> partial extraction
Files:
- `MyMoney/ViewModels/Pages/DashboardViewModel.cs`
- `MyMoney/ViewModels/Pages/BudgetReportsViewModel.cs`

#### Move candidate: report totals and summary metrics
- Dashboard: `GetCashFlowTotal(...)`, `GetSpendingTotal(...)`, total aggregations
- BudgetReports: `CalculateTotal(...)`, `CalculateReportTotals(...)`

Proposed core location:
- `MyMoney.Core/Reports/ReportSummaryCalculator.cs`

Why:
- Numeric/report computations are reusable in any frontend.

Do not move:
- LiveCharts `ISeries` creation, axis setup, theme color decisions, brush/resource logic.

### 4) `SettingsViewModel` -> selective extraction
File: `MyMoney/ViewModels/Pages/SettingsViewModel.cs`

Move candidate:
- Settings dictionary serialization/parsing for backup settings and theme keys

Proposed core location:
- `MyMoney.Core/Services/Settings/AppSettingsService.cs`

Do not move:
- `OpenFileDialog`, `SaveFileDialog`, `OpenFolderDialog`, `ApplicationThemeManager`, `Application.Current.Shutdown()`.

## Suggested Core Structure

- `MyMoney.Core/Services/Accounts/`
  - `IAccountTransactionService.cs`
  - `AccountTransactionService.cs`
  - `ITransactionQueryService.cs`
  - `TransactionQueryService.cs`
- `MyMoney.Core/Services/Budgets/`
  - `IBudgetCreationService.cs`
  - `BudgetCreationService.cs`
  - `BudgetComputationService.cs`
  - `BudgetValidationService.cs`
  - `BudgetActualsApplier.cs`
  - `FutureBudgetAdjustmentService.cs`
  - `ISavingsCategoryTransactionSyncService.cs`
  - `SavingsCategoryTransactionSyncService.cs`
- `MyMoney.Core/Services/Reports/`
  - `ReportSummaryCalculator.cs`
- `MyMoney.Core/Services/Settings/`
  - `AppSettingsService.cs`

## Infrastructure Seam Needed
Current view models call `IDatabaseManager` directly. To make core reusable across app frameworks and storage backends, introduce core-facing repository interfaces in `MyMoney.Core`, for example:

- `IAccountRepository`
- `ITransactionRepository`
- `IBudgetRepository`
- `ISettingsRepository`

Then have WPF app provide adapters that use existing `DatabaseManager`.

## Suggested Migration Order
1. Extract pure calculations/validation first (`BudgetComputationService`, `ReportSummaryCalculator`, account validation rule).
2. Extract workflow services next (account transaction operations, budget creation/carry-forward, savings sync).
3. Introduce repository interfaces and move persistence orchestration behind those interfaces.
4. Simplify ViewModels to orchestration + UI-only concerns (dialogs, commands, bindings, theme/chart presentation).

## Testing Strategy
- Add unit tests in `MyMoney.Tests/CoreTests` for each extracted service.
- Port existing ViewModel behavior tests toward service-level tests where possible.
- Keep a thin set of ViewModel tests focused on command wiring and UI interaction branching.

## Key Risks / Design Notes
- Category matching currently relies heavily on string values (`"Savings"`, category names). Consider central constants or stronger category identity.
- Some logic depends on mutable model collections and order/IDs (e.g., reindexing list IDs after delete). Preserve behavior in extracted services before attempting redesign.
- Ensure hash-based transaction linking (`TransactionHash`, planned/balance hashes) is covered by tests before refactoring.
