using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Services
{
    public interface IRegistrationService
    {
        ER_Visit RegisterPatientAndVisit(Patient patient, string chiefComplaint);
    }
}
