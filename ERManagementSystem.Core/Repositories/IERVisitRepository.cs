using System.Collections.Generic;
using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Repositories
{
    public interface IERVisitRepository
    {
        void Add(ER_Visit visit);
        List<ER_Visit> GetActiveVisits();
        void UpdateStatus(int visitId, string newStatus);
        ER_Visit? GetByVisitId(int visitId);
        List<ER_Visit> GetByStatus(string status);
        List<(ER_Visit visit, Triage triage)> GetActiveVisitsWithTriage();
    }
}
