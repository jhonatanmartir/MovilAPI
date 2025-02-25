namespace AESMovilAPI.Utilities
{
    public static class ApiEndpoint
    {
        private static string _base = string.Empty;
        private static string _mandante = string.Empty;
        public static void Initialize(IConfiguration config)
        {
            _base = config.GetValue<string>(Constants.CONF_SAP_BASE)!;
            _mandante = config.GetValue<string>(Constants.CONF_SAP_ENVIRONMENT)!;
        }

        #region "SAP"
        private static string GetSAPCommon => $"{_base}/gw/odata/SAP/CIS{_mandante}_";

        /// <summary>
        /// Obtener historico de facturacion existente en SAP.
        /// </summary>
        /// <remarks>NS-12698</remarks>
        /// <param name="nc">Número de cuenta contrato.</param>
        /// <param name="fromDate">Fecha desde cuando se busca el historico.</param>
        /// <param name="toDate">Fecha hasta donde se obtendrá el historico.</param>
        /// <returns>URL que devuelve la lista de la información del historico.</returns>
        public static string GetSAPBillHistory(string nc, string fromDate, string toDate) => $"{GetSAPCommon}BIL_BILLIMAGEPREVIEWES_AZUREAPPSERVICES_TO_SAPCIS;v=1/InvHistSummarySet(Nic='{nc}',Ab='{fromDate}',Bis='{toDate}')";

        /// <summary>
        /// Obtener historico de alcaldia existemte en SAP.
        /// </summary>
        /// <remarks>NS-12674</remarks>
        /// <param name="nc">Número de cuenta contrato.</param>
        /// <param name="fromDate">Fecha desde cuando se busca el historico.</param>
        /// <param name="toDate">Fecha hasta donde se obtendrá el historico.</param>
        /// <returns>URL que devuelve la información del historico.</returns>
        public static string GetSAPMayoralHistory(string nc, string fromDate, string toDate) => $"{GetSAPCommon}ACC_GETHISTORICOCARGOSALCALDIA_AZUREAPPSSERVICES_TO_SAPCIS;v=1/GetHistoricoAlcaldiaSet(Nic='{nc}',Fechainicio='{fromDate}',Fechafin='{toDate}')";

        /// <summary>
        /// Obtener saldo de la factura con RealPayment SAP.
        /// </summary>
        /// <returns>Detalle del saldo de la factura.</returns>
        public static string GetSAPBalance => $"{_base}/http/{_mandante.TrimStart('_')}OpenItemSummaryByElements";

        /// <summary>
        /// Obtener JSON de la factura certificada en SAP.
        /// </summary>
        /// <param name="documentNumber">Número de documento de la factura.</param>
        /// <returns>URL para obtener JSON con el detalle de la factura.</returns>
        public static string GetSAPJson(string documentNumber) => $"{GetSAPCommon}ACC_GETINVOICEFORMJSON_AZUREAPPSSERVICES_TO_SAPCIS;v=1/GetInvoiceToJsonSet('{documentNumber}')";
        #endregion

        #region "Others"
        /// <summary>
        /// Obtener JSON de la factura certificada en SAP.
        /// </summary>
        /// <param name="documentNumber">Número de documento de la factura.</param>
        /// <returns>URL para obtener JSON con el detalle de la factura.</returns>
        public static string GetBillDTE(string documentNumber) => $"{GetSAPCommon}_ACC_GETINVOICEFORMJSON_AZUREAPPSSERVICES_TO_SAPCIS;v=1/GetInvoiceToJsonSet('{documentNumber}')";
        #endregion
    }
}
