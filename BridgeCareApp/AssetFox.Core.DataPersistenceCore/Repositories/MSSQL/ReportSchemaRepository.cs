using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DataPersistenceCore.Repositories.Generics;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class ReportSchemaRepository : IReportSchemaRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfDataPersistenceWork;

        public ReportSchemaRepository(UnitOfDataPersistenceWork unitOfDataPersistenceWork)
        {
            _unitOfDataPersistenceWork = unitOfDataPersistenceWork ?? throw new ArgumentNullException(nameof(unitOfDataPersistenceWork));
        }

        public void Upsert(ReportSchemaDTO dto, Guid? ownerUserId = null)
        {
            var entity = dto.ToEntity();
            var existingEntity = _unitOfDataPersistenceWork.Context.ReportSchema
                .FirstOrDefault(_ => _.Id == dto.Id);

            if (existingEntity != null)
            {
                existingEntity.Name = entity.Name;
                existingEntity.Description = entity.Description;
                existingEntity.SchemaJson = entity.SchemaJson;
                existingEntity.ReportCategory = entity.ReportCategory;
                existingEntity.IsSystemDefault = entity.IsSystemDefault;
                existingEntity.IsSharedWithAll = entity.IsSharedWithAll;
                existingEntity.LastModifiedBy = ownerUserId ?? _unitOfDataPersistenceWork.UserEntity?.Id ?? existingEntity.LastModifiedBy;
                existingEntity.LastModifiedDate = DateTime.Now;
                _unitOfDataPersistenceWork.Context.SaveChanges();
                return;
            }

            _unitOfDataPersistenceWork.AsTransaction(() =>
            {
                _unitOfDataPersistenceWork.Context.AddEntity(entity, ownerUserId ?? _unitOfDataPersistenceWork.UserEntity?.Id);
                if (ownerUserId.HasValue && ownerUserId.Value != Guid.Empty)
                {
                    var users = LibraryUserDtolists.OwnerAccess(ownerUserId.Value);
                    UpsertOrDeleteUsers(dto.Id, users);
                }
            });
        }

        public ReportSchemaDTO Get(Guid id)
        {
            var entity = QueryWithUsers()
                .FirstOrDefault(_ => _.Id == id);

            return ToDtoWithAccess(entity, null, true, includeSchemaJson: true);
        }

        public ReportSchemaDTO GetAccessible(Guid id, Guid userId, bool includeAll)
        {
            var entity = QueryWithUsers()
                .FirstOrDefault(_ => _.Id == id);

            if (!CanRead(entity, userId, includeAll))
            {
                return null;
            }

            return ToDtoWithAccess(entity, userId, includeAll, includeSchemaJson: true);
        }

        public List<ReportSchemaDTO> GetAll()
        {
            return QueryWithUsers()
                .ToList()
                .Select(_ => ToDtoWithAccess(_, null, true, includeSchemaJson: true))
                .ToList();
        }

        public List<ReportSchemaDTO> GetAccessibleSummaries(Guid userId, bool includeAll)
        {
            return QueryWithUsers()
                .ToList()
                .Where(_ => CanRead(_, userId, includeAll))
                .Select(_ => ToDtoWithAccess(_, userId, includeAll, includeSchemaJson: false))
                .ToList();
        }

        public List<ReportSchemaDTO> GetByCategory(string category)
        {
            return QueryWithUsers()
                .Where(_ => _.ReportCategory == category)
                .ToList()
                .Select(_ => ToDtoWithAccess(_, null, true, includeSchemaJson: true))
                .ToList();
        }

        public List<ReportSchemaDTO> GetDefaults()
        {
            return QueryWithUsers()
                .Where(_ => _.IsSystemDefault)
                .ToList()
                .Select(_ => ToDtoWithAccess(_, null, true, includeSchemaJson: true))
                .ToList();
        }

        public LibraryUserAccessModel GetLibraryAccess(Guid reportSchemaId, Guid userId)
        {
            var schema = QueryWithUsers().FirstOrDefault(_ => _.Id == reportSchemaId);
            if (schema == null)
            {
                return LibraryAccessModels.LibraryDoesNotExist();
            }

            var explicitAccess = schema.Users?
                .Where(_ => _.UserId == userId)
                .Select(LibraryUserMapper.ToDto)
                .OrderByDescending(_ => _.AccessLevel)
                .FirstOrDefault();

            if (explicitAccess == null && (schema.IsSystemDefault || schema.IsSharedWithAll))
            {
                explicitAccess = new LibraryUserDTO
                {
                    UserId = userId,
                    AccessLevel = LibraryAccessLevel.Read,
                };
            }

            return LibraryAccessModels.LibraryExistsWithUsers(userId, explicitAccess);
        }

        public List<LibraryUserDTO> GetLibraryUsers(Guid reportSchemaId)
        {
            return _unitOfDataPersistenceWork.Context.ReportSchemaUser
                .Include(_ => _.User)
                .Where(_ => _.ReportSchemaId == reportSchemaId)
                .Select(LibraryUserMapper.ToDto)
                .OrderBy(_ => _.UserName)
                .ToList();
        }

        public void UpsertOrDeleteUsers(Guid reportSchemaId, IList<LibraryUserDTO> users)
        {
            var existingEntities = _unitOfDataPersistenceWork.Context.ReportSchemaUser
                .Where(_ => _.ReportSchemaId == reportSchemaId)
                .ToList();

            var existingUserIds = existingEntities.Select(_ => _.UserId).ToList();
            var desiredUserIds = users.Select(_ => _.UserId).ToList();
            var userIdsToDelete = existingUserIds.Except(desiredUserIds).ToList();
            var userIdsToUpdate = existingUserIds.Intersect(desiredUserIds).ToList();
            var userIdsToAdd = desiredUserIds.Except(existingUserIds).ToList();

            var entitiesToAdd = users
                .Where(_ => userIdsToAdd.Contains(_.UserId))
                .Select(_ => _.ToReportSchemaUserEntity(reportSchemaId))
                .ToList();

            var entitiesToUpdate = new List<ReportSchemaUserEntity>();
            foreach (var dto in users.Where(_ => userIdsToUpdate.Contains(_.UserId)))
            {
                var entity = existingEntities.FirstOrDefault(_ => _.UserId == dto.UserId);
                if (entity != null && entity.AccessLevel != (int)dto.AccessLevel)
                {
                    entity.AccessLevel = (int)dto.AccessLevel;
                    entitiesToUpdate.Add(entity);
                }
            }

            _unitOfDataPersistenceWork.Context.AddRange(entitiesToAdd);
            _unitOfDataPersistenceWork.Context.UpdateRange(entitiesToUpdate);

            var entitiesToDelete = existingEntities
                .Where(_ => userIdsToDelete.Contains(_.UserId))
                .ToList();
            _unitOfDataPersistenceWork.Context.RemoveRange(entitiesToDelete);
            _unitOfDataPersistenceWork.Context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            _unitOfDataPersistenceWork.Context.DeleteEntity<ReportSchemaEntity>(_ => _.Id == id);
        }

        private IQueryable<ReportSchemaEntity> QueryWithUsers()
        {
            return _unitOfDataPersistenceWork.Context.ReportSchema
                .AsNoTracking()
                .Include(_ => _.Users)
                .ThenInclude(_ => _.User);
        }

        private static bool CanRead(ReportSchemaEntity entity, Guid userId, bool includeAll)
        {
            return entity != null &&
                (includeAll ||
                entity.IsSystemDefault ||
                entity.IsSharedWithAll ||
                entity.Users.Any(_ => _.UserId == userId));
        }

        private ReportSchemaDTO ToDtoWithAccess(
            ReportSchemaEntity entity,
            Guid? currentUserId,
            bool includeAll,
            bool includeSchemaJson)
        {
            if (entity == null)
            {
                return null;
            }

            var creator = _unitOfDataPersistenceWork.Context.User
                .AsNoTracking()
                .FirstOrDefault(_ => _.Id == entity.CreatedBy);
            var users = entity.Users?
                .Select(LibraryUserMapper.ToDto)
                .OrderBy(_ => _.UserName)
                .ToList() ?? new List<LibraryUserDTO>();
            var owner = users
                .FirstOrDefault(_ => _.AccessLevel == LibraryAccessLevel.Owner);
            var explicitAccess = currentUserId.HasValue
                ? users.Where(_ => _.UserId == currentUserId.Value).OrderByDescending(_ => _.AccessLevel).FirstOrDefault()
                : null;
            var effectiveAccess = includeAll
                ? LibraryAccessLevel.Owner
                : explicitAccess?.AccessLevel;

            if (!includeAll && effectiveAccess == null && (entity.IsSystemDefault || entity.IsSharedWithAll))
            {
                effectiveAccess = LibraryAccessLevel.Read;
            }

            var counts = GetSchemaCounts(entity.SchemaJson);
            var dto = entity.ToDTO(creator);
            dto.SchemaJson = includeSchemaJson ? entity.SchemaJson : string.Empty;
            dto.OwnerName = owner?.UserName ?? creator?.Username ?? string.Empty;
            dto.Users = users;
            dto.CurrentUserAccessLevel = effectiveAccess;
            dto.CanModify = includeAll || (effectiveAccess.HasValue && effectiveAccess.Value >= LibraryAccessLevel.Modify);
            dto.CanShare = includeAll || effectiveAccess == LibraryAccessLevel.Owner;
            dto.IsShared = entity.IsSharedWithAll || users.Any(_ => _.AccessLevel != LibraryAccessLevel.Owner);
            dto.SharedUserCount = users.Count(_ => _.AccessLevel != LibraryAccessLevel.Owner);
            dto.PageCount = counts.PageCount;
            dto.ComponentCount = counts.ComponentCount;
            return dto;
        }

        private static (int PageCount, int ComponentCount) GetSchemaCounts(string schemaJson)
        {
            if (string.IsNullOrWhiteSpace(schemaJson))
            {
                return (0, 0);
            }

            try
            {
                using var doc = JsonDocument.Parse(schemaJson);
                if (!doc.RootElement.TryGetProperty("pages", out var pages) ||
                    pages.ValueKind != JsonValueKind.Array)
                {
                    return (0, 0);
                }

                var pageCount = 0;
                var componentCount = 0;
                foreach (var page in pages.EnumerateArray())
                {
                    pageCount++;
                    if (page.TryGetProperty("components", out var components) &&
                        components.ValueKind == JsonValueKind.Array)
                    {
                        componentCount += components.GetArrayLength();
                    }
                }

                return (pageCount, componentCount);
            }
            catch
            {
                return (0, 0);
            }
        }
    }
}
