using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AciteMediaApp.Pages.Controls
{
    public class DiscreteSlider : Slider
    {
        public static readonly BindableProperty DiscreteValueProperty =
            BindableProperty.Create(nameof(DiscreteValue), typeof(int), typeof(DiscreteSlider), 0);

        public int DiscreteValue
        {
            get => (int)GetValue(DiscreteValueProperty);
            set => SetValue(DiscreteValueProperty, value);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(Value))
            {
                DiscreteValue = (int)Math.Round(Value);
                Value = DiscreteValue;

                DiscreteValueChanged?.Invoke(DiscreteValue);
            }
        }



        public event Action<int>? DiscreteValueChanged;
    }
}
