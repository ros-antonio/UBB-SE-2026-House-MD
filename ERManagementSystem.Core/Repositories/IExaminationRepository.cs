using System.Collections.Generic;
using ERManagementSystem.Core.Models;

namespace ERManagementSystem.Core.Repositories
{
    public interface IExaminationRepository
    {
        void Add(Examination exam);
        List<Examination> GetByPatientId(string patientId);
        void UpdateNotes(int examId, string notes);
        ExaminationSummaryDTO? GetExaminationSummary(int examId);
        int GetFirstRoomId();
    }
}
