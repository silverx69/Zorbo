using System;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;

using Zorbo.Core.Models;

namespace Zorbo.Server.WPF
{
    public class Help : ModelBase
    {
#pragma warning disable IDE0044 // Add readonly modifier
        bool isopen = false;
        string helptext = null;
        UIElement control = null;
#pragma warning restore IDE0044 // Add readonly modifier

        public bool IsOpen {
            get { return isopen; }
            set { OnPropertyChanged(() => isopen, value); }
        }

        public string Text {
            get { return helptext; }
            set { OnPropertyChanged(() => helptext, value); }
        }

        public UIElement Control {
            get { return control; }
            set { OnPropertyChanged(() => control, value); }
        }

        public static void SetHelpText(DependencyObject target, string text) {
            target.SetValue(HelpTextProperty, text);
        }

        public static string GetHelpText(DependencyObject target) {
            return (string)target.GetValue(HelpTextProperty);
        }


        private static void HelpTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            SetHelpText(obj, e.NewValue as string);
        }


        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.RegisterAttached(
            "HelpText",
            typeof(string),
            typeof(Help),
            new FrameworkPropertyMetadata(HelpTextChanged));
    }
}
