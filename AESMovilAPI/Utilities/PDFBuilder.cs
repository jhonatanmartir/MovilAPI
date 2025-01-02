﻿using AESMovilAPI.DTOs;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;

namespace AESMovilAPI.Utilities
{
    public class PDFBuilder
    {
        private string _rootPath;
        private DataTable? _header;
        private DataTable? _detail;

        public string DocNumber { set; get; }
        public string FFact { set; get; }
        public string CodUnicom { set; get; }
        public string Company { set; get; }
        public PDFBuilder(string rootPath, string id)
        {
            DocNumber = id;
            FFact = "";
            CodUnicom = "";
            Company = "";
            _rootPath = rootPath;
        }

        public byte[]? DoFillFormByte(string jsonHeader, string jsonDetail, bool isPaid = false, List<CargosAnuladosDto>? anulados = null)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonHeader.Trim()) && string.IsNullOrEmpty(jsonDetail.Trim()))
                {
                    return null;
                }

                _header = JsonConvert.DeserializeObject<DataTable>(jsonHeader);
                _detail = JsonConvert.DeserializeObject<DataTable>(jsonDetail);

                DocNumber = GetValueFromDS("SIMBOLO_VAR");
                FFact = GetValueFromDS("F_FACT");
                CodUnicom = GetValueFromDS("COD_UNICOM");

                StampingProperties stamping = new StampingProperties();

                using (MemoryStream msTemp = new MemoryStream())
                {
                    using (PdfWriter pdfWriter = new PdfWriter(msTemp))
                    {
                        // Since creating a reader/writer in iText causes the underlying
                        // stream to close, we need to prevent that with this call
                        //pdfWriter.SetCloseStream(false);
                        string templatePath = Path.Combine(_rootPath, "Sources", "Template", "FACT122024a.pdf");
                        using (PdfDocument doc = new PdfDocument(new PdfReader(templatePath), pdfWriter, stamping))
                        {
                            //PdfDocument doc = new PdfDocument(new PdfReader(templatePath), new PdfWriter(destPath), stamping);
                            PdfAcroForm form = PdfFormCreator.GetAcroForm(doc, true);

                            //Font
                            //PdfFont font = PdfFontFactory.CreateFont(Utils.Utils.GetDirFolderResources(Constants.DIR_FONTS, Constants.FONT_FIRA_SANS_REGULAR), PdfEncodings.UTF8);
                            //font.SetSubset(false);

                            //PdfFont fontBold = PdfFontFactory.CreateFont(Utils.Utils.GetDirFolderResources(Constants.DIR_FONTS, Constants.FONT_FIRA_SANS_BOLD), PdfEncodings.UTF8);
                            //fontBold.SetSubset(false);

                            List<IdValueDto> dataList = new List<IdValueDto>();
                            string promedio = string.Empty;
                            decimal suma = 0;
                            string npeValid = string.Empty;

                            //Campos
                            if (_header != null)
                            {
                                PdfFormField field;

                                foreach (DataColumn col in _header.Columns)
                                {
                                    string fieldName = col.ColumnName;
                                    string fielValue = GetValueFromDS(fieldName);

                                    try
                                    {
                                        field = form.GetField("FH_" + fieldName);

                                        if (field != null)
                                        {
                                            //Set values to form
                                            field.SetValue(fielValue);
                                            //field.SetValue(fielValue, font, Constants.FONT_SIZE_FORM);

                                            //Customizacion
                                            //if (fieldName.Equals("NIC"))
                                            //{
                                            //    field = form.GetField("F_" + fieldName + "_C");
                                            //    field.SetValue(fielValue);
                                            //    //field.SetValue(fielValue, fontBold, Constants.FONT_SIZE_FORM_NIC);
                                            //}

                                            if (fieldName.Equals("SUBTOTAL_CON_GRA"))
                                            {
                                                field.SetValue(fielValue);
                                                //field.SetValue(fielValue, fontBold, Constants.FONT_SIZE_FORM);
                                            }

                                            if (fieldName.Equals("SUBTOTAL_CV_GRA"))
                                            {
                                                field.SetValue(fielValue);
                                                //field.SetValue(fielValue, fontBold, Constants.FONT_SIZE_FORM);
                                            }

                                            if (fieldName.Equals("IVA"))
                                            {
                                                field.SetValue(fielValue);
                                                //field.SetValue(fielValue, fontBold, Constants.FONT_SIZE_FORM);
                                            }

                                            if (fieldName.Equals("SUBTOTAL_SEGURO"))
                                            {
                                                field.SetValue(fielValue);
                                                //field.SetValue(fielValue, fontBold, Constants.FONT_SIZE_FORM);
                                                suma = suma + Helper.ToDecimal(fielValue);
                                            }

                                            if (fieldName.Equals("SUBTOTAL_ALCALDIA"))
                                            {
                                                if (anulados == null || anulados.Count == 0)
                                                {
                                                    field.SetValue(fielValue);
                                                    //field.SetValue(fielValue, fontBold, Constants.FONT_SIZE_FORM);
                                                    suma = suma + Helper.ToDecimal(fielValue);
                                                }
                                                else
                                                {
                                                    field.SetValue("0.00");
                                                    //field.SetValue("0.00", fontBold, Constants.FONT_SIZE_FORM);
                                                }
                                            }

                                            if (fieldName.Equals("TOTAL_ELECT_DOL"))
                                            {
                                                field.SetValue(fielValue);
                                                //field.SetValue(fielValue, fontBold, Constants.FONT_SIZE_FORM);
                                                suma = suma + Helper.ToDecimal(fielValue);
                                            }

                                            if (fieldName.Equals("PAIS"))
                                            {
                                                if (!string.IsNullOrEmpty(fielValue))
                                                {
                                                    field.SetValue("PAIS " + fielValue);
                                                    //field.SetValue("PAIS " + fielValue, fontBold, Constants.FONT_SIZE_FORM);
                                                }
                                            }

                                            if (fieldName.Equals("TOTAL_SEGURO_DOL"))
                                            {
                                                field.SetValue(fielValue);
                                                //field.SetValue(fielValue, fontBold, Constants.FONT_SIZE_FORM);
                                            }

                                            if (fieldName.Equals("RUTA"))
                                            {
                                                string ruta = GetValueFromDS("COD_UNICOM") + "  " + fielValue + "  " +
                                                    GetValueFromDS("NUM_ITIN") + "  " + GetValueFromDS("AOL_FIN");
                                                field.SetValue(ruta);
                                                //field.SetValue(ruta, font, Constants.FONT_SIZE_FORM);
                                            }

                                            if (fieldName.Equals("MESES_PEND") && fielValue == "1" ||
                                                fieldName.Equals("MESES_PEND") && fielValue == "01")
                                            {
                                                field = form.GetField("FH_MENSAJE_1");
                                                string mensaje = "ESTIMADO CLIENTE USTED TIENE 2 FACTURAS PENDIENTES DE PAGO, SI SU CONSUMO ES HASTA 300KWH EN TARIFA RESIDENCIAL, A PARTIR DE LA FECHA DE VENCIMIENTO CUENTA CON 72 HORAS ADICIONALES PARA EFECTUAR SU PAGO, DE NO REALIZARLO SU SERVICIO SERÁ SUSPENDIDO; SI SU CONUSMO ES MAYOR A 300KWH SU SERVICIO SERÁ SUSPENDIDO AL DIA SIGUIENTE.";
                                                field.SetValue(mensaje);
                                                //field.SetValue(mensaje, fontBold, Constants.FONT_SIZE_FORM);
                                            }

                                            //Tipo de factura
                                            if (fieldName.Equals("TIPO_DOC"))   // TODO cambio nombre
                                            {
                                                switch (fielValue)
                                                {
                                                    case "E":   // FC560
                                                        field.SetValue("FACTURA DE EXPORTACION");
                                                        //field.SetValue("FACTURA DE EXPORTACION", fontBold, Constants.FONT_SIZE_FORM);
                                                        break;
                                                    case "F":   // FC500, FC580
                                                        field.SetValue("FACTURA");
                                                        //field.SetValue("FACTURA", fontBold, Constants.FONT_SIZE_FORM);
                                                        break;
                                                    case "C":   // FC540, FC550, FC570
                                                        field.SetValue("CREDITO FISCAL");
                                                        //field.SetValue("CREDITO FISCAL", fontBold, Constants.FONT_SIZE_FORM);
                                                        break;
                                                    default: break;
                                                }
                                            }

                                            // Documento del cliente
                                            if (fieldName.Equals("TIP_IDEN"))
                                            {
                                                if (string.IsNullOrEmpty(fielValue))
                                                {
                                                    field.SetValue("NIT");
                                                    //field.SetValue("NIT", font, Constants.FONT_SIZE_FORM);
                                                    field = form.GetField("FH_NUM_DOC");
                                                    field.SetValue(GetValueFromDS("NIT_TIT_PAGO"));
                                                    //field.SetValue(GetValueFromDS("NIT_TIT_PAGO"), fontBold, Constants.FONT_SIZE_FORM);
                                                }
                                                else
                                                {
                                                    switch (fielValue)
                                                    {
                                                        case "36":   // TD001 - N.I.T.
                                                            field.SetValue("NIT");
                                                            //field.SetValue("NIT", font, Constants.FONT_SIZE_FORM);
                                                            break;
                                                        case "03":   // TD002 - Pasaporte
                                                            field.SetValue("PASAPORTE");
                                                            //field.SetValue("PASAPORTE", font, Constants.FONT_SIZE_FORM);
                                                            break;
                                                        case "02":   // TD003 - Carnet Residencial
                                                            field.SetValue("CARNET RESIDENCIAL");
                                                            //field.SetValue("CARNET RESIDENCIAL", font, Constants.FONT_SIZE_FORM);
                                                            break;
                                                        case "37":   // TD005 - Otros
                                                            field.SetValue("OTROS");
                                                            //field.SetValue("OTROS", font, Constants.FONT_SIZE_FORM);
                                                            break;
                                                        case "13":   // TD008 - Documento Unico
                                                            field.SetValue("DUI");
                                                            //field.SetValue("DUI", font, Constants.FONT_SIZE_FORM);
                                                            break;
                                                        default: break;
                                                    }
                                                }
                                            }
                                        }

                                        //Custom fields
                                        if (fieldName.Equals("EMPRESA"))
                                        {
                                            string empresaName = string.Empty;
                                            string registroValue = string.Empty;
                                            string nitValue = string.Empty;
                                            string addressValue = string.Empty;
                                            string CompanyInfo = string.Empty;

                                            switch (fielValue)
                                            {
                                                case "2":   //CAESS
                                                    Company = "CAESS";
                                                    CompanyInfo = "Compañia de Alumbrado Eléctrico de San Salvador S.A. de C.V.\n" +
                                                        "Edificio Corporativo CAESS Col. San Antonio,Calle El Bambú\n" +
                                                        "Ayutuxtepeque, San Salvador\n" +
                                                        "Registro: 321-2\n" +
                                                        "NIT 0614-171190-001-3 / Giro: Distribución de Energía Eléctrica";
                                                    break;
                                                case "3":   //EEO
                                                    Company = "EEO";
                                                    CompanyInfo = "EMPRESA ELECTRICA DE ORIENTE\n" +
                                                        "Final 8a. Calle Pte., calle a C. Pacifica, Edif. Jalacatal\n" +
                                                        "San Miguel\n" +
                                                        "Registro: 90597-6\n" +
                                                        "NIT 0614-161195-103-0 / Giro: Distribución de Energía Eléctrica";
                                                    break;
                                                case "4":   //DEUSEM
                                                    Company = "DEUSEM";
                                                    CompanyInfo = "DEUSEM\n" +
                                                        "Centro Comercial Puerta de Oriente\n" +
                                                        "Usulutan, local 2\n" +
                                                        "Registro: 3267-0\n" +
                                                        "NIT 1123-260757-001-0 / Giro: Distribución de Energía Eléctrica";
                                                    break;
                                                case "5":   //CLESA
                                                    Company = "CLESA";
                                                    CompanyInfo = "AES CLESA Y CIA S.EN C.DE C.V.\n" +
                                                        "23 Av. Sur y 5a. Calle Ote., Barrio San Rafael\n" +
                                                        "Santa Ana\n" +
                                                        "Registro: 2023-0\n" +
                                                        "NIT 0210-120792-0015 / Giro: Distribución de Energía Eléctrica";
                                                    break;
                                                default:
                                                    break;
                                            }

                                            //field = form.GetField("F_EMPRESA");
                                            //field.SetValue(empresaNameVal, fontBold, Constants.FONT_SIZE_FORM_LONG);
                                            //field.SetColor(DeviceRgb.WHITE);
                                            field = form.GetField("X_EMPRESA");
                                            field.SetValue(Company);
                                            field = form.GetField("X_INFO");
                                            field.SetValue(CompanyInfo);
                                            //field.SetValue(empresaName, font, Constants.FONT_SIZE_FORM_SMALL);
                                            //field = form.GetField("X_REGISTRO");
                                            //field.SetValue(registroValue);
                                            ////field.SetValue(registroValue, font, Constants.FONT_SIZE_FORM_SMALL);
                                            //field = form.GetField("X_NIT");
                                            //field.SetValue(nitValue);
                                            ////field.SetValue(nitValue, font, Constants.FONT_SIZE_FORM_SMALL);
                                            //field = form.GetField("X_DIRECCION");
                                            //field.SetValue(addressValue);
                                            ////field.SetValue(addressValue, font, Constants.FONT_SIZE_FORM_SMALL);
                                            //field = form.GetField("X_EMPRESA_LABEL");
                                            //field.SetValue(empresaNameVal);
                                            //field.SetValue(empresaNameVal, fontBold, Constants.FONT_SIZE_FORM);
                                        }

                                        //if (fieldName.Equals("CONTROL_NUMBER"))
                                        //{
                                        //    field = form.GetField("F_VL_CONTROLNUMBER");
                                        //    field.SetValue(fielValue, font, Constants.FONT_SIZE_FORM);
                                        //}
                                    }
                                    catch (Exception ex)
                                    {
                                        //Log.Ex(ex);
                                    }
                                }

                                //Totales
                                field = form.GetField("X_TOTAL_DOL");
                                field.SetValue(suma.ToString());
                                //field.SetValue(suma.ToString(), fontBold, Constants.FONT_SIZE_FORM);

                                // Datos para grafica
                                dataList.Add(new IdValueDto() { Id = Helper.ParseStrDate(GetValueFromDS("BARRA6_F_LECT"), "yyyy-MM-dd", "dd MMM"), Value = GetValueFromDS("BARRA6_CSMO", true) });
                                dataList.Add(new IdValueDto() { Id = GetValueFromDS("BARRA5_F_LECT"), Value = GetValueFromDS("BARRA5_CSMO", true) });
                                dataList.Add(new IdValueDto() { Id = GetValueFromDS("BARRA4_F_LECT"), Value = GetValueFromDS("BARRA4_CSMO", true) });
                                dataList.Add(new IdValueDto() { Id = GetValueFromDS("BARRA3_F_LECT"), Value = GetValueFromDS("BARRA3_CSMO", true) });
                                dataList.Add(new IdValueDto() { Id = GetValueFromDS("BARRA2_F_LECT"), Value = GetValueFromDS("BARRA2_CSMO", true) });
                                dataList.Add(new IdValueDto() { Id = GetValueFromDS("BARRA1_F_LECT"), Value = GetValueFromDS("BARRA1_CSMO", true) });
                                promedio = GetValueFromDS("CONSUMO_PRO_MES");

                                // Creacion de codigo de barras
                                string npe = GetValueFromDS("NPE");
                                string npeAlca = GetValueFromDS("NPE_ALCA");
                                npeAlca = anulados == null || anulados.Count == 0 ? npeAlca : string.Empty;
                                npeValid = string.IsNullOrEmpty(npeAlca.Trim()) ? npe : npeAlca;

                                npeValid = Helper.GetStyledNPE(npeValid);
                                field = form.GetField("FH_NPE");
                                field.SetValue(npeValid);
                                //field.SetValue(npeValid, font, Constants.FONT_SIZE_BIG);
                            }

                            //Detalles
                            if (_detail != null)
                            {
                                int counterTipDatosSuministro = 1;
                                int counterTipLecMed = 1;
                                int counterTipRegConceptos = 1;
                                int counterTipRegCargVarios = 1;
                                int counterTipRegAlcaldia = 1;
                                int counterTipRegVentasExentas = 1;
                                int counterTipRegOtrosServicios = 1;
                                int counterTipLecMedLev = 1;
                                int counterTarifa = 1;

                                foreach (DataRow row in _detail.Rows)
                                {
                                    try
                                    {
                                        int tipReg = GetIntValueFromDR(row, "TIP_REG");
                                        string stringDetail = GetValueFromDR(row, "DATO_PRINT");
                                        string[] words = Regex.Split(stringDetail, @"\s{2,}"); //ER dividirá la cadena siempre que aparezcan dos o más espacios consecutivos

                                        PdfFormField field;
                                        switch (tipReg)
                                        {
                                            case Constants.TIP_REG1_DATOS_SUMINISTRO:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField("FD_CALC_CONSUMO_" + counterTipDatosSuministro);
                                                        field.SetValue(words[0]);
                                                        //field.SetValue(words[0], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_MEDIDOR_" + counterTipDatosSuministro);
                                                        field.SetValue(words[1]);
                                                        //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_MULT_" + counterTipDatosSuministro);
                                                        field.SetValue(words[2]);
                                                        //field.SetValue(words[2], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_TIPO_" + counterTipDatosSuministro);
                                                        field.SetValue(words[3]);
                                                        //field.SetValue(words[3], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_MEDI_" + counterTipDatosSuministro);
                                                        field.SetValue(words[4]);
                                                        //field.SetValue(words[4], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }
                                                    counterTipDatosSuministro++;
                                                }
                                                break;
                                            case Constants.TIP_REG2_TARIFA_APLICADA:
                                                words = stringDetail.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);       // Separar por espacios (incluye múltiples espacios)
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField($"FD_INICIO_{counterTarifa}");
                                                        if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                        {
                                                            field.SetValue(words[0].Trim());
                                                            //field.SetValue(words[0].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        }

                                                        //field = form.GetField("FD_FINAL");
                                                        //if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                        //{
                                                        //    field.SetValue(words[1].Trim());
                                                        //    //field.SetValue(words[1].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        //}

                                                        field = form.GetField($"FD_CARGO_COM_{counterTarifa}");
                                                        if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                        {
                                                            field.SetValue(words[1].Trim());
                                                            //field.SetValue(words[2].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        }

                                                        field = form.GetField($"FD_BLOQUE_{counterTarifa}");
                                                        if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                        {
                                                            field.SetValue(words[3].Trim());
                                                            //field.SetValue(words[3].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        }

                                                        try
                                                        {
                                                            field = form.GetField($"FD_UPR_{counterTarifa}");
                                                            if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                            {
                                                                field.SetValue(words[2].Trim());
                                                                //field.SetValue(words[2].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }

                                                        try
                                                        {
                                                            field = form.GetField($"FD_PUNTA_{counterTarifa}");
                                                            if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                            {
                                                                field.SetValue(words[4].Trim());
                                                                //field.SetValue(words[4].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                            }

                                                            field = form.GetField($"FD_VALLE_{counterTarifa}");
                                                            if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                            {
                                                                field.SetValue(words[6].Trim());
                                                                //field.SetValue(words[6].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                            }

                                                            field = form.GetField($"FD_RESTO_{counterTarifa}");
                                                            if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                            {
                                                                field.SetValue(words[5].Trim());
                                                                //field.SetValue(words[5].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                            }

                                                            field = form.GetField($"FD_DISTRIBUCION_{counterTarifa}");
                                                            if (field.GetValue() == null || field.GetValue().ToString() == Constants.FILLER)
                                                            {
                                                                field.SetValue(words[7].Trim());
                                                                //field.SetValue(words[7].Trim(), font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            //Log.Err(ex.Message);
                                                        }
                                                        counterTarifa++;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }
                                                }
                                                break;
                                            case Constants.TIP_REG3_TIPO_MED_LECTURAS:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField("FD_TIPO_LECT_" + counterTipLecMed);
                                                        field.SetValue(words[1]);
                                                        //field.SetValue(words[0], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_LECT_ACT_" + counterTipLecMed);
                                                        field.SetValue(words[2]);
                                                        //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_LECT_ANT_" + counterTipLecMed);
                                                        field.SetValue(words[3]);
                                                        //field.SetValue(words[2], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_CONSUMO_" + counterTipLecMed);
                                                        field.SetValue(words[4]);
                                                        //field.SetValue(words[3], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }

                                                    counterTipLecMed++;
                                                }
                                                break;
                                            case Constants.TIP_REG4_CONCEPTOS:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField("FD_CONCEPTO_" + counterTipRegConceptos);
                                                        field.SetValue(words[0]);
                                                        //field.SetValue(words[0], font, Constants.FONT_SIZE_FORM);
                                                        field = form.GetField("FD_CONCEPTO_VAL_" + counterTipRegConceptos);
                                                        field.SetValue(words[1]);
                                                        //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }

                                                    counterTipRegConceptos++;
                                                }
                                                break;
                                            case Constants.TIP_REG5_CARG_VARIOS_OTROS_INGRESOS:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField("FD_CONCEPTO_CV_" + counterTipRegCargVarios);
                                                        field.SetValue(words[0]);
                                                        //field.SetValue(words[0], font, Constants.FONT_SIZE_FORM);
                                                        field = form.GetField("FD_CONCEPTO_CV_VAL_" + counterTipRegCargVarios);
                                                        field.SetValue(words[1]);
                                                        //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }

                                                    counterTipRegCargVarios++;
                                                }
                                                break;
                                            case Constants.TIP_REG6_VENTAS_EXENTAS:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField("FD_CONCEPTO_EX_" + counterTipRegVentasExentas);
                                                        field.SetValue(words[0]);
                                                        //field.SetValue(words[0], font, Constants.FONT_SIZE_FORM);
                                                        field = form.GetField("FD_CONCEPTO_EX_VAL_" + counterTipRegVentasExentas);
                                                        field.SetValue(words[1]);
                                                        //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }

                                                    counterTipRegVentasExentas++;
                                                }
                                                break;
                                            case Constants.TIP_REG7_ALCALDIA:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        if (anulados == null)
                                                        {
                                                            field = form.GetField("FD_CONCEPTO_ALCA_" + counterTipRegAlcaldia);
                                                            field.SetValue(words[0]);
                                                            //field.SetValue(words[0], font, Constants.FONT_SIZE_FORM);
                                                            field = form.GetField("FD_CONCEPTO_ALCA_VAL_" + counterTipRegAlcaldia);
                                                            field.SetValue(words[1]);
                                                            //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }

                                                    counterTipRegAlcaldia++;
                                                }
                                                break;
                                            case Constants.TIP_REG8_OTROS_SERVICIOS:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField("FD_CONCEPTO_OTROS_" + counterTipRegOtrosServicios);
                                                        field.SetValue(words[0]);
                                                        //field.SetValue(words[0], font, Constants.FONT_SIZE_FORM);
                                                        field = form.GetField("FD_CONCEPTO_OTROS_VAL_" + counterTipRegOtrosServicios);
                                                        field.SetValue(words[1]);
                                                        //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }

                                                    counterTipRegOtrosServicios++;
                                                }
                                                break;
                                            case Constants.TIP_REG9_RETENCIONES:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField("FD_RETENCION");
                                                        field.SetValue(words[0]);
                                                        //field.SetValue(words[0], font, Constants.FONT_SIZE_FORM);
                                                        field = form.GetField("FD_RETENCION_VAL");
                                                        field.SetValue(words[1]);
                                                        //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }
                                                }
                                                break;
                                            case Constants.TIP_REG10_MEDIDOR_LEVANTADO:
                                                if (words.Length > 0)
                                                {
                                                    try
                                                    {
                                                        field = form.GetField("FD_LECT_ACT_ML_" + counterTipLecMedLev);
                                                        field.SetValue(words[1]);
                                                        //field.SetValue(words[1], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_LECT_ANT_ML_" + counterTipLecMedLev);
                                                        field.SetValue(words[2]);
                                                        //field.SetValue(words[2], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                        field = form.GetField("FD_CONSUMO_ML_" + counterTipLecMedLev);
                                                        field.SetValue(words[3]);
                                                        //field.SetValue(words[3], font, Constants.FONT_SIZE_FORM_MEDIUM);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Log.Ex(ex);
                                                    }

                                                    counterTipLecMedLev++;
                                                }
                                                break;

                                            default: break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //Log.Ex(ex);
                                    }
                                }
                            }

                            //Imagenes
                            try
                            {
                                string imgName = DocNumber + Constants.EXT_PNG;
                                var document = new Document(doc);

                                // Imagenes
                                byte[]? qrData = Graphic.GenerateSimpleQRByte(GetValueFromDS("COD_QR"), "QR_" + imgName, 2);
                                if (qrData != null)
                                {
                                    ImageData imageData = ImageDataFactory.Create(qrData);
                                    // Create layout image object and provide parameters. 
                                    Image image = new Image(imageData);
                                    image.ScaleToFit(80, 80);
                                    image.SetFixedPosition(526, 696);
                                    // This adds the image to the page
                                    document.Add(image);
                                }

                                string codBar = GetValueFromDS("BARRA1_DV");
                                byte[]? barData = Graphic.GenerateBarCodeByte(codBar, "BR_" + imgName, 1);
                                if (barData != null)
                                {
                                    ImageData imageData = ImageDataFactory.Create(barData);
                                    Image image = new Image(imageData);
                                    image.SetHeight(32);
                                    image.SetFixedPosition(284, 30);
                                    document.Add(image);
                                }

                                byte[]? charBytes = Graphic.CreateBarChartByte(dataList, promedio);
                                if (charBytes != null)
                                {
                                    ImageData imageData = ImageDataFactory.Create(charBytes);
                                    Image image = new Image(imageData);
                                    image.SetHeight(80);
                                    image.SetFixedPosition(28, 512);
                                    document.Add(image);
                                }

                                if (isPaid)
                                {
                                    //string pathImg = Util.GetDirFolderResources(Constants.DIR_TEMPLATES, Constants.TEMPLATE_IMG_PAGADO);
                                    //ImageData imageData = ImageDataFactory.Create(pathImg);
                                    //Image image = new Image(imageData);
                                    //image.SetHeight(112);
                                    //image.SetFixedPosition(352, 300);
                                    //document.Add(image);
                                }

                                ImageData logoData = ImageDataFactory.Create(Path.Combine(_rootPath, "Sources", "Images", Helper.GetCompanyName(Company) + "-logo.png")); ;
                                Image logo = new Image(logoData);
                                logo.SetHeight(20);
                                logo.SetFixedPosition(20, 752);
                                document.Add(logo);
                            }
                            catch (Exception ex)
                            {
                                //Log.Ex(ex);
                            }

                            form.FlattenFields();
                            doc.GetDocumentInfo().SetTitle(DocNumber);
                            doc.Close();

                            return msTemp.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Ex(ex);
            }

            return null;
        }

        private string GetValueFromDS(string name, bool header = true, bool isNumber = false)
        {
            string? value = string.Empty;
            if (header)
            {
                if (_header != null)
                {
                    try
                    {
                        value = _header.Rows[0][name].ToString();
                    }
                    catch (Exception ex)
                    {

                    }

                    if (string.IsNullOrEmpty(value) && isNumber)
                    {
                        value = "0";
                    }

                }
            }
            else
            {
                if (_detail != null)
                {
                    try
                    {
                        value = _detail.Rows[0][name].ToString();
                    }
                    catch (Exception ex)
                    {

                    }

                    if (string.IsNullOrEmpty(value) && isNumber)
                    {
                        value = "0";
                    }

                }
            }

            return value == null ? string.Empty : value;
        }
        public string GetValueFromDR(DataRow dr, string colName)
        {
            string? value = string.Empty;

            try
            {
                value = dr[colName].ToString();
            }
            catch (Exception ex)
            {

            }

            return value == null ? string.Empty : value;
        }
        public int GetIntValueFromDR(DataRow? dr, string colName)
        {
            string? value = "0";
            if (dr != null)
            {
                try
                {
                    value = dr[colName].ToString();

                    if (string.IsNullOrEmpty(value))
                    {
                        value = "0";
                    }
                }
                catch (Exception ex)
                {
                    value = "0";
                }
            }

            return int.Parse(value);
        }

        public long GetLongValueFromDR(DataRow? dr, string colName)
        {
            string? value = "0";
            if (dr != null)
            {
                try
                {
                    value = dr[colName].ToString();

                    if (string.IsNullOrEmpty(value))
                    {
                        value = "0";
                    }
                }
                catch (Exception ex)
                {
                    value = "0";
                }
            }

            return long.Parse(value);
        }

        public string GetValueFromJson(string json, string name, bool header = true, bool isNumber = false)
        {
            string? value = string.Empty;

            try
            {
                value = GetValueFromDS(name, header, isNumber);
            }
            catch (Exception ex)
            {

            }

            return value;
        }
    }
}
