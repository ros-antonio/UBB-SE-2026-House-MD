using System;
using System.Collections.Generic;
using System.Linq;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using Microsoft.Data.SqlClient;

namespace ERManagementSystem.Services
{
    internal class QueueService
    {
        private readonly ERVisitRepository _visitRepository;
        private readonly TriageRepository _triageRepository;

        public QueueService(ERVisitRepository visitRepository, TriageRepository triageRepository)
        {
            _visitRepository = visitRepository;
            _triageRepository = triageRepository;
        }

        /// <summary>
        /// Fetches all active visits with their triage data and orders them.
        /// </summary>
        public List<(ER_Visit visit, Triage triage)> GetOrderedQueue()
        {
            // Step 1: fetch all active visits (REGISTERED, TRIAGED, WAITING_FOR_ROOM)
            var activeVisits = _visitRepository.GetActiveVisits();

            // Step 2: fetch triage for each visit
            var queueWithTriage = activeVisits
                .Select(v => (visit: v, triage: _triageRepository.GetByVisitId(v.Visit_ID)))
                .Where(x => x.triage != null) // only include visits with triage
                .ToList();

            // Step 3: order by triage level ascending, then arrival time ascending
            return OrderByTriageLevelAndArrivalTime(queueWithTriage);
        }

        /// <summary>
        /// Orders the queue: triage level ascending, then arrival time ascending.
        /// </summary>
        private List<(ER_Visit visit, Triage triage)> OrderByTriageLevelAndArrivalTime(
            List<(ER_Visit visit, Triage triage)> queue)
        {
            return queue
                .OrderBy(x => x.triage.Triage_Level) // 1 = highest priority
                .ThenBy(x => x.visit.Arrival_date_time)
                .ToList();
        }

        /// <summary>
        /// Removes a visit from the queue.
        /// </summary>
        public void RemoveFromQueue(int visitId)
        {
            // Depending on your workflow, "removing from queue" may mean:
            // 1) Mark visit as IN_ROOM
            // 2) Or remove from an in-memory queue (if you have one)
            // Here we assume the first approach: update status to IN_ROOM

            var visit = _visitRepository.GetByVisitId(visitId);
            if (visit == null)
                throw new InvalidOperationException($"Visit {visitId} not found.");

            visit.Status = "IN_ROOM"; // or whatever status indicates removal
            _visitRepository.UpdateStatus(visitId, visit.Status);
        }


    }
}
