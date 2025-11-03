namespace MyMoney.Tests.ModelTests
{
    [TestClass]
    public class CurrencyTests
    {
        [TestMethod]
        public void TestCurrencyToString()
        {
            Core.Models.Currency currency = new(25.99m);

            Assert.AreEqual("$25.99", currency.ToString());

            currency = new(-47.75m);

            Assert.AreEqual("-$47.75", currency.ToString());
        }
    }
}
