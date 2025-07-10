using GongSolutions.Wpf.DragDrop;
using MyMoney.Core.Models;
using MyMoney.ViewModels.Pages;

namespace MyMoney.Helpers.DropHandlers
{
    public class BudgetExpenseGroupReorderHandler(BudgetViewModel viewModel) : IDropTarget
    {
        private readonly BudgetViewModel _viewModel = viewModel;

        public void DragOver(IDropInfo dropInfo)
        {
            // Only allow moves within the same collection
            if (ReferenceEquals(dropInfo.DragInfo.SourceCollection, dropInfo.TargetCollection))
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
            if (!ReferenceEquals(dropInfo.DragInfo.SourceCollection, dropInfo.TargetCollection))
                return;

            if (dropInfo.DragInfo.SourceCollection is not IList<BudgetExpenseCategory> sourceList ||
                dropInfo.TargetCollection is not IList<BudgetExpenseCategory> targetList ||
                dropInfo.Data is not BudgetExpenseCategory item)
                return;

            int oldIndex = sourceList.IndexOf(item);
            int newIndex = dropInfo.InsertIndex;

            // If dragging downward in the same list, the removal shifts the target index down by one
            if (oldIndex < newIndex) newIndex--;

            sourceList.RemoveAt(oldIndex);
            targetList.Insert(newIndex, item);

            _viewModel.WriteToDatabase();
        }
    }
}
