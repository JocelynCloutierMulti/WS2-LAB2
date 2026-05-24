using FR_WS2_BaseLab.Models;
using Microsoft.Exchange.WebServices.Data;
namespace FR_WS2_BaseLab.Services.Interfaces;
public interface ITopicService
{
	Task<ServiceResult<List<Topic>>> GetByCategoryIdAsync(int categoryId);
	Task<ServiceResult<Topic>> GetDetailsAsync(int id);
	Task<ServiceResult<Topic>> GetForEditAsync(int id);
	Task<ServiceResult<Topic>> CreateAsync(Topic topic, string? userId);
	Task<ServiceResult<Topic>> UpdateAsync(int id, Topic topic);
	Task<ServiceResult<Topic>> DeleteAsync(int id);
	Task<bool> ExistsAsync(int id);

}

