using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MyMoney.Helpers
{
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        /// <summary>
        /// Add many items in one shot.
        /// Raises a single collection reset notification.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            _suppressNotification = true;

            foreach (var item in items)
            {
                Items.Add(item);
            }

            _suppressNotification = false;

            // Notify view that everything changed.
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            );
        }

        /// <summary>
        /// Remove many items in one shot.
        /// Raises a single collection reset notification.
        /// </summary>
        public void RemoveRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            _suppressNotification = true;

            foreach (var item in items)
            {
                Items.Remove(item);
            }

            _suppressNotification = false;

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            );
        }

        /// <summary>
        /// Replace entire contents with the provided items.
        /// </summary>
        public void ReplaceAll(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            _suppressNotification = true;

            Items.Clear();
            AddRange(items);

            _suppressNotification = false;

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            );
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressNotification) return;
            base.OnCollectionChanged(e);
        }
    }
}
