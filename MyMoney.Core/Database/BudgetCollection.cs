using MyMoney.Core.FS.Models;

namespace MyMoney.Core.Database
{
    /// <summary>
    /// Represents the collection of budgets in the database
    /// </summary>
    public class BudgetCollection
    {
        private const string BUDGET_COLLECTION_NAME = "Budgets";

        private readonly List<Budget> _budgets;

        public BudgetCollection() 
        {
            // Load budgets
            _budgets = DatabaseReader.GetCollection<Budget>(BUDGET_COLLECTION_NAME);
        }

        /// <summary>
        /// A list of the budgets currently in the database
        /// </summary>
        public List<Budget> Budgets { get { return _budgets; } }

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

            foreach (var budget in _budgets)
            {
                if (budget.BudgetTitle == GetCurrentBudgetName())
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
            foreach (var budget in _budgets)
            {
                if (budget.BudgetTitle == budgetName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the title for this months budget. This works even if there is no budget for this month
        /// </summary>
        /// <returns>A string with the title of this months budget</returns>
        public static string GetCurrentBudgetName()
        {
            return DateTime.Today.ToString("MMMM, yyyy");
        }

        /// <summary>
        /// An error that occurrs when a budget is not found
        /// </summary>
        public class BudgetNotFoundException : Exception
        {
            public BudgetNotFoundException() : base() { }
            public BudgetNotFoundException(string message) : base(message) { }
            public BudgetNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        }
    }
}
