using ADPA.Shared.DTOs;

namespace ADPA.Web.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    bool IsAuthenticated { get; }
}

public class AuthService : IAuthService
{
    private readonly ApiService _apiService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ApiService apiService, ILogger<AuthService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public bool IsAuthenticated { get; private set; }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var response = await _apiService.PostAsync<AuthResponseDto>("/auth/login", loginDto);
            
            if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
            {
                _apiService.SetAuthorizationHeader(response.Token);
                IsAuthenticated = true;
            }
            
            return response ?? new AuthResponseDto { Success = false, Message = "Login failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user: {Email}", loginDto.Email);
            return new AuthResponseDto { Success = false, Message = "Login failed: " + ex.Message };
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var response = await _apiService.PostAsync<AuthResponseDto>("/auth/register", registerDto);
            return response ?? new AuthResponseDto { Success = false, Message = "Registration failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for user: {Email}", registerDto.Email);
            return new AuthResponseDto { Success = false, Message = "Registration failed: " + ex.Message };
        }
    }

    public Task LogoutAsync()
    {
        _apiService.ClearAuthorizationHeader();
        IsAuthenticated = false;
        return Task.CompletedTask;
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            if (!IsAuthenticated) return null;
            
            return await _apiService.GetAsync<UserDto>("/auth/user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user");
            return null;
        }
    }
}