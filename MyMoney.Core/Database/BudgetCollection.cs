using MyMoney.Core.Models;

namespace MyMoney.Core.Database
{
    /// <summary>
    /// Represents the collection of budgets in the database
    /// </summary>
    public class BudgetCollection
    {
        private const string BudgetCollectionName = "Budgets";

        private readonly IDatabaseManager _databaseManager;

        /// <summary>
        /// A list of the budgets currently in the database
        /// </summary>
        public List<Budget> Budgets { get; set; } = [];

        public BudgetCollection(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
            var budgetsInDatabase = _databaseManager.GetCollection<Budget>(BudgetCollectionName);
            if (budgetsInDatabase != null)
            {
                foreach (var budget in budgetsInDatabase)
                {
                    if (budget != null)
                    {
                        foreach (var incomeItem in budget.BudgetIncomeItems)
                        {
                            incomeItem.Category ??= "";
                        }

                        foreach (var expenseItem in budget.BudgetExpenseItems)
                        {
                            expenseItem.CategoryName ??= "";
                        }

                        Budgets.Add(budget);
                    }
                }
            }
        }

        /// <summary>
        /// Save the budgets collection to the database
        /// </summary>
        public void SaveBudgetCollection()
        {
            _databaseManager.WriteCollection(BudgetCollectionName, Budgets);
        }

        /// <summary>
        /// Get the budget for the current month
        /// </summary>
        /// <returns>A Budget for the current month</returns>
        /// <exception cref="BudgetNotFoundException">Throws this when no budget for this month is found</exception>
        public Budget GetCurrentBudget()
        {
            if (!DoesCurrentBudgetExist())
            {
                throw new BudgetNotFoundException("No budget exists for current month");
            }

            var budget = Budgets.FirstOrDefault(budget => budget.BudgetTitle == GetCurrentBudgetName());
            return budget ?? throw new BudgetNotFoundException("No budget exists for current month");
        }

        /// <summary>
        /// Get the index of the budget for the current month
        /// </summary>
        /// <returns>The index of the budget for the current month</returns>
        /// <exception cref="BudgetNotFoundException">Throws this when no budget for this month is found</exception>
        public int GetCurrentBudgetIndex()
        {
            if (!DoesCurrentBudgetExist())
            {
                throw new BudgetNotFoundException("No budget exists for current month");
            }

            for (int i = 0; i < Budgets.Count; i++)
            {
                if (Budgets[i].BudgetTitle == GetCurrentBudgetName())
                    return i;
            }

            throw new BudgetNotFoundException("No budget exists for the current month");
        }

        /// <summary>
        /// Check to see if a budget exists for the current month
        /// </summary>
        /// <returns>True if a budget exists, false otherwise</returns>
        public bool DoesCurrentBudgetExist()
        {
            var budgetName = GetCurrentBudgetName();

            // search through budget collection for a budget with this name
            return Budgets.Any(budget => budget.BudgetTitle == budgetName);
        }

        /// <summary>
        /// Get the title for this month's budget. This works even if there is no budget for this month
        /// </summary>
        /// <returns>A string with the title of this month's budget</returns>
        public static string GetCurrentBudgetName()
        {
            return DateTime.Today.ToString("MMMM, yyyy");
        }

        /// <summary>
        /// An error that occurs when a budget is not found
        /// </summary>
        public class BudgetNotFoundException(string message) : Exception(message) { }
    }
}
