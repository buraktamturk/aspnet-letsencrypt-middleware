# LetsEncrypt ASP.NET 5 Middleware

LetsEncrypt integration draft as ASP.NET 5 middleware.

## Modes

### Proxy

Proxy mode forwards all .well-known requests to another server. It does not have to be https but I strongly recommend because of security.

```csharp
app.UseLetsEncrypt(new LetsEncryptMiddlewareOptions
{
    useProxyMode = true,
    proxyPrefix = "https://vpn.buraktamturk.org/.well-known"
});
```

### Put

In put mode, you put the acme challenge data to /.well-known/acme-challenge/CODE1?key=PRE-SHARED-KEY-HERE by issuing HTTP PUT request. Then this library makes it accessible /.well-known/acme-challenge/CODE1 to anyone. It does not use any cache-storage it stores the data on a Dictionary, therefore if your application restarts (or recycles) the data is gone. Since it stores locally on single node on RAM, It does not suit well web farms (where your application runs on multiple servers)

It is your benefit to put the pre shared key to config.json or secret storage. Be sure not to include the file in repositories.

```csharp
app.UseLetsEncrypt(new LetsEncryptMiddlewareOptions
{
	usePutMode = true,
	preSharedKey = "PRE-SHARED-KEY-HERE"
});
```

## Security

The library itself is **currently** not secure under some circumstances.

* anyone knows pre shared keys can issue certificate for your own domain if the library configured to run on PUT mode, be sure not to include the secret in any repositories. 
* anyone hijacks the proxy host can issue certificate for your own domain if the library configured to run on PROXY mode, be sure to use HTTPS in your proxy in order to avoid this. 

I'll add more checks and more methods in future in order to improve security and scalability.