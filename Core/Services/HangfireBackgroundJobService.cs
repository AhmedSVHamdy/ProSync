using Core.ServiceContracts;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Core.Services
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            BackgroundJob.Enqueue(methodCall);
        }
    }
}
