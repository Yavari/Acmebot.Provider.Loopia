# Loopia provider for Key Vault Acmebot

[Loopia](https://www.loopia.se) provier for [Key Vault Acmebot](https://github.com/shibayan/keyvault-acmebot) that automates the issuance and renewal of ACME SSL/TLS certificates.

This Azure Function implenets the REST API for [CUSTOM DNS Provider](https://github.com/shibayan/keyvault-acmebot/wiki/DNS-Provider-Configuration) 

## Key Vault Acmebot settings
Set following settings to use the Custom DNS provider in Key Vault Acmebot. The *APIKEY* is *_master* key under *APP KEYS*  in Loopia Azure Function.

    Acmebot:CustomDns:ApiKey=<APIKEY>
    Acmebot:CustomDns:ApiKeyHeaderName=x-functions-key
    Acmebot:CustomDns:Endpoint=https://<FUNCTION_NAME>.azurewebsites.net/api/
    Acmebot:CustomDns:PropagationSeconds=1800

## Loopia Azure Function Settings

Sign in to Loopia and and click *API-användare* in the bottom right corner. Click on *Skapa API-användare* and pick a username and password. Then click on the created user and select following permissions:

- getZoneRecords
- addZoneRecords
- removeZoneRecords
- removeSubdomain 

Set the username and password under the Loopia Azure Function configurations:

    Loopia:Password=<Loopia Username>
    Loopia:Username=<Loopia Password>