var certificate = new X509Certificate2(X509Certificate.CreateFromCertFile("example.crt"));

services
    .AddHttpClient()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new SocketsHttpHandler
        {
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificationValidationCallback = SslValidation.CreateValidator(certificate),
            }
        };
    });

public static class SslValidation
{
    public static RemoteCertificationValidationCallback CreateValidator(X509Certificate2 certificate)
    {
        return CreateValidator(new X509Certificate2Collection(certificate));
    }

    // https://github.com/dotnet/runtime/issues/39835
    // https://github.com/dotnet/runtime/issues/39835#issuecomment-663106476
    public static RemoteCertificationValidationCallback CreateValidator(X509Certificate2Collection certificates)
    {
        bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // Missing cert or the destination hostname wasn't valid for the cert.
            if((errors & ~SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                return false;
            }

            chain.ChainPolicy.CustomTrustStore.Clear();
            chain.ChainPolicy.CustomTrustStore.AddRange(certificates);
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

            return chain.Build((X509Certificate2)certificate);
        }

        return Validator;
    }
}