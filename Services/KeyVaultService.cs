using Azure.Security.KeyVault.Secrets;
using DocuSense.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DocuSense.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger _logger;

        public KeyVaultService(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            
            var keyVaultUrl = configuration["Azure:KeyVault:Url"];
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new InvalidOperationException("Azure Key Vault URL not configured");
            }

            var credential = new Azure.Identity.DefaultAzureCredential();
            _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
        }

        public async Task<string?> GetSecretAsync(string secretName)
        {
            try
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                return secret.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting secret from Key Vault: {SecretName}", secretName);
                return null;
            }
        }

        public async Task<bool> SetSecretAsync(string secretName, string secretValue)
        {
            try
            {
                await _secretClient.SetSecretAsync(secretName, secretValue);
                _logger.Information("Secret set successfully in Key Vault: {SecretName}", secretName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting secret in Key Vault: {SecretName}", secretName);
                return false;
            }
        }

        public async Task<bool> DeleteSecretAsync(string secretName)
        {
            try
            {
                await _secretClient.StartDeleteSecretAsync(secretName);
                _logger.Information("Secret deleted successfully from Key Vault: {SecretName}", secretName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting secret from Key Vault: {SecretName}", secretName);
                return false;
            }
        }

        public async Task<List<string>> ListSecretsAsync()
        {
            try
            {
                var secrets = new List<string>();
                await foreach (var secret in _secretClient.GetPropertiesOfSecretsAsync())
                {
                    secrets.Add(secret.Name);
                }
                return secrets;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error listing secrets from Key Vault");
                return new List<string>();
            }
        }

        public async Task<bool> SecretExistsAsync(string secretName)
        {
            try
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                return secret.Value != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking if secret exists in Key Vault: {SecretName}", secretName);
                return false;
            }
        }
    }
} 