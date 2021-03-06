﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EFBot.Shared.Scaffolding.Converters
{
    internal sealed class ObjectToVisibilityConverter : IValueConverter
    {
        public Visibility? TrueValue { get; set; }

        public Visibility? FalseValue { get; set; }

        public object CompareTo { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = object.Equals(value, CompareTo)
                ? TrueValue
                : FalseValue;
            return result ?? Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}