using System;
using System.Windows.Controls;

namespace IrssiNotifier.Components
{
	public class DiscreteSlider : Slider
	{
		private bool _busy;
		private double _discreteValue;

		protected override void OnValueChanged(double oldValue, double newValue)
		{
			if (!_busy)
			{
				_busy = true;
				if (SmallChange.CompareTo(0) != 0)
				{
					var newDiscreteValue = (int) (Math.Round(newValue/SmallChange))*SmallChange;
					if (newDiscreteValue.CompareTo(Value) != 0)
					{
						Value = newDiscreteValue;
						base.OnValueChanged(_discreteValue, newDiscreteValue);
						_discreteValue = newDiscreteValue;
					}
				}
				else
				{
					base.OnValueChanged(oldValue, newValue);
				}
				_busy = false;
			}
		}
	}
}
