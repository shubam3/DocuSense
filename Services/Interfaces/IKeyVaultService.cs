namespace DocuSense.Services.Interfaces
{
    public interface IKeyVaultService
    {
        Task<string?> GetSecretAsync(string secretName);
        Task<bool> SetSecretAsync(string secretName, string secretValue);
        Task<bool> DeleteSecretAsync(string secretName);
        Task<List<string>> ListSecretsAsync();
        Task<bool> SecretExistsAsync(string secretName);
    }
} 