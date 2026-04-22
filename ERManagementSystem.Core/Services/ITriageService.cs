using System.Collections.Generic;
using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Services
{
    public interface ITriageService
    {
        Triage CreateTriage(int visitId, Triage_Parameters parameters);
        Triage? GetByVisitId(int visitId);
        IReadOnlyList<ER_Visit> GetVisitsForTriage();
        void MoveVisitToQueue(int visitId);
        void CloseVisit(int visitId);
    }
}
