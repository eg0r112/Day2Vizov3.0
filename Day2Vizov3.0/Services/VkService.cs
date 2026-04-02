using VkNet;
using VkNet.Model;
using Microsoft.Extensions.Options;

namespace Day2Vizov3._0.Services;

public class VkService : IDisposable
{
    private readonly VkApi _vkApi;
    private readonly ILogger<VkService> _logger;
    private readonly string _groupToken;
    private readonly long _adminUserId;
    private bool _isInitialized = false;

    public VkService(IConfiguration configuration, ILogger<VkService> logger)
    {
        _logger = logger;
        _groupToken = configuration["VkSettings:GroupToken"] ?? throw new ArgumentNullException("VkSettings:GroupToken");
        _adminUserId = configuration.GetValue<long>("VkSettings:AdminUserId");
        _vkApi = new VkApi();
        
        Task.Run(async () => await InitializeAsync()).Wait();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _vkApi.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = _groupToken
            });
            
            _isInitialized = true;
            _logger.LogInformation("VK API успешно авторизован");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка авторизации VK API");
        }
    }

    public async Task<bool> SendConfirmationCodeAsync(string username, string role, string code)
    {
        if (!_isInitialized)
        {
            _logger.LogWarning("VK API не инициализирован");
            return false;
        }

        try
        {
            var message = $"🔐 **Новая регистрация!**\n\n" +
                         $"👤 **Имя пользователя:** {username}\n" +
                         $"🎭 **Желаемая роль:** {role}\n" +
                         $"🔑 **Код подтверждения:** `{code}`\n\n" +
                         $"⏰ Код действителен 5 минут";

            var result = await _vkApi.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = _adminUserId,
                Message = message,
                RandomId = new Random().Next(1, int.MaxValue)
            });

            _logger.LogInformation($"Код подтверждения отправлен администратору. ID сообщения: {result}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке кода администратору");
            return false;
        }
    }

    public void Dispose()
    {
        _vkApi?.Dispose();
    }
}