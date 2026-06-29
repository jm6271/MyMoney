using System.ComponentModel.DataAnnotations;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class RenameAccountViewModel : ObservableValidator
    {
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Account name is required.")]
        [CustomValidation(typeof(RenameAccountViewModel), nameof(ValidateNewName))]
        private string _newName = string.Empty;

        public void Validate()
        {
            ValidateAllProperties();
        }

        public static ValidationResult? ValidateNewName(object? value, ValidationContext context)
        {
            var name = value as string;

            if (string.IsNullOrWhiteSpace(name))
            {
                return new ValidationResult("Account name is required.");
            }

            return ValidationResult.Success;
        }
    }
}
