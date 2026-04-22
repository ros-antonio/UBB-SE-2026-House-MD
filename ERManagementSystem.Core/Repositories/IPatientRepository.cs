using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Repositories
{
    public interface IPatientRepository
    {
        void Add(Patient patient);
        Patient? GetById(string id);
        void Update(Patient patient);
        void Delete(Patient patient);
    }
}
