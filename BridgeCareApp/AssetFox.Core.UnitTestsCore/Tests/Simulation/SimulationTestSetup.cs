using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Attributes.CalculatedAttributes;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class SimulationTestSetup { 

        public static SimulationEntity EntityInDb(UnitOfDataPersistenceWork unitOfWork, Guid networkId, Guid creatingUserId = new())
        {
            var name = RandomStrings.WithPrefix("Simulation");
            var entity = new SimulationEntity
            {
                NetworkId = networkId,
                Name = name,
                CreatedBy = creatingUserId,
            };
            unitOfWork.Context.Add(entity);
            unitOfWork.Context.SaveChanges();
            return entity;
        }

        public static SimulationDTO ModelForEntityInDb(UnitOfDataPersistenceWork unitOfWork, Guid? id = null, string name = null, Guid? owner = null, Guid? networkId = null)
        {
            var resolveNetworkId = networkId ?? NetworkTestSetup.NetworkId;
            CalculatedAttributeTestSetup.CreateDefaultCalculatedAttributeLibrary(unitOfWork);
            var dto = SimulationDtos.Dto(id, name, owner);
            unitOfWork.SimulationRepo.CreateSimulation(resolveNetworkId, dto);
            return dto;
        }

        public static Simulation DomainSimulation(UnitOfDataPersistenceWork unitOfWork, Guid? networkId = null)
        {
            var resolveNetworkId = networkId ?? NetworkTestSetup.NetworkId;
            var simulationEntity = EntityInDb(unitOfWork, NetworkTestSetup.NetworkId);
            var simulationDto = unitOfWork.SimulationRepo.GetSimulation(simulationEntity.Id);
            var explorer = unitOfWork.AttributeRepo.GetExplorer();
            var network = unitOfWork.NetworkRepo.GetSimulationAnalysisNetwork(resolveNetworkId, explorer);
            var date = new DateTime(2022, 10, 6);
            SimulationMapper.CreateSimulation(simulationEntity, network, date, date);
            var simulationObject = network.Simulations.Single(s => s.Id == simulationDto.Id);
            return simulationObject;
        }
    }
}
