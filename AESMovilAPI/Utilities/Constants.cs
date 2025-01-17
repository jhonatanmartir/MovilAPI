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
        public const string CAESS_NAME = "CAESS";
        public const string DEUSEM_NAME = "DEUSEM";
        public const string EEO_NAME = "EEO";
        public const string CLESA_NAME = "CLESA";

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

        public const string CONF_OMS_BASE = "OMS:Base";
        public const string CONF_OMS_USER = "OMS:Usr";
        public const string CONF_OMS_PASSWORD = "OMS:Pwd";
        public const string CONF_OMS_IVRADMS = "OMS:ivradms";
        #endregion

        #region "Security"
        public const string ENCRYPT_KEY = "DdAWpGHUnLbvmGhkbydMp4qJNySZ98VAYUgXewR6trs=";
        public const string SECRECT_KEY_IV = "qJBO+FfIMogaSgmdPAZNFg==";
        #endregion

        public const string SUCCESS = "success";

        #region Types
        public const int TIP_REG1_DATOS_SUMINISTRO = 1;
        public const int TIP_REG2_TARIFA_APLICADA = 2;
        public const int TIP_REG3_TIPO_MED_LECTURAS = 3;
        public const int TIP_REG4_CONCEPTOS = 4;
        public const int TIP_REG5_OTROS_CONCEPTOS = 5;
        public const int TIP_REG6_VENTAS_EXENTAS = 6;
        public const int TIP_REG7_ALCALDIA = 7;
        public const int TIP_REG8_OTROS_SERVICIOS = 8;
        public const int TIP_REG9_COMPENSACIONES = 9;
        public const int TIP_REG10_MEDIDOR_LEVANTADO = 10;

        public const string TIP_REG1_DATOS_SUMINISTRO_STR = "01";
        public const string TIP_REG2_TARIFA_APLICADA_STR = "02";
        public const string TIP_REG3_TIPO_MED_LECTURAS_STR = "03";
        public const string TIP_REG4_CONCEPTOS_STR = "04";
        public const string TIP_REG5_OTROS_CONCEPTOS_STR = "05";
        public const string TIP_REG6_VENTAS_EXENTAS_STR = "06";
        public const string TIP_REG7_ALCALDIA_STR = "07";
        public const string TIP_REG8_OTROS_SERVICIOS_STR = "08";
        public const string TIP_REG9_COMPENSACIONES_STR = "09";
        public const string TIP_REG10_MEDIDOR_LEVANTADO_STR = "10";
        #endregion

        public const string FILLER = "0.000000";

        #region Extensions
        public const string EXT_PDF = ".pdf";
        public const string EXT_PNG = ".png";
        public const string EXT_ZIP = ".zip";
        public const string EXT_JSON = ".json";
        #endregion

        public const string HTTP_CLIENT_NAME = "DefaultClient";
    }
}
