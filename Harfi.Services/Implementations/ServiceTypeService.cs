using Harfi.DTOs.Service;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Harfi.Services.Implementations;

public class ServiceTypeService : IServiceTypeService
{
    private readonly IGenericRepository<ServiceType> _serviceTypeRepo;

    public ServiceTypeService(IGenericRepository<ServiceType> serviceTypeRepo)
    {
        _serviceTypeRepo = serviceTypeRepo;
    }

    public async Task<IEnumerable<PublicServiceTypeDto>> GetActiveServiceTypesAsync()
    {
        var items = await _serviceTypeRepo.FindAsync(s => s.IsActive);
        return items.Select(s => new PublicServiceTypeDto
        {
            Id = s.Id,
            NameAr = s.NameAr,
            NameEn = s.NameEn,
            Icon = s.Icon
        });
    }
}
