using CitySO.Configuration.Models;

namespace CitySO.Configuration;

public interface IConfigurationService
{
    public AppConfiguration GetGeneralOptions();
    public void SaveGeneralOptions(AppConfiguration options);
}