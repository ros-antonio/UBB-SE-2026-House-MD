using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Repositories
{
    public interface ITriageParametersRepository
    {
        void Add(Triage_Parameters parameters);
        Triage_Parameters? GetByTriageId(int triageId);
        void Delete(Triage_Parameters parameters);
    }
}
