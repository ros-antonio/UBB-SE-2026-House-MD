using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ERManagementSystem.Core.Helpers;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;

namespace ERManagementSystem.Core.Services
{
    public class RoomManagementService : IRoomManagementService
    {
        private readonly IRoomRepository roomRepository;
        private readonly IPatientRepository patientRepository;
        private readonly ITriageRepository triageRepository;

        public RoomManagementService(
            IRoomRepository roomRepository,
            IPatientRepository patientRepository,
            ITriageRepository triageRepository)
        {
            this.roomRepository = roomRepository;
            this.patientRepository = patientRepository;
            this.triageRepository = triageRepository;
        }

        [ExcludeFromCodeCoverage]
        public List<ER_Room> GetAvailableRooms() => roomRepository.GetAvailableRooms();
        [ExcludeFromCodeCoverage]
        public List<ER_Room> GetOccupiedRooms() => roomRepository.GetOccupiedRooms();
        [ExcludeFromCodeCoverage]
        public List<ER_Room> GetCleaningRooms() => roomRepository.GetCleaningRooms();

        public void MarkRoomAsCleaning(int roomId)
        {
            ER_Room room = roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Occupied)
            {
                throw new InvalidOperationException(
                    $"Room {roomId} cannot be set to cleaning from '{room.Availability_Status}'. Must be 'occupied'.");
            }

            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Cleaning);
            roomRepository.UpdateAvailabilityStatus(roomId, ER_Room.RoomStatus.Cleaning);
            roomRepository.ClearCurrentVisit(roomId);   // clear visit link so panel doesn't show stale data
            Logger.Info($"Room {roomId} set to cleaning.");
        }

        public void MarkRoomAsCleaned(int roomId)
        {
            ER_Room room = roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Cleaning)
            {
                throw new InvalidOperationException(
                    $"Room {roomId} cannot be marked as cleaned — current status is '{room.Availability_Status}', not 'cleaning'.");
            }

            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Available);
            roomRepository.UpdateAvailabilityStatus(roomId, ER_Room.RoomStatus.Available);
            Logger.Info($"Room {roomId} is now available.");
        }

        public RoomVisitDetails? GetRoomVisitDetails(int roomId)
        {
            var visit = roomRepository.GetVisitByRoomId(roomId);
            if (visit == null)
            {
                return null;
            }

            return new RoomVisitDetails
            {
                Visit = visit,
                Patient = patientRepository.GetById(visit.Patient_ID),
                Triage = triageRepository.GetByVisitId(visit.Visit_ID)
            };
        }
    }
}
