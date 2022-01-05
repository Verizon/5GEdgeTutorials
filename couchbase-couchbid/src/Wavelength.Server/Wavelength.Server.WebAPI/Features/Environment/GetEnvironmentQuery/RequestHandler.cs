using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wavelength.Core.DataAccessObjects;
using Wavelength.Core.Models;
using Wavelength.Server.WebAPI.Services;

namespace Wavelength.Server.WebAPI.Features.Environment.GetEnvironmentQuery
{
    public class RequestHandler
        : IRequestHandler<RequestQuery, AppEnvironmentDAO>
    {
        private readonly CouchbaseConfig _couchbaseConfig;

        public RequestHandler(
            ICouchbaseConfigService configuration)
        {
            _couchbaseConfig = configuration.Config;
        }

        public async Task<AppEnvironmentDAO> Handle(
            RequestQuery request, 
            CancellationToken cancellationToken)
        {
            //map config to DAO
            return await Task.Run<AppEnvironmentDAO>(() =>
              {
                  return Mapper();
              });
        }

        private AppEnvironmentDAO Mapper()
        {
            return new AppEnvironmentDAO
            {
                BucketName = _couchbaseConfig.BucketName ?? string.Empty,
                CollectionName = _couchbaseConfig.CollectionName ?? string.Empty,
                DbConnectionString = _couchbaseConfig.ConnectionString ?? string.Empty,
                Mode = _couchbaseConfig.Mode ?? string.Empty,
                Name = _couchbaseConfig.Location ?? string.Empty,
                ScopeName = _couchbaseConfig.ScopeName ?? string.Empty,
                SyncGatewayUri = _couchbaseConfig.SyncGatewayUri ?? string.Empty,
                IsDurabilityPersistToMajority = _couchbaseConfig.DurabilityPersistToMajority
            };
        }
    }
}
