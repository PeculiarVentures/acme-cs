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
            Assert.NotNull(response.ReplayNonce);
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
            Assert.NotNull(response.ReplayNonce);
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
                Nonce = response.ReplayNonce,
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
                Nonce = response.ReplayNonce,
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
                Nonce = response.ReplayNonce,
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
                Nonce = response.ReplayNonce,
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

            Assert.Null(content.TermsOfServiceAgreed);
        }

        [Fact(DisplayName = "Account: Create a new account with enabled TermsOfService with TermsOfServiceAgreed:false")]
        public void Account_New_TermsOfService_Enabled_TermsOfServiceAgreed_False()
        {
            AcmeResponse response = null;
            using var provider = Application.CreateProvider(o => {
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
                Nonce = response.ReplayNonce,
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

        #endregion
    }
}
