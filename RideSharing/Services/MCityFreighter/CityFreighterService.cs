using TrueSight.Common;
using RideSharing.Handlers.Configuration;
using RideSharing.Common;
using RideSharing.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using RideSharing.Repositories;
using RideSharing.Entities;
using RideSharing.Enums;

namespace RideSharing.Services.MCityFreighter
{
    public interface ICityFreighterService :  IServiceScoped
    {
        Task<int> Count(CityFreighterFilter CityFreighterFilter);
        Task<List<CityFreighter>> List(CityFreighterFilter CityFreighterFilter);
        Task<CityFreighter> Get(long Id);
        Task<CityFreighter> Create(CityFreighter CityFreighter);
        Task<CityFreighter> Update(CityFreighter CityFreighter);
        Task<CityFreighter> Delete(CityFreighter CityFreighter);
        Task<List<CityFreighter>> BulkDelete(List<CityFreighter> CityFreighters);
        Task<List<CityFreighter>> BulkMerge(List<CityFreighter> CityFreighters);
        Task<CityFreighterFilter> ToFilter(CityFreighterFilter CityFreighterFilter);
    }

    public class CityFreighterService : BaseService, ICityFreighterService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ILogging Logging;
        private ICurrentContext CurrentContext;
        
        private ICityFreighterValidator CityFreighterValidator;

        public CityFreighterService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            ICityFreighterValidator CityFreighterValidator,
            ILogging Logging
        )
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.Logging = Logging;
           
            this.CityFreighterValidator = CityFreighterValidator;
        }

        public async Task<int> Count(CityFreighterFilter CityFreighterFilter)
        {
            try
            {
                int result = await UOW.CityFreighterRepository.Count(CityFreighterFilter);
                return result;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CityFreighterService));
            }
            return 0;
        }

        public async Task<List<CityFreighter>> List(CityFreighterFilter CityFreighterFilter)
        {
            try
            {
                List<CityFreighter> CityFreighters = await UOW.CityFreighterRepository.List(CityFreighterFilter);
                return CityFreighters;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CityFreighterService));
            }
            return null;
        }

        public async Task<CityFreighter> Get(long Id)
        {
            CityFreighter CityFreighter = await UOW.CityFreighterRepository.Get(Id);
            if (CityFreighter == null)
                return null;
            await CityFreighterValidator.Get(CityFreighter);
            return CityFreighter;
        }
        
        public async Task<CityFreighter> Create(CityFreighter CityFreighter)
        {
            if (!await CityFreighterValidator.Create(CityFreighter))
                return CityFreighter;

            try
            {
                await UOW.CityFreighterRepository.Create(CityFreighter);
                CityFreighter = await UOW.CityFreighterRepository.Get(CityFreighter.Id);
                Sync(new List<CityFreighter> { CityFreighter });
                return CityFreighter;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CityFreighterService));
            }
            return null;
        }

        public async Task<CityFreighter> Update(CityFreighter CityFreighter)
        {
            if (!await CityFreighterValidator.Update(CityFreighter))
                return CityFreighter;
            try
            {
                var oldData = await UOW.CityFreighterRepository.Get(CityFreighter.Id);

                await UOW.CityFreighterRepository.Update(CityFreighter);

                CityFreighter = await UOW.CityFreighterRepository.Get(CityFreighter.Id);
                Sync(new List<CityFreighter> { CityFreighter });
                return CityFreighter;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CityFreighterService));
            }
            return null;
        }

        public async Task<CityFreighter> Delete(CityFreighter CityFreighter)
        {
            if (!await CityFreighterValidator.Delete(CityFreighter))
                return CityFreighter;

            try
            {
                await UOW.CityFreighterRepository.Delete(CityFreighter);
                return CityFreighter;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CityFreighterService));
            }
            return null;
        }

        public async Task<List<CityFreighter>> BulkDelete(List<CityFreighter> CityFreighters)
        {
            if (!await CityFreighterValidator.BulkDelete(CityFreighters))
                return CityFreighters;

            try
            {
                await UOW.CityFreighterRepository.BulkDelete(CityFreighters);
                return CityFreighters;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CityFreighterService));
            }
            return null;
        }

        public async Task<List<CityFreighter>> BulkMerge(List<CityFreighter> CityFreighters)
        {
            if (!await CityFreighterValidator.Import(CityFreighters))
                return CityFreighters;
            try
            {
                var Ids = await UOW.CityFreighterRepository.BulkMerge(CityFreighters);
                CityFreighters = await UOW.CityFreighterRepository.List(Ids);
                Sync(CityFreighters);
                return CityFreighters;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CityFreighterService));
            }
            return null;
        }     
        
        public async Task<CityFreighterFilter> ToFilter(CityFreighterFilter filter)
        {
            if (filter.OrFilter == null) filter.OrFilter = new List<CityFreighterFilter>();
            if (CurrentContext.Filters == null || CurrentContext.Filters.Count == 0) return filter;
            foreach (var currentFilter in CurrentContext.Filters)
            {
                CityFreighterFilter subFilter = new CityFreighterFilter();
                filter.OrFilter.Add(subFilter);
                List<FilterPermissionDefinition> FilterPermissionDefinitions = currentFilter.Value;
                foreach (FilterPermissionDefinition FilterPermissionDefinition in FilterPermissionDefinitions)
                {
                    if (FilterPermissionDefinition.Name == "CityFreighterId")
                        subFilter.Id = FilterBuilder.Merge(subFilter.Id, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Name))
                        subFilter.Name = FilterBuilder.Merge(subFilter.Name, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Capacity))
                        subFilter.Capacity = FilterBuilder.Merge(subFilter.Capacity, FilterPermissionDefinition.DecimalFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Latitude))
                        subFilter.Latitude = FilterBuilder.Merge(subFilter.Latitude, FilterPermissionDefinition.DecimalFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Longtitude))
                        subFilter.Longtitude = FilterBuilder.Merge(subFilter.Longtitude, FilterPermissionDefinition.DecimalFilter);
                    if (FilterPermissionDefinition.Name == nameof(CurrentContext.UserId) && FilterPermissionDefinition.IdFilter != null)
                    {
                        if (FilterPermissionDefinition.IdFilter.Equal.HasValue && FilterPermissionDefinition.IdFilter.Equal.Value == CurrentUserEnum.IS.Id)
                        {
                        }
                        if (FilterPermissionDefinition.IdFilter.Equal.HasValue && FilterPermissionDefinition.IdFilter.Equal.Value == CurrentUserEnum.ISNT.Id)
                        {
                        }
                    }
                }
            }
            return filter;
        }

        private void Sync(List<CityFreighter> CityFreighters)
        {


            
        }
    }
}
