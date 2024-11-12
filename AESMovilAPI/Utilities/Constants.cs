namespace AESMovilAPI.Utilities
{
    public class Constants
    {
        public const string SV10_CAESS = "2";
        public const string SV20_DEUSEM = "3";
        public const string SV30_EEO = "4";
        public const string SV40_CLESA = "5";
        public const string SV10_CAESS_CODE = "SV10";
        public const string SV20_DEUSEM_CODE = "SV20";
        public const string SV30_EEO_CODE = "SV30";
        public const string SV40_CLESA_CODE = "SV40";

        public const string EBILLAPI_BEARER = "EBillAPI_Bearer";
        public const string RP_TOKEN = "RP_TOKEN";
        public const string AESMOVIL_BEARER = "AM_TOKEN";

        #region "Config"
        public const string CONF_SAP_BASE = "SAP:Base";
        public const string CONF_SAP_ENVIRONMENT = "SAP:ID";
        public const string CONF_SAP_USER = "SAP:Usr";
        public const string CONF_SAP_PASSWORD = "SAP:Pwd";
        public const string CONF_SAP_TOKEN = "SAP:Token";

        public const string CONF_PAGADITO_ENDPOINT = "PagaditoParams:Endpoint";
        public const string CONF_PAGADITO_USER = "PagaditoParams:Usr";
        public const string CONF_PAGADITO_PASSWORD = "PagaditoParams:Pwd";

        public const string CONF_PAYWAY_ENDPOINT = "PaywayParams:Endpoint";
        public const string CONF_PAYWAY_TOKEN = "PaywayParams:Token";
        public const string CONF_PAYWAY_USER_CAESS = "PaywayParams:UsrCAESS";
        public const string CONF_PAYWAY_USER_EEO = "PaywayParams:UsrEEO";
        public const string CONF_PAYWAY_USER_DEUSEM = "PaywayParams:UsrDEUSEM";
        public const string CONF_PAYWAY_USER_CLESA = "PaywayParams:UsrCLESA";
        public const string CONF_PAYWAY_ID_CAESS = "PaywayParams:IdCAESS";
        public const string CONF_PAYWAY_ID_EEO = "PaywayParams:IdEEO";
        public const string CONF_PAYWAY_ID_DEUSEM = "PaywayParams:IdDEUSEM";
        public const string CONF_PAYWAY_ID_CLESA = "PaywayParams:IdCLESA";

        public const string CONF_SAP_REAL_PAYMENT_TOKEN = "RealPayment:Token";
        public const string CONF_REAL_PAYMENT_CASH_POINT = "RealPayment:CashPoint";
        public const string CONF_REAL_PAYMENT_CASH_POINT_OFFICE = "RealPayment:CashPointOffice";
        #endregion

        #region "Security"
        public const string ENCRYPT_KEY = "DdAWpGHUnLbvmGhkbydMp4qJNySZ98VAYUgXewR6trs=";
        public const string SECRECT_KEY_IV = "qJBO+FfIMogaSgmdPAZNFg==";
        #endregion
    }
}
