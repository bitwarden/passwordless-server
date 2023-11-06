using System.Formats.Asn1;
using System.Net;
using System.Net.Http.Json;
using Fido2NetLib;
using FluentAssertions;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.Integration.Tests;

public class SignInTests : IClassFixture<PasswordlessApiFactory>
{
    private readonly HttpClient _httpClient;

    public SignInTests(PasswordlessApiFactory factory)
    {
        _httpClient = factory.CreateClient().AddPublicKey();
    }

    [Fact]
    public async Task Server_returns_encoded_assertion_options_to_be_used_for_sign_in()
    {
        var request = new SignInBeginDTO
        {
            Origin = "http://integration-tests.passwordless.dev",
            RPID = Environment.MachineName,
        };

        var response = await _httpClient.PostAsJsonAsync("/signin/begin", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInResponse = await response.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        signInResponse.Should().NotBeNull();
        signInResponse!.Session.Should().StartWith("session_");
        signInResponse.Data.RpId.Should().Be(request.RPID);
        signInResponse.Data.Status.Should().Be("ok");
    }

    [Fact]
    public async Task Server_returns_encoded_token_to_verify_sign_in_has_been_completed()
    {
        var request = new SignInCompleteDTO
        {
            Origin = "http://integration-tests.passwordless.dev",
            RPID = Environment.MachineName,
            ServerName = Environment.MachineName,
            Response = new AuthenticatorAssertionRawResponse
            {
                Id = new byte[] { },
                RawId = new byte[] { },
                Response = null,
                Type = null,
                Extensions = null
            },
            Session = null
        };
    }

    public static byte[] EcDsaSigFromSig(ReadOnlySpan<byte> sig, int keySizeInBits)
    {
        var coefficientSize = (int)Math.Ceiling((decimal)keySizeInBits / 8);
        var r = sig.Slice(0, coefficientSize);
        var s = sig.Slice(sig.Length - coefficientSize);

        var writer = new AsnWriter(AsnEncodingRules.BER);

        ReadOnlySpan<byte> zero = new byte[1] { 0 };

        using (writer.PushSequence())
        {
            writer.WriteIntegerUnsigned(r.TrimStart(zero));
            writer.WriteIntegerUnsigned(s.TrimStart(zero));
        }

        return writer.Encode();
    }
}