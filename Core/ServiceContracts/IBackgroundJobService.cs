using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IBackgroundJobService
    {
        void Enqueue<T>(System.Linq.Expressions.Expression<Func<T, Task>> methodCall);
    }
}
