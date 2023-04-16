namespace ChiaLogProcessingApp.Core.Services
{
	public interface IJsonSettingsService
	{
		T Load<T>() where T : class;
		void Save<T>(T settings) where T : class;
	}
}