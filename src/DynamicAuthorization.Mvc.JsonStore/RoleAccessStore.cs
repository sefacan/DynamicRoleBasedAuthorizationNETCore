﻿using DynamicAuthorization.Mvc.Core;
using JsonFlatFileDataStore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicAuthorization.Mvc.JsonStore
{
    public class RoleAccessStore : IRoleAccessStore
    {
        private readonly DataStore _store;

        public RoleAccessStore(DataStore store)
        {
            _store = store;
        }

        public Task<bool> AddRoleAccessAsync(RoleAccess roleAccess)
        {
            var collection = _store.GetCollection<RoleAccess>();

            return collection.InsertOneAsync(roleAccess);
        }

        public Task<bool> EditRoleAccessAsync(RoleAccess roleAccess)
        {
            var collection = _store.GetCollection<RoleAccess>();
            return collection.AsQueryable().Any(ra => ra.RoleId == roleAccess.RoleId)
                ? collection.ReplaceOneAsync(roleAccess.RoleId, roleAccess)
                : collection.InsertOneAsync(roleAccess);
        }

        public Task<bool> RemoveRoleAccessAsync(string roleId)
        {
            var collection = _store.GetCollection<RoleAccess>();

            return collection.DeleteOneAsync(roleId);
        }

        public Task<RoleAccess> GetRoleAccessAsync(string roleId)
        {
            var collection = _store.GetCollection<RoleAccess>();

            return Task.FromResult(collection.AsQueryable().FirstOrDefault(ra => ra.RoleId == roleId));
        }

        public Task<bool> HasAccessToActionAsync(string actionId, params string[] roles)
        {
            if (roles == null || !roles.Any())
                return Task.FromResult(false);

            var accessList = _store.GetCollection<RoleAccess>()
                .AsQueryable()
                .Where(ra => roles.Contains(ra.RoleId))
                .SelectMany(ra => ra.Controllers)
                .ToList();

            return Task.FromResult(accessList.SelectMany(c => c.Actions).Any(a => a.Id == actionId));
        }
    }
}