using System;
using System.Windows;
using System.Windows.Controls;

namespace RaysVideoMixer.Controls
{
    public partial class RangeSlider : UserControl
    {
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Bubble, typeof (RoutedPropertyChangedEventHandler<double[]>), typeof (RangeSlider));
        
        public RangeSlider()
        {
            InitializeComponent();
            
            this.Loaded += RangeSlider_Loaded;
        }
        
        void RangeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            UpperSlider.ValueChanged += UpperSlider_ValueChanged;
        }
        
        public event RoutedPropertyChangedEventHandler<double[]> ValueChanged
        {
            add
            {
                this.AddHandler(RangeSlider.ValueChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(RangeSlider.ValueChangedEvent, value);
            }
        }
        
        private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpperSlider.Value = Math.Max(UpperSlider.Value, LowerSlider.Value);
            this.OnValueChanged(new double[]{ e.OldValue, UpperValue }, new double[]{ e.NewValue, UpperValue });
        }

        private void UpperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LowerSlider.Value = Math.Min(UpperSlider.Value, LowerSlider.Value);
            this.OnValueChanged(new double[]{ LowerValue, e.OldValue }, new double[]{ LowerValue, e.NewValue});
        }
        
        protected virtual void OnValueChanged(double[] oldValue, double[] newValue)
        {
            var changedEventArgs = new RoutedPropertyChangedEventArgs<double[]>(oldValue, newValue) {RoutedEvent = RangeSlider.ValueChangedEvent};
            this.RaiseEvent((RoutedEventArgs) changedEventArgs);
        }
        
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }

        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double UpperValue
        {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }

        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));
    }
}
