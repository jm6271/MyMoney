using MyMoney.Core.FS.Models;

namespace MyMoney.Core.Database
{
    /// <summary>
    /// Represents the collection of budgets in the database
    /// </summary>
    public class BudgetCollection
    {
        private const string BudgetCollectionName = "Budgets";

        // Load budgets

        /// <summary>
        /// A list of the budgets currently in the database
        /// </summary>
        public List<Budget> Budgets { get; } = [];

        public BudgetCollection(IDatabaseReader databaseReader)
        {
            var budgetsInDatabase = databaseReader.GetCollection<Budget>(BudgetCollectionName);
            if (budgetsInDatabase != null)
            {
                Budgets.AddRange(budgetsInDatabase);
            }
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

            foreach (var budget in Budgets.Where(budget => budget.BudgetTitle == GetCurrentBudgetName()))
            {
                return budget;
            }

            throw new BudgetNotFoundException("No budget exists for current month");
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
        private static string GetCurrentBudgetName()
        {
            return DateTime.Today.ToString("MMMM, yyyy");
        }

        /// <summary>
        /// An error that occurs when a budget is not found
        /// </summary>
        private class BudgetNotFoundException(string message) : Exception(message)
        {
        }
    }
}
