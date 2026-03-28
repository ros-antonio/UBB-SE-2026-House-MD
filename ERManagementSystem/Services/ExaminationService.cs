using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    public class ExaminationServices
    {
        private readonly ExaminationRepository _examRepository;
        private readonly ERVisitRepository _erVisitRepository;
        private readonly TriageRepository _triageRepository;
        private readonly MockStaffService _mockStaffService;
        private readonly StateManagementService _stateManagementService;

        public ExaminationService(
            ExaminationRepository examRepository,
            ERVisitRepository erVisitRepository,
            TriageRepository triageRepository,
            MockStaffService mockStaffService,
            StateManagementService stateManagementService)
        {
            _examRepository = examRepository;
            _erVisitRepository = erVisitRepository;
            _triageRepository = triageRepository;
            _mockStaffService = mockStaffService;
            _stateManagementService = stateManagementService;
        }
        
        // Retrieves Doctor_ID from MockStaffService 
        public int RequestDoctor(int visitID)
        {
            var triage = _triageRepository.GetByVisitId(visitID);

            if (triage == null)
                throw new Exception($"Triage record not found for visit {visitID}");

            int assignedDoctorId = _mockStaffService.RequestDoctor(
                triage.Specialization, 
                triage.TriageParameters);

            _stateManagementService.UpdateVisitStatus(visitID, "WAITING_FOR_DOCTOR");

            return assignedDoctorID;
        }

        // save examination in database
        public void SaveExamination(Examination exam)
        {
            _examRepository.Add(exam);
        }

        
        public Examination getExaminationByVisit(int visitID)
        {
            _examRepository.GetByVisitId(visitID);
        }
    }
}
