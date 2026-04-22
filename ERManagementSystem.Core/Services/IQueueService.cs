using System.Collections.Generic;
using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Services
{
    public interface IQueueService
    {
        List<(ER_Visit visit, Triage triage)> GetOrderedQueue();
        void RemoveFromQueue(int visitId);
    }
}
