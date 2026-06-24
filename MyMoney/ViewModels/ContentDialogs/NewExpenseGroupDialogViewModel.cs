using System.ComponentModel.DataAnnotations;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewExpenseGroupDialogViewModel : ObservableValidator
    {
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Group name is required.")]
        [CustomValidation(typeof(NewExpenseGroupDialogViewModel), nameof(ValidateGroupName))]
        private string _groupName = string.Empty;

        public void Validate()
        {
            ValidateAllProperties();
        }

        public static ValidationResult? ValidateGroupName(object? value, ValidationContext context)
        {
            var groupName = value as string;

            if (string.IsNullOrWhiteSpace(groupName))
            {
                return new ValidationResult("Group name is required.");
            }

            return ValidationResult.Success;
        }
    }
}
