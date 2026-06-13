using Harfi.DTOs.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Harfi.Services.Interfaces;

public interface IServiceTypeService
{
    Task<IEnumerable<PublicServiceTypeDto>> GetActiveServiceTypesAsync();
}
