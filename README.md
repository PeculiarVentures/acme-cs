# ACME-CS

![ACME .NET](https://github.com/microshine/acme-cs/workflows/.NET%20Core/badge.svg)

## About

Provides client and server implementations of ACME ([RFC 8555](https://tools.ietf.org/html/rfc8555)) with C-Sharp. It enables you to build solutions that provide complete and robust certificate lifecycle management.

## Installation

```
Install-Package ACME-CS
```
## Usage

### Server

To create a server, see an [example server](https://github.com/PeculiarVentures/acme/tree/master/examples/AspNetCore.Server)

### Client

Create client:

```cs
var client = AcmeClient.CreateAsync(Uri rootUrl, AsymmetricAlgorithm key);
```

#### Directory

Get directory:

```cs
var directory = client.DirectoryGetAsync()
```

or

```cs
var directory = client.Directory;
```

#### Account

Create account:

```cs
var acc = client.AccountCreateAsync(new NewAccount
{
    Contacts = new string[] { "mailto: example@gmail.com" },
    TermsOfServiceAgreed = true,
});
```

Get account:

```cs
var acc = client.AccountCreateAsync(new NewAccount
{
    OnlyReturnExisting = true,
}));
```

Deactivate account:

```cs
client.AccountDeactivateAsync();
```

#### Order

Create order:

```cs
var order = client.OrderCreateAsync(new NewOrder
{
    Identifiers = new List<Identifier> { },
});
```

Get order:

```cs
var orderGet = await client.OrderGetAsync(order.Location);
```

Finalize order:

```cs
var param = new CertificateRequestParams
{
    CommonName = "CN=CommonName",
};
var csr = new Pkcs10CertificateRequest(param, RsaKey);

var orderFinalize = await client.OrderFinalizeAsync(
    order.Finalize,
    new FinalizeOrder
    {
        Csr = Base64Url.Encode(csr.Export()),
    }
);
```

#### Authorization

Create authorization:

```cs
var authorization = await client.AuthorizationCreateAsync(order.Authorizations[0]);
```

Deactivate authorization:

```cs
var authorization = await client.AuthorizationDeactivateAsync(order.Authorizations[0]);
```

#### Challenge

Get challenge:

```cs
var challenge = await client.ChallengeGetAsync(authorization.Challenges[0]);
```

Validate challenge:

```cs
var challenge = await client.ChallengeValidateAsync(authorization.Challenges[0]);
```

#### Certificate

Get certificate:

```cs
var cert = await client.OrderCertificateGetAsync(order.Certificate);
```

Revoke certificate:

```cs
await client.CertificateRevokeAsync(new RevokeCertificate {
  Certificate = Base64Url.Encode(certificateBytes),
  Reason = Reason,
});
```