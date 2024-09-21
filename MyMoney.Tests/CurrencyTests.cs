﻿namespace MyMoney.Tests
{
    [TestClass]
    public class CurrencyTests
    {
        [TestMethod]
        public void TestCurrencyToString()
        {
            MyMoney.Models.Currency currency = new(25.99m);

            Assert.AreEqual("$25.99", currency.ToString());
        }
    }
}
