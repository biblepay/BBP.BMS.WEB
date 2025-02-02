using BBPAPI;
using BMSCommon.Model;
using System.Threading.Tasks;
using static BMSCommon.Encryption;

namespace BiblePay.BMS.DSQL
{
    public static class UIWallet
    {
        public static double ConvertUSDToBiblePay(double nUSD)
        {
            price1 nBTCPrice = PricingService.GetCryptoPrice("BTC/USD");
            price1 nBBPPrice = PricingService.GetCryptoPrice("BBP/BTC");
            double nUSDBBP = nBTCPrice.AmountUSD * nBBPPrice.Amount;
            double nOut = nUSD / (nUSDBBP + .000000001);
            return nOut;
        }

        public static double ConvertBBPToUSD(double nBBP)
        {
            price1 nBTCPrice = PricingService.GetCryptoPrice("BTC/USD");
            price1 nBBPPrice = PricingService.GetCryptoPrice("BBP/BTC");
            double nUSDBBP = nBTCPrice.AmountUSD * nBBPPrice.Amount;
            double nOut = nUSDBBP * nBBP;
            return nOut;
        }


        // This is only used by MFA - it derives a bbp key by 2fa seed
        public static async Task<BBPKeyPair> GetKeyPairByGUID(bool fTestNet, string sGUID, string sNonce = "")
        {
            BBPKeyPair p = new BBPKeyPair();
            string sDerivationSource = sGUID + sNonce;
            p = await BBPAPI.Interface.Repository.DeriveKey(fTestNet, sDerivationSource);
            return p;
        }


        // Only used by GenerateToken() proof of concept:
        public static async Task<BBPKeyPair> GetKeyPair3(bool fTestNet, string sDerivSource)
        {
            BBPKeyPair k = await BBPAPI.Interface.Repository.DeriveKey(fTestNet, sDerivSource);
            return k;
        }

    }
}
