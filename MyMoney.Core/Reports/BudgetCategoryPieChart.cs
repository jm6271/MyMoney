namespace MyMoney.Core.Reports
{
    public class BudgetCategoryPieChart
    {
        public static List<double> GetBudgetPercentages(List<double> CategoryTotals)
        {
            List<double> percentages = [];

            // get total
            var total = GetOverallTotals(CategoryTotals);

            // build a list of percentages
            foreach (var item in CategoryTotals)
            {
                percentages.Add(item / total);
            }

            return percentages;
        }

        private static double GetOverallTotals(List<double> CategoryTotals)
        {
            double total = 0;

            foreach (var item in CategoryTotals)
            {
                total += item;
            }

            return total;
        }
    }
}
