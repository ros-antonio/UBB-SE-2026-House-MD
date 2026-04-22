using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Services
{
    public interface IExaminationService
    {
        int RequestDoctor(int visitId);
        void SaveExamination(Examination examination);
    }
}
