using GongSolutions.Wpf.DragDrop;
using MyMoney.Core.Models;
using MyMoney.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Helpers.DropHandlers
{
    public class BudgetExpenseItemMoveAndReorderHandler(BudgetViewModel viewModel) : IDropTarget
    {
        private readonly BudgetViewModel _viewModel = viewModel;

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.SourceCollection is IList<BudgetItem> &&
                dropInfo.TargetCollection is IList<BudgetItem>
                && !ReferenceEquals(dropInfo.DragInfo.SourceCollection, _viewModel.CurrentBudget?.BudgetIncomeItems))
            {
                dropInfo.Effects = DragDropEffects.Move;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }
            else
            {
                dropInfo.Effects = DragDropEffects.None;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.SourceCollection is not IList<BudgetItem> sourceList||
                dropInfo.TargetCollection is not IList<BudgetItem> targetList||
                dropInfo.Data is not BudgetItem item ||
                ReferenceEquals(dropInfo.DragInfo.SourceCollection, _viewModel.CurrentBudget?.BudgetIncomeItems))
                return;

            if (ReferenceEquals(sourceList, targetList)) // Reordering within the same collection
            {
                int oldIndex = sourceList.IndexOf(item);
                int newIndex = dropInfo.InsertIndex;

                // If dragging downward in the same list, the removal shifts the target index down by one
                if (oldIndex < newIndex) newIndex--;

                sourceList.RemoveAt(oldIndex);
                targetList.Insert(newIndex, item);
            }
            else // Moving to a different collection
            {
                sourceList.Remove(item);
                targetList.Insert(dropInfo.InsertIndex, item);
            }

            _viewModel.WriteToDatabase();
        }
    }
}
