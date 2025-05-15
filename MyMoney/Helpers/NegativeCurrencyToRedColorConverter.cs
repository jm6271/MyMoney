﻿using MyMoney.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace MyMoney.Helpers
{
    public class NegativeCurrencyToRedColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Currency currency && currency.Value < 0)
            {
                return new SolidColorBrush(Color.FromArgb(255,255,0,0));
            }
            else
            {
                if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
