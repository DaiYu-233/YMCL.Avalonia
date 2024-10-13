using System;
using System.Timers;

namespace YMCL.Main.Public;

public partial class Method
{
    public static class Module
    {
        public class Debouncer
        {
            private Timer _timer;
            private Action _action;
            private double _interval = 1000;

            public Debouncer(Action action, double interval = 1000)
            {
                _action = action;
                _interval = interval;
                _timer = new Timer(_interval);
                _timer.Elapsed += OnTimerElapsed;
                _timer.AutoReset = false;
            }

            public void Trigger()
            {
                _timer.Stop();
                _timer.Start();
            }

            private void OnTimerElapsed(Object source, ElapsedEventArgs e)
            {
                _timer.Stop();
                _action?.Invoke();
            }

            public void Dispose()
            {
                _timer?.Stop();
                _timer?.Dispose();
            }
        }
    }
}