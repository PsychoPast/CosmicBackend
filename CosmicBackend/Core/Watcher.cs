using System;
using System.Collections.Generic;
using System.Timers;
using CosmicBackend.Models;

namespace CosmicBackend.Core
{
    internal class Watcher
    {
        private readonly Timer _timer;
        internal Watcher()
        {
            _timer = new Timer()
                         {
                             Interval = 1.44e+7, 
                             AutoReset = true
                         };
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach ((ulong key, DateTime value) in Singleton.Instance.RegisteredClientCredentialTokens)
            {
                if(DateTime.Now > value.AddHours(4))
                {
                    Singleton.Instance.RegisteredClientCredentialTokens.Remove(key, out _);
                }
            }

            foreach ((string key, Session value) in Singleton.Instance.RegisteredSessions)
            {
                if (DateTime.Now > value.StartTime.AddHours(8))
                {
                    Singleton.Instance.RegisteredSessions.Remove(key, out _);
                }
            }
        }
    }
}