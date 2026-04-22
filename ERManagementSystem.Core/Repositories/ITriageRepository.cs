using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Repositories
{
    public interface ITriageRepository
    {
        int Add(Triage triage);
        Triage? GetByVisitId(int visitId);
        void Delete(Triage triage);
    }
}
