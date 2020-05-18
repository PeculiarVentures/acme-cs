using System;
using System.Security.Cryptography;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Web;
using Xunit;

namespace PeculiarVentures.ACME.Server.Controllers
{
    public class AcmeControllerTests
    {

        #region Directory

        [Fact(DisplayName = "Directory: GET without meta data")]
        public void Directory_Get()
        {
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            var response = controller.GetDirectory(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}directory",
                Method = "GET",
            });

            var content = response.GetContent<Protocol.Directory>();

            Assert.Equal($"{Application.BaseAddress}new-nonce", content.NewNonce);
            Assert.Equal($"{Application.BaseAddress}new-acct", content.NewAccount);
            Assert.Equal($"{Application.BaseAddress}new-order", content.NewOrder);
            // TODO Assert.Equal($"{Application.BaseAddress}new-authz", content.NewAuthz);
            Assert.Equal($"{Application.BaseAddress}revoke", content.RevokeCertificate);
            Assert.Equal($"{Application.BaseAddress}key-change", content.KeyChange);
            Assert.Null(content.Meta);

        }

        [Fact(DisplayName = "Directory: GET with meta data")]
        public void Directory_GeMetadata()
        {
            using var provider = Application.CreateProvider(o =>
            {
                o.BaseAddress = Application.BaseAddress;
                o.ExternalAccountOptions.Type = ExternalAccountType.Required;
            });
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            var response = controller.GetDirectory(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}directory",
                Method = "GET",
            });

            var content = response.GetContent<Protocol.Directory>();

            Assert.NotNull(content.Meta);
            Assert.Equal(true, content.Meta.ExternalAccountRequired);

        }
        #endregion

        #region Nonce

        [Fact(DisplayName = "New nonce: GET")]
        public void Nonce_Get()
        {
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            var response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "GET",
            });

            // The server MUST also respond to GET requests for this
            // resource, returning an empty body(while still providing a Replay -
            // Nonce header) with a status code of 204(No Content)
            Assert.Equal(204, response.StatusCode);
            Assert.NotNull(response.Headers.ReplayNonce);
        }

        [Fact(DisplayName = "New nonce: HEAD")]
        public void Nonce_Head()
        {
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            var response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // The server's response MUST include a Replay-
            // Nonce header field containing a fresh nonce and SHOULD have status
            // code 200(OK)
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Headers.ReplayNonce);
        }

        #endregion

        #region POST request security validation

        [Fact(DisplayName = "POST: url does not present")]
        public void POST_Url_Empty()
        {
            /// The "url" header parameter MUST be carried
            /// in the protected header of the JWS.

            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Nonce = response.Headers.ReplayNonce,
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
            });
            token.Sign(accountKey);

            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(400, response.StatusCode);

            var content = response.GetContent<Protocol.Error>();

            Assert.Equal(Protocol.ErrorType.Malformed, content.Type);
            Assert.NotNull(content.Detail);
        }

        [Fact(DisplayName = "POST: url does not match to target URL")]
        public void POST_Url_WrongTargetUrl()
        {
            /// The value of the "url" header
            /// parameter MUST be a string representing the target URL

            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = "https://wrong.com/acme/new-account",
                Nonce = response.Headers.ReplayNonce,
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
            });
            token.Sign(accountKey);

            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(400, response.StatusCode);

            var content = response.GetContent<Protocol.Error>();

            Assert.Equal(Protocol.ErrorType.Malformed, content.Type);
            Assert.NotNull(content.Detail);
        }

        [Fact(DisplayName = "POST: empty nonce")]
        public void POST_Nonce_Empty()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
            });
            token.Sign(accountKey);

            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(400, response.StatusCode);

            var content = response.GetContent<Protocol.Error>();

            Assert.Equal(Protocol.ErrorType.Malformed, content.Type);
            Assert.NotNull(content.Detail);
        }

        [Fact(DisplayName = "POST: wrong encoding")]
        public void POST_Nonce_WrongEncoding()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = "Wrong encoded nonce"
            });
            token.Sign(accountKey);

            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            // If the value of a "nonce" header parameter is not valid
            // according to this encoding, then the verifier MUST reject the JWS as
            // malformed.
            Assert.Equal(400, response.StatusCode);

            var content = response.GetContent<Protocol.Error>();

            Assert.Equal(Protocol.ErrorType.Malformed, content.Type);
            Assert.NotNull(content.Detail);
        }

        [Fact(DisplayName = "POST: invalid signature")]
        public void POST_InvalidSignature()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
            });
            token.Sign(accountKey);
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
            });

            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(401, response.StatusCode);

            var content = response.GetContent<Protocol.Error>();

            Assert.Equal(Protocol.ErrorType.Unauthorized, content.Type);
            Assert.NotNull(content.Detail);
        }
        #endregion

        #region Account management

        [Fact(DisplayName = "Account: Create a new account with a wrong contact")]
        public void Account_New_Contact_Wrong()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:some@contact@mail.com" },
                TermsOfServiceAgreed = true,
            });
            token.Sign(accountKey);

            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });
            Assert.Equal(400, response.StatusCode);

            var content = response.GetContent<Protocol.Error>();

            Assert.Equal(Protocol.ErrorType.UnsupportedContact, content.Type);
            Assert.NotNull(content.Detail);
        }

        [Fact(DisplayName = "Account: Create a new account with disabled TermsOfService")]
        public void Account_New_TermsOfService_Disabled()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // Create token
            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact@mail.com" },
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(201, response.StatusCode);

            var content = response.GetContent<Protocol.Account>();

            Assert.Single(response.Headers.Link);
            Assert.Equal("https://test.server.com/acme/acct/1", response.Headers.Location);
            Assert.NotNull(content);
            Assert.NotNull(content.Key);
            Assert.Null(content.TermsOfServiceAgreed);
        }

        [Fact(DisplayName = "Account: Create a new account with enabled TermsOfService with TermsOfServiceAgreed:false")]
        public void Account_New_TermsOfService_Enabled_TermsOfServiceAgreed_False()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider(o =>
            {
                o.TermsOfService = "https://some.com/acme/terms.pdf";
            });
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // Create token
            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact@mail.com" },
                TermsOfServiceAgreed = false,
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(400, response.StatusCode);

            var content = response.GetContent<Protocol.Error>();

            Assert.Equal(Protocol.ErrorType.Malformed, content.Type);
            Assert.NotNull(content.Detail);
        }

        [Fact(DisplayName = "Account: Create using duplicate key")]
        public void Account_New_DuplicateKey()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // Create token
            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact@mail.com" },
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(201, response.StatusCode);

            token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact2@mail.com" },
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(200, response.StatusCode);

            var content = response.GetContent<Protocol.Account>();

            Assert.Single(response.Headers.Link);
            Assert.Equal("https://test.server.com/acme/acct/1", response.Headers.Location);
            Assert.Equal("mailto:somecontact@mail.com", content.Contacts[0]);
        }

        [Fact(DisplayName = "Account: Update")]
        public void Account_Update()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // Create token
            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact@mail.com" },
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(201, response.StatusCode);

            token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = response.Headers.Location,
                Algorithm = AlgorithmsEnum.RS256,
                KeyID = response.Headers.Location,
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new UpdateAccount
            {
                Contacts = new string[] { "mailto:somecontact2@mail.com" },
            });
            token.Sign(accountKey);

            // Update account
            response = controller.PostAccount(new AcmeRequest
            {
                Path = response.Headers.Location,
                Method = "POST",
                Token = token,
            });

            Assert.Equal(200, response.StatusCode);

            var content = response.GetContent<Protocol.Account>();

            Assert.Single(response.Headers.Link);
            Assert.Equal("https://test.server.com/acme/acct/1", response.Headers.Location);
            Assert.Equal("mailto:somecontact2@mail.com", content.Contacts[0]);
        }

        [Fact(DisplayName = "Account: Update with invalid contact")]
        public void Account_Update_InvalidContact()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // Create token
            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact@mail.com" },
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(201, response.StatusCode);

            token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = response.Headers.Location,
                Algorithm = AlgorithmsEnum.RS256,
                KeyID = response.Headers.Location,
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new UpdateAccount
            {
                Contacts = new string[] { "mailto:some@contact@mail.com" },
            });
            token.Sign(accountKey);

            // Update account
            response = controller.PostAccount(new AcmeRequest
            {
                Path = response.Headers.Location,
                Method = "POST",
                Token = token,
            });

            Assert.Equal(400, response.StatusCode);

            var content = response.GetContent<Protocol.Error>();

            Assert.Equal(Protocol.ErrorType.UnsupportedContact, content.Type);
            Assert.NotNull(content.Detail);
        }

        [Fact(DisplayName = "Account: Deactivate account")]
        public void Account_Deactivate()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // Create token
            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact@mail.com" },
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(201, response.StatusCode);

            token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = response.Headers.Location,
                Algorithm = AlgorithmsEnum.RS256,
                KeyID = response.Headers.Location,
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new UpdateAccount
            {
                Contacts = new string[] { "mailto:somecontact2@mail.com" },
                Status = Protocol.AccountStatus.Deactivated,
            });
            token.Sign(accountKey);

            // Update account
            response = controller.PostAccount(new AcmeRequest
            {
                Path = response.Headers.Location,
                Method = "POST",
                Token = token,
            });

            Assert.Equal(200, response.StatusCode);

            var content = response.GetContent<Protocol.Account>();

            Assert.Equal("mailto:somecontact@mail.com", content.Contacts[0]);
            Assert.Equal(Protocol.AccountStatus.Deactivated, content.Status);
        }

        [Fact(DisplayName = "Account: Change key")]
        public void Account_ChangeKey()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // Create token
            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact@mail.com" },
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(201, response.StatusCode);
            var oldAccount = response.GetContent<Protocol.Account>();

            token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}key-change",
                Algorithm = AlgorithmsEnum.RS256,
                KeyID = response.Headers.Location,
                Nonce = response.Headers.ReplayNonce,
            });
            #region New key
            var newKey = RSA.Create(2048);
            var newKeyToken = new JsonWebSignature();
            newKeyToken.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}key-change",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(newKey),
            });
            newKeyToken.SetPayload(new ChangeKey
            {
                Account = response.Headers.Location,
                Key = new JsonWebKey(accountKey),
            });
            newKeyToken.Sign(newKey);
            #endregion

            token.SetPayload(newKeyToken);
            token.Sign(accountKey);

            // Update account
            response = controller.ChangeKey(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}key-change",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(200, response.StatusCode);

            var content = response.GetContent<Protocol.Account>();

            Assert.NotEqual(oldAccount.Key, content.Key);
            Assert.Equal("mailto:somecontact@mail.com", content.Contacts[0]);
            Assert.Equal(Protocol.AccountStatus.Valid, content.Status);
        }

        [Fact(DisplayName = "Account: Change key using duplicate key")]
        public void Account_ChangeKey_DuplicateKey()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider();
            IAcmeController controller = (IAcmeController)provider.GetService(typeof(IAcmeController));

            // Get nonce
            response = controller.GetNonce(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-nonce",
                Method = "HEAD",
            });

            // Create token
            var accountKey = RSA.Create(2048);
            var token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(accountKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact@mail.com" },
            });
            token.Sign(accountKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(201, response.StatusCode);

            // Create token
            var duplicateKey = RSA.Create(2048);
            token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}new-acct",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(duplicateKey),
                Nonce = response.Headers.ReplayNonce,
            });
            token.SetPayload(new NewAccount
            {
                Contacts = new string[] { "mailto:somecontact2@mail.com" },
            });
            token.Sign(duplicateKey);

            // Create account
            response = controller.CreateAccount(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}new-acct",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(201, response.StatusCode);

            token = new JsonWebSignature();
            token.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}key-change",
                Algorithm = AlgorithmsEnum.RS256,
                KeyID = $"{Application.BaseAddress}acct/1",
                Nonce = response.Headers.ReplayNonce,
            });
            #region New key
            var newKeyToken = new JsonWebSignature();
            newKeyToken.SetProtected(new JsonWebSignatureProtected
            {
                Url = $"{Application.BaseAddress}key-change",
                Algorithm = AlgorithmsEnum.RS256,
                Key = new JsonWebKey(duplicateKey),
            });
            newKeyToken.SetPayload(new ChangeKey
            {
                Account = $"{Application.BaseAddress}acct/1",
                Key = new JsonWebKey(accountKey),
            });
            newKeyToken.Sign(duplicateKey);
            #endregion

            token.SetPayload(newKeyToken);
            token.Sign(accountKey);

            // Update account
            response = controller.ChangeKey(new AcmeRequest
            {
                Path = $"{Application.BaseAddress}key-change",
                Method = "POST",
                Token = token,
            });

            Assert.Equal(409, response.StatusCode);
            Assert.Equal($"{Application.BaseAddress}acct/2", response.Headers.Location);

            var content = response.GetContent<Protocol.Error>();
            Assert.Equal(Protocol.ErrorType.Malformed, content.Type);
            Assert.NotNull(content.Detail);
        }
        #endregion
    }
}
