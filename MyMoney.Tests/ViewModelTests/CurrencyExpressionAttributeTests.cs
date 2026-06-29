using System.ComponentModel.DataAnnotations;
using MyMoney.ValidationAttributes;

namespace MyMoney.Tests.ViewModelTests
{
    [TestClass]
    public class CurrencyExpressionAttributeTests
    {
        [TestMethod]
        public void GetValidationResult_AllowsValidCurrencyExpression()
        {
            var attribute = new CurrencyExpressionAttribute { AllowNegative = false };

            var result = attribute.GetValidationResult("$1,200 / 2", CreateValidationContext());

            Assert.AreEqual(ValidationResult.Success, result);
        }

        [TestMethod]
        public void GetValidationResult_ReturnsParserErrorForInvalidExpression()
        {
            var attribute = new CurrencyExpressionAttribute();

            var result = attribute.GetValidationResult("not currency", CreateValidationContext());

            Assert.IsNotNull(result);
            Assert.AreNotEqual(attribute.InvalidExpressionErrorMessage, result.ErrorMessage);
        }

        [TestMethod]
        public void GetValidationResult_RejectsNegativeExpressionWhenConfigured()
        {
            var attribute = new CurrencyExpressionAttribute { AllowNegative = false };

            var result = attribute.GetValidationResult("10 - 25", CreateValidationContext());

            Assert.IsNotNull(result);
            Assert.AreEqual(attribute.NegativeErrorMessage, result.ErrorMessage);
        }

        private static ValidationContext CreateValidationContext()
        {
            return new ValidationContext(new object());
        }
    }
}
