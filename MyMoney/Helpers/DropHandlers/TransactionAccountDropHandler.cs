using GongSolutions.Wpf.DragDrop;
using MyMoney.Core.Models;
using MyMoney.ViewModels.Pages;

namespace MyMoney.Helpers.DropHandlers;

public sealed class TransactionAccountDropHandler(AccountsViewModel viewModel) : IDropTarget
{
    private readonly AccountsViewModel _viewModel = viewModel;

    public void DragOver(IDropInfo dropInfo)
    {
        if (IsValidDrop(dropInfo, out _, out _))
        {
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
        }
        else
        {
            dropInfo.Effects = DragDropEffects.None;
        }
    }

    public async void Drop(IDropInfo dropInfo)
    {
        if (!IsValidDrop(dropInfo, out var transaction, out var destinationAccount))
            return;

        await _viewModel.MoveTransactionAsync(transaction, destinationAccount);
    }

    private static bool IsValidDrop(
        IDropInfo dropInfo,
        out Transaction transaction,
        out Account destinationAccount
    )
    {
        transaction = null!;
        destinationAccount = null!;

        if (dropInfo.Data is not Transaction draggedTransaction)
            return false;

        if (dropInfo.TargetItem is not Account targetAccount)
            return false;

        if (draggedTransaction.AccountId == targetAccount.Id)
            return false;

        transaction = draggedTransaction;
        destinationAccount = targetAccount;
        return true;
    }
}
